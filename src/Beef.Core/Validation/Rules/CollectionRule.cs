﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        IValidator Validator { get; }

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
    /// Provides validation configuration for an item within a <see cref="CollectionRule{TEntity, TProperty}"/>.
    /// </summary>
    /// <typeparam name="TItemEntity">The item entity <see cref="Type"/>.</typeparam>
    public sealed class CollectionRuleItem<TItemEntity> : ICollectionRuleItem where TItemEntity : class
    {
        private bool _duplicateCheck = false;
        private IPropertyExpression _propertyExpression;
        private LText _duplicateText = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionRuleItem{TItemEntity}"/> class.
        /// </summary>
        /// <param name="validator">The corresponding item <see cref="Validator{TItemEntity}"/>.</param>
        public CollectionRuleItem(Validator<TItemEntity> validator = null)
        {
            Validator = validator;
        }

        /// <summary>
        /// Gets the corresponding item <see cref="IValidator"/>.
        /// </summary>
        IValidator ICollectionRuleItem.Validator => Validator;

        /// <summary>
        /// Gets or sets the corresponding item <see cref="Validator{TItemEntity}"/>.
        /// </summary>
        public Validator<TItemEntity> Validator { get; private set; }

        /// <summary>
        /// Gets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType => typeof(TItemEntity);

        /// <summary>
        /// Specifies that the collection is to be checked for duplicates using the item's <see cref="IUniqueKey"/> value.
        /// </summary>
        /// <param name="duplicateText">The duplicate text <see cref="LText"/> to be passed for the error message (default is to derive the text from the property itself where possible).</param>
        /// <returns>The <see cref="CollectionRuleItem{TItemEntity}"/> instance to support chaining/fluent.</returns>
        public CollectionRuleItem<TItemEntity> UniqueKeyDuplicateCheck(LText duplicateText = null)
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
                throw new InvalidOperationException($"A CollectionRuleItem ItemType '{ItemType.Name}' could not be constructed as an IUniqueKey to infer UniqueKeyProperties: {ex.Message}");
            }

            if (!uk.HasUniqueKey || uk.UniqueKeyProperties == null || uk.UniqueKeyProperties.Length == 0)
                throw new InvalidOperationException("A CollectionRule TProperty ItemType '{_itemType.Name}' must define the 'IUniqueKey.UniqueKeyProperties' to support CheckForDuplicates.");

            _duplicateText = duplicateText;
            if (_duplicateText == null && uk.UniqueKeyProperties.Length == 1)
                _duplicateText = new EntityReflectorArgs().GetReflector(ItemType).GetProperty(uk.UniqueKeyProperties[0]).PropertyExpression.Text;

            if (_duplicateText == null)
                _duplicateText = "Unique Key";

            _duplicateCheck = true;

            return this;
        }

        /// <summary>
        /// Specifies that the collection is to be checked for duplicates using the specified item property.
        /// </summary>
        /// <typeparam name="TItemProperty">The item property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the item property that is being duplicate checked.</param>
        /// <param name="duplicateText">The duplicate text <see cref="LText"/> to be passed for the error message (default is to derive the text from the property itself where possible).</param>
        /// <returns>The <see cref="CollectionRuleItem{TItemEntity}"/> instance to support chaining/fluent.</returns>
        public CollectionRuleItem<TItemEntity> DuplicateCheck<TItemProperty>(Expression<Func<TItemEntity, TItemProperty>> propertyExpression, LText duplicateText = null)
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
        void ICollectionRuleItem.DuplicateValidation(IPropertyContext context, IEnumerable items)
        {
            DuplicateValidation(context, (IEnumerable<TItemEntity>)items);
        }

        /// <summary>
        /// Performs the duplicate validation check.
        /// </summary>
        /// <param name="context">The <see cref="IPropertyContext"/>.</param>
        /// <param name="items">The items to duplicate check.</param>
        private void DuplicateValidation(IPropertyContext context, IEnumerable<TItemEntity> items)
        {
            if (!_duplicateCheck)
                return;

            if (_propertyExpression == null)
            {
                var dict = new Dictionary<UniqueKey, object>(new UniqueKeyComparer());
                foreach (var item in items.Cast<IUniqueKey>())
                {
                    if (dict.ContainsKey(item.UniqueKey))
                    {
                        if (item.UniqueKey.Args.Length == 1)
                            context.CreateErrorMessage(ValidatorStrings.DuplicateValueFormat, _duplicateText, item.UniqueKey.Args[0]);
                        else
                            context.CreateErrorMessage(ValidatorStrings.DuplicateValue2Format, _duplicateText);

                        return;
                    }

                    dict.Add(item.UniqueKey, null);
                }
            }
            else
            {
                var dict = new Dictionary<object, object>();
                foreach (var item in items)
                {
                    var val = _propertyExpression.GetValue(item);
                    if (dict.ContainsKey(_propertyExpression.GetValue(item)))
                    {
                        context.CreateErrorMessage(ValidatorStrings.DuplicateValueFormat, _duplicateText, val);
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
        where TProperty : IEnumerable
    {
        private ICollectionRuleItem _item;
        private readonly Type _itemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionRule{TEntity, TProperty}"/> class.
        /// </summary>
        public CollectionRule()
        {
            _itemType = ComplexTypeReflector.GetItemType(typeof(TProperty));
        }

        /// <summary>
        /// Gets or sets the minimum count;
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the collection item validation configuration.
        /// </summary>
        public ICollectionRuleItem Item
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
            return context.Parent.ShallowValidation ? false : base.Check(context);
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, IEnumerable}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));
            if (context.Value == null)
                return;

            // Iterate through the collection validating each of the items.
            var hasItemErrors = false;
            int i = 0;
            foreach (var item in context.Value)
            {
                // Create the context args.
                var args = context.CreateValidationArgs();
                args.FullyQualifiedEntityName += "[" + i + "]";
                args.FullyQualifiedJsonEntityName += "[" + i + "]";
                i++;

                // Validate and merge.
                if (Item?.Validator != null)
                {
                    var r = Item.Validator.Validate(item, args);
                    context.MergeResult(r);
                    if (r.HasErrors)
                        hasItemErrors = true;
                }
            }

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