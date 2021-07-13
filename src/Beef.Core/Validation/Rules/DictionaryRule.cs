// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Enables the validation configuration for an item (<see cref="KeyValuePair"/>) within a <see cref="DictionaryRule{TEntity, TProperty}"/>.
    /// </summary>
    public interface IDictionaryRuleItem
    {
        /// <summary>
        /// Gets the corresponding key <see cref="IValidator"/>.
        /// </summary>
        IValidator? KeyValidator { get; }

        /// <summary>
        /// Gets the corresponding value <see cref="IValidator"/>.
        /// </summary>
        IValidator? ValueValidator { get; }

        /// <summary>
        /// Gets the item <see cref="Type"/>.
        /// </summary>
        Type ItemType { get; }

        /// <summary>
        /// Gets the key <see cref="Type"/>.
        /// </summary>
        Type KeyType { get; }

        /// <summary>
        /// Gets the value <see cref="Type"/>.
        /// </summary>
        Type ValueType { get; }
    }

    /// <summary>
    /// Provides the means to create a <see cref="DictionaryRuleItem{TKey, TValue}"/> instance.
    /// </summary>
    public static class DictionaryRuleItem
    {
        /// <summary>
        /// Create an instance of the <see cref="DictionaryRuleItem{TKey, TValue}"/> class.
        /// </summary>
        /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The corresponding value <see cref="IValidator{TValue}"/>.</param>
        /// <param name="value">The corresponding value <see cref="IValidator{TValue}"/>.</param>
        /// <returns>The <see cref="DictionaryRuleItem{TKey, TValue}"/>.</returns>
        public static DictionaryRuleItem<TKey, TValue> Create<TKey, TValue>(IValidator<TKey>? key = null, IValidator<TValue>? value = null) => new(key, value);
    }

    /// <summary>
    /// Provides validation configuration for an item (<see cref="KeyValuePair"/>) within a <see cref="DictionaryRule{TEntity, TProperty}"/>.
    /// </summary>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
    public sealed class DictionaryRuleItem<TKey, TValue> : IDictionaryRuleItem 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryRuleItem{TKey, TValue}"/> class with a corresponding <paramref name="valueValidator"/>.
        /// </summary>
        /// <param name="keyValidator">The corresponding key <see cref="IValidator{TKey}"/>.</param>
        /// <param name="valueValidator">The corresponding value <see cref="IValidator{TValue}"/>.</param>
        /// <remarks><i>Note:</i> the underlying <see cref="PropertyRuleBase"/> properties <see cref="PropertyRuleBase.Name"/>, <see cref="PropertyRuleBase.JsonName"/> and <see cref="PropertyRuleBase.Text"/> will be automatically updated
        /// (overridden) to <see cref="Validator.KeyNameDefault"/> when passing the <paramref name="keyValidator"/> (where the passed values are currently <see cref="Validator.ValueNameDefault"/>).</remarks>
        internal DictionaryRuleItem(IValidator<TKey>? keyValidator, IValidator<TValue>? valueValidator)
        {
            KeyValidator = keyValidator;
            ValueValidator = valueValidator;
        }

        /// <summary>
        /// Gets the corresponding key <see cref="IValidator"/>.
        /// </summary>
        IValidator? IDictionaryRuleItem.KeyValidator => KeyValidator;

        /// <summary>
        /// Gets the corresponding value <see cref="IValidator{TValue}"/>.
        /// </summary>
        public IValidator<TKey>? KeyValidator { get; private set; }

        /// <summary>
        /// Gets the corresponding value <see cref="IValidator"/>.
        /// </summary>
        IValidator? IDictionaryRuleItem.ValueValidator => ValueValidator;

        /// <summary>
        /// Gets the corresponding value <see cref="IValidator{TValue}"/>.
        /// </summary>
        public IValidator<TValue>? ValueValidator { get; private set; }

        /// <summary>
        /// Gets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType => typeof(KeyValuePair<TKey, TValue>);

        /// <summary>
        /// Gets the key <see cref="Type"/>.
        /// </summary>
        public Type KeyType => typeof(TKey);

        /// <summary>
        /// Gets the value <see cref="Type"/>.
        /// </summary>
        public Type ValueType => typeof(TValue);
    }

    /// <summary>
    /// Provides dictionary validation including <see cref="MinCount"/> and <see cref="MaxCount"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The dictionary property <see cref="Type"/>.</typeparam>
    public class DictionaryRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
        where TProperty : IDictionary?
    {
        private readonly Type _keyType;
        private readonly Type _valueType;
        private IDictionaryRuleItem? _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryRule{TEntity, TProperty}"/> class.
        /// </summary>
        public DictionaryRule()
        {
            var (kt, vt) = ComplexTypeReflector.GetDictionaryType(typeof(TProperty));
            _keyType = kt!;
            _valueType = vt!;
        }

        /// <summary>
        /// Indicates whether the underlying dictionary key can be null.
        /// </summary>
        public bool AllowNullKeys { get; set; }

        /// <summary>
        /// Indicates whether the underlying dictionary value can be null.
        /// </summary>
        public bool AllowNullValues { get; set; }

        /// <summary>
        /// Gets or sets the minimum count;
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the dictionary item validation configuration.
        /// </summary>
        public IDictionaryRuleItem? Item
        {
            get => _item;

            set
            {
                if (value == null)
                {
                    _item = value;
                    return;
                }

                if (_keyType != value.KeyType)
                    throw new ArgumentException($"A DictionaryRule TProperty KeyType '{_keyType.Name}' must be the same as the Key {value.KeyType.Name}.");

                if (_valueType != value.ValueType)
                    throw new ArgumentException($"A DictionaryRule TProperty ValueType '{_valueType.Name}' must be the same as the Value {value.ValueType.Name}.");

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

            // Iterate through the dictionary validating each of the items.
            var i = 0;
            var hasNullKey = false;
            var hasNullValue = false;
            foreach (var item in context.Value)
            {
                var de = (DictionaryEntry)item;

                // Create the context args.
                var args = context.CreateValidationArgs();
                args.FullyQualifiedEntityName += $"[{de.Key}]";
                args.FullyQualifiedJsonEntityName += $"[{de.Key}]";
                i++;

                if (!AllowNullKeys && de.Key == null)
                    hasNullKey = true;

                if (!AllowNullValues && de.Value == null)
                    hasNullValue = true;

                // Validate and merge.
                if (de.Key != null && Item?.KeyValidator != null)
                {
                    var r = await Item.KeyValidator.ValidateAsync(de.Key, args).ConfigureAwait(false);
                    context.MergeResult(r);
                }

                if (de.Value != null && Item?.ValueValidator != null)
                {
                    var r = await Item.ValueValidator.ValidateAsync(de.Value, args).ConfigureAwait(false);
                    context.MergeResult(r);
                }
            }

            if (hasNullKey)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.DictionaryNullKeyFormat);

            if (hasNullValue)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.DictionaryNullValueFormat);

            // Check the length/count.
            if (i < MinCount)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MinCountFormat, MinCount);
            else if (MaxCount.HasValue && i > MaxCount.Value)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MaxCountFormat, MaxCount);
        }
    }
}