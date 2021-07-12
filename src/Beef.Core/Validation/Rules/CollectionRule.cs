// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Enables the validation configuration for an item within a <see cref="CollectionRule{TEntity, TProperty}"/>.
    /// </summary>
    public interface ICollectionRuleItem
    {
        /// <summary>
        /// Gets the corresponding item <see cref="IValidator"/>.
        /// </summary>
        IValidator? ItemValidator { get; }

        /// <summary>
        /// Gets the item <see cref="Type"/>.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Performs the duplicate validation check.
        /// </summary>
        /// <param name="context">The <see cref="IPropertyContext"/>.</param>
        /// <param name="items">The items to duplicate check.</param>
        void DuplicateValidation(IPropertyContext context, IEnumerable items);
    }

    /// <summary>
    /// Provides the means to create a <see cref="CollectionRuleItem{TItemEntity}"/> instance.
    /// </summary>
    public static class CollectionRuleItem
    {
        /// <summary>
        /// Create an instance of the <see cref="CollectionRuleItem{TItem}"/> class with no <see cref="Validator"/>.
        /// </summary>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <returns>The <see cref="CollectionRuleItem{TItem}"/>.</returns>
        public static CollectionRuleItem<TItem> Create<TItem>() => new(null);

        /// <summary>
        /// Create an instance of the <see cref="CollectionRuleItem{TItem}"/> class with a corresponding <paramref name="validator"/>.
        /// </summary>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="validator">The corresponding item <see cref="IValidator{TItem}"/>.</param>
        /// <returns>The <see cref="CollectionRuleItem{TItem}"/>.</returns>
        public static CollectionRuleItem<TItem> Create<TItem>(IValidator<TItem> validator) => new(validator ?? throw new ArgumentNullException(nameof(validator)));

        /// <summary>
        /// Create an instance of the <see cref="CollectionRuleItem{TItem}"/> class leveraging the underlying <see cref="ExecutionContext.GetService{T}(bool)">service provider</see> to get the instance.
        /// </summary>
        /// <typeparam name="TItem">The item entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The item validator <see cref="Type"/>.</typeparam>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>; defaults to <see cref="ExecutionContext.ServiceProvider"/> where not specified.</param>
        /// <returns>The <see cref="CollectionRuleItem{TItem}"/>.</returns>
        public static CollectionRuleItem<TItem> Create<TItem, TValidator>(IServiceProvider? serviceProvider = null) where TValidator : IValidator<TItem> => new(Validator.Create<TValidator>(serviceProvider));
    }

    /// <summary>
    /// Provides validation configuration for an item within a <see cref="CollectionRule{TEntity, TProperty}"/>.
    /// </summary>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public sealed class CollectionRuleItem<TItem> : ICollectionRuleItem
    {
        private bool _duplicateCheck = false;
        private IPropertyExpression? _propertyExpression;
        private LText? _duplicateText = null;
        private bool _ignoreWhereUniqueKeyIsInitial = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionRuleItem{TItem}"/> class with a corresponding <paramref name="validator"/>.
        /// </summary>
        /// <param name="validator">The corresponding item <see cref="IValidator{TItem}"/>.</param>
        internal CollectionRuleItem(IValidator<TItem>? validator) => ItemValidator = validator;

        /// <summary>
        /// Gets the corresponding item <see cref="IValidator"/>.
        /// </summary>
        IValidator? ICollectionRuleItem.ItemValidator => ItemValidator;

        /// <summary>
        /// Gets the corresponding item <see cref="IValidator{TItemEntity}"/>.
        /// </summary>
        public IValidator<TItem>? ItemValidator { get; private set; }

        /// <summary>
        /// Gets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType => typeof(TItem);

        /// <summary>
        /// Specifies that the collection is to be checked for duplicates using the item's <see cref="IUniqueKey"/> value.
        /// </summary>
        /// <param name="duplicateText">The duplicate text <see cref="LText"/> to be passed for the error message (default is to derive the text from the property itself where possible).</param>
        /// <returns>The <see cref="CollectionRuleItem{TItemEntity}"/> instance to support chaining/fluent.</returns>
        public CollectionRuleItem<TItem> UniqueKeyDuplicateCheck(LText? duplicateText = null)
        {
            if (_duplicateCheck)
                throw new InvalidOperationException("A DuplicateCheck or UniqueKeyDuplicateCheck can only be specified once.");

            if (ItemType.GetInterface(typeof(IUniqueKey).Name) == null)
                throw new InvalidOperationException("A CollectionRuleItem ItemType '{_itemType.Name}' must implement 'IUniqueKey' to support UniqueKeyDuplicateCheck.");

            IUniqueKey uk;
            try
            {
                uk = (IUniqueKey)Activator.CreateInstance(ItemType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"A CollectionRuleItem ItemType '{ItemType.Name}' could not be constructed as an IUniqueKey is required to infer UniqueKeyProperties: {ex.Message}");
            }

            if (uk.UniqueKeyProperties == null || uk.UniqueKeyProperties.Length == 0)
                throw new InvalidOperationException("A CollectionRule TProperty ItemType '{_itemType.Name}' must define one or more 'IUniqueKey.UniqueKeyProperties' to support CheckForDuplicates.");

            _duplicateText = duplicateText;
            if (_duplicateText == null && uk.UniqueKeyProperties.Length == 1)
                _duplicateText = new EntityReflectorArgs().GetReflector(ItemType).GetProperty(uk.UniqueKeyProperties[0]).PropertyExpression.Text;

            if (_duplicateText == null)
                _duplicateText = "Unique Key";

            _duplicateCheck = true;
            _ignoreWhereUniqueKeyIsInitial = false;

            return this;
        }

        /// <summary>
        /// Specifies that the collection is to be checked for duplicates using the item's <see cref="IUniqueKey"/> value with an option to <paramref name="ignoreWhereUniqueKeyIsInitial"/>.
        /// </summary>
        /// <param name="ignoreWhereUniqueKeyIsInitial">Indicates whether to ignore the <see cref="UniqueKey"/> where <see cref="UniqueKey.IsInitial"/>; useful where the unique key will be generated by the underlying data source on create.</param>
        /// <param name="duplicateText">The duplicate text <see cref="LText"/> to be passed for the error message (default is to derive the text from the property itself where possible).</param>
        /// <returns>The <see cref="CollectionRuleItem{TItemEntity}"/> instance to support chaining/fluent.</returns>
        public CollectionRuleItem<TItem> UniqueKeyDuplicateCheck(bool ignoreWhereUniqueKeyIsInitial, LText? duplicateText = null)
        {
            UniqueKeyDuplicateCheck(duplicateText);
            _ignoreWhereUniqueKeyIsInitial = ignoreWhereUniqueKeyIsInitial;
            return this;
        }

        /// <summary>
        /// Specifies that the collection is to be checked for duplicates using the specified item property.
        /// </summary>
        /// <typeparam name="TItemProperty">The item property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the item property that is being duplicate checked.</param>
        /// <param name="duplicateText">The duplicate text <see cref="LText"/> to be passed for the error message (default is to derive the text from the property itself where possible).</param>
        /// <returns>The <see cref="CollectionRuleItem{TItemEntity}"/> instance to support chaining/fluent.</returns>
        public CollectionRuleItem<TItem> DuplicateCheck<TItemProperty>(Expression<Func<TItem, TItemProperty>> propertyExpression, LText? duplicateText = null)
        {
            if (_duplicateCheck)
                throw new InvalidOperationException("A DuplicateCheck or UniqueKeyDuplicateCheck can only be specified once.");

            _propertyExpression = PropertyExpression.Create(Check.NotNull(propertyExpression, nameof(propertyExpression)), true);
            _duplicateText = duplicateText ?? _propertyExpression.Text;
            _duplicateCheck = true;

            return this;
        }

        /// <summary>
        /// Performs the duplicate validation check.
        /// </summary>
        /// <param name="context">The <see cref="IPropertyContext"/>.</param>
        /// <param name="items">The items to duplicate check.</param>
        void ICollectionRuleItem.DuplicateValidation(IPropertyContext context, IEnumerable items) => DuplicateValidation(context, (IEnumerable<TItem>)items);

        /// <summary>
        /// Performs the duplicate validation check.
        /// </summary>
        /// <param name="context">The <see cref="IPropertyContext"/>.</param>
        /// <param name="items">The items to duplicate check.</param>
        private void DuplicateValidation(IPropertyContext context, IEnumerable<TItem> items)
        {
            if (!_duplicateCheck)
                return;

            if (_propertyExpression == null)
            {
                var dict = new Dictionary<UniqueKey, object?>(new UniqueKeyComparer());
                foreach (var item in items.Where(x => x != null).Cast<IUniqueKey>())
                {
                    if (_ignoreWhereUniqueKeyIsInitial && (item.UniqueKey == null || item.UniqueKey.IsInitial))
                        continue;

                    if (dict.ContainsKey(item.UniqueKey))
                    {
                        if (item.UniqueKey.Args.Length == 1)
                            context.CreateErrorMessage(ValidatorStrings.DuplicateValueFormat, _duplicateText!, item.UniqueKey.Args[0]);
                        else
                            context.CreateErrorMessage(ValidatorStrings.DuplicateValue2Format, _duplicateText!);

                        return;
                    }

                    dict.Add(item.UniqueKey, null);
                }
            }
            else
            {
                var dict = new Dictionary<object?, object?>();
                foreach (var item in items.Where(x => x != null))
                {
                    var val = _propertyExpression.GetValue(item!);
                    if (dict.ContainsKey(val))
                    {
                        context.CreateErrorMessage(ValidatorStrings.DuplicateValueFormat, _duplicateText!, val!);
                        return;
                    }

                    dict.Add(val, null);
                }
            }
        }
    }

    /// <summary>
    /// Provides collection validation including <see cref="MinCount"/> and <see cref="MaxCount"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The collection property <see cref="Type"/>.</typeparam>
    public class CollectionRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
        where TProperty : IEnumerable?
    {
        private readonly Type _itemType;
        private ICollectionRuleItem? _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionRule{TEntity, TProperty}"/> class.
        /// </summary>
        public CollectionRule() => _itemType = ComplexTypeReflector.GetItemType(typeof(TProperty));

        /// <summary>
        /// Gets or sets the minimum count;
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Indicates whether the underlying collection items can be null.
        /// </summary>
        public bool AllowNullItems { get; set; }

        /// <summary>
        /// Gets or sets the collection item validation configuration.
        /// </summary>
        public ICollectionRuleItem? Item
        {
            get => _item;

            set
            {
                if (value == null)
                {
                    _item = value;
                    return;
                }

                if (_itemType != value.ItemType)
                    throw new ArgumentException($"A CollectionRule TProperty ItemType '{_itemType.Name}' must be the same as the Item {value.ItemType.Name}");

                _item = value;
            }
        }

        /// <summary>
        /// Overrides the <b>Check</b> method and will not validate where performing a shallow validation.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        public override bool Check(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));
            return !context.Parent.ShallowValidation && base.Check(context);
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, IEnumerable}"/>.</param>
        public override async Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));
            if (context.Value == null)
                return;

            // Iterate through the collection validating each of the items.
            var i = 0;
            var hasNullItem = false;
            var hasItemErrors = false;
            foreach (var item in context.Value)
            {
                // Create the context args.
                var args = context.CreateValidationArgs();
                args.FullyQualifiedEntityName += "[" + i + "]";
                args.FullyQualifiedJsonEntityName += "[" + i + "]";
                i++;

                if (!AllowNullItems && item == null)
                    hasNullItem = true;

                // Validate and merge.
                if (item != null && Item?.ItemValidator != null)
                {
                    var r = await Item.ItemValidator.ValidateAsync(item, args).ConfigureAwait(false);
                    context.MergeResult(r);
                    if (r.HasErrors)
                        hasItemErrors = true;
                }
            }

            if (hasNullItem)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.CollectionNullItemFormat);

            // Check the length/count.
            if (i < MinCount)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MinCountFormat, MinCount);
            else if (MaxCount.HasValue && i > MaxCount.Value)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MaxCountFormat, MaxCount);

            // Check for duplicates.
            if (!hasItemErrors)
                Item?.DuplicateValidation(context, context.Value);
        }
    }
}