// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Provides dictionary validation.
    /// </summary>
    /// <typeparam name="TDict">The dictionary <see cref="Type"/>.</typeparam>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
    public class DictionaryValidator<TDict, TKey, TValue> : ValidatorBase<TDict>
        where TDict : class, IDictionary<TKey, TValue>
    {
        private IDictionaryRuleItem? _item;
        private Func<ValidationContext<TDict>, Task>? _additionalAsync;

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
        /// Gets or sets the collection item validation configuration.
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

                if (typeof(TKey) != value.KeyType)
                    throw new ArgumentException($"A CollectionRule TProperty Key Type '{typeof(TKey).Name}' must be the same as the Key {value.KeyType.Name}.");

                if (typeof(TValue) != value.ValueType)
                    throw new ArgumentException($"A CollectionRule TProperty Value Type '{typeof(TValue).Name}' must be the same as the Value {value.ValueType.Name}.");

                _item = value;
            }
        }

        /// <summary>
        /// Gets or sets the friendly text name used in validation messages.
        /// </summary>
        /// <remarks>Defaults to the <see cref="ValidationArgs.FullyQualifiedEntityName"/> formatted as sentence case where specified; otherwise, 'Value'.</remarks>
        public LText? Text { get; set; }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public override async Task<ValidationContext<TDict>> ValidateAsync(TDict value, ValidationArgs? args = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            args ??= new ValidationArgs();
            if (string.IsNullOrEmpty(args.FullyQualifiedEntityName))
                args.FullyQualifiedEntityName = Validator.ValueNameDefault;

            if (string.IsNullOrEmpty(args.FullyQualifiedEntityName))
                args.FullyQualifiedJsonEntityName = Validator.ValueNameDefault;

            var context = new ValidationContext<TDict>(value, args);

            var i = 0;
            var hasNullKey = false;
            var hasNullValue = false;
            foreach (var item in value)
            {
                i++;

                if (!AllowNullKeys && item.Key == null)
                    hasNullKey = true;

                if (!AllowNullValues && item.Value == null)
                    hasNullValue = true;

                if (Item?.KeyValidator == null && Item?.ValueValidator == null)
                    continue;

                // Validate and merge.
                var name = $"[{item.Key}]";

                if (item.Key != null && Item?.KeyValidator != null)
                {
                    var kc = new PropertyContext<TDict, KeyValuePair<TKey, TValue>>(context, item, name, name);
                    var ka = kc.CreateValidationArgs();
                    var kr = await Item.KeyValidator.ValidateAsync(item.Key, ka).ConfigureAwait(false);
                    context.MergeResult(kr);
                }

                if (item.Value != null && Item?.ValueValidator != null)
                {
                    var vc = new PropertyContext<TDict, KeyValuePair<TKey, TValue>>(context, item, name, name);
                    var va = vc.CreateValidationArgs();
                    var vr = await Item.ValueValidator.ValidateAsync(item.Value, va).ConfigureAwait(false);
                    context.MergeResult(vr);
                }
            }

            var text = new Lazy<LText>(() => Text ?? Beef.StringConversion.ToSentenceCase(args?.FullyQualifiedEntityName) ?? Validator.ValueNameDefault);
            if (hasNullKey)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.DictionaryNullKeyFormat, new object?[] { text.Value, null });

            if (hasNullValue)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.DictionaryNullValueFormat, new object?[] { text.Value, null });

            // Check the length/count.
            if (i < MinCount)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.MinCountFormat, new object?[] { text.Value, null, MinCount });
            else if (MaxCount.HasValue && i > MaxCount.Value)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.MaxCountFormat, new object?[] { text.Value, null, MaxCount });

            await OnValidateAsync(context).ConfigureAwait(false);
            if (_additionalAsync != null)
                await _additionalAsync(context).ConfigureAwait(false);

            return context;
        }

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added by the inheriting classes.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/>.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        protected virtual Task OnValidateAsync(ValidationContext<TDict> context) => Task.CompletedTask;

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added.
        /// </summary>
        /// <param name="additionalAsync">The asynchronous function to invoke.</param>
        /// <returns>The <see cref="DictionaryValidator{TColl, TKey, TValue}"/>.</returns>
        public DictionaryValidator<TDict, TKey, TValue> Additional(Func<ValidationContext<TDict>, Task> additionalAsync)
        {
            Check.NotNull(additionalAsync, nameof(additionalAsync));

            if (_additionalAsync != null)
                throw new InvalidOperationException("Additional can only be defined once for a DictionaryValidator.");

            _additionalAsync = additionalAsync;
            return this;
        }
    }
}