// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Provides collection validation.
    /// </summary>
    /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public class CollectionValidator<TColl, TItem> : ValidatorBase<TColl>
        where TColl : class, IEnumerable<TItem>
    {
        private ICollectionRuleItem? _item;
        private Func<ValidationContext<TColl>, Task>? _additionalAsync;

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

                if (typeof(TItem) != value.ItemType)
                    throw new ArgumentException($"A CollectionRule TProperty ItemType '{typeof(TItem).Name}' must be the same as the Item {value.ItemType.Name}");

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
        public override async Task<ValidationContext<TColl>> ValidateAsync(TColl value, ValidationArgs? args = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            args ??= new ValidationArgs();
            if (string.IsNullOrEmpty(args.FullyQualifiedEntityName))
                args.FullyQualifiedEntityName = Validator.ValueNameDefault;

            if (string.IsNullOrEmpty(args.FullyQualifiedEntityName))
                args.FullyQualifiedJsonEntityName = Validator.ValueNameDefault;

            var context = new ValidationContext<TColl>(value, args);

            var i = 0;
            var hasNullItem = false;
            var hasItemErrors = false;
            foreach (var item in value)
            {
                if (!AllowNullItems && item == null)
                    hasNullItem = true;

                // Validate and merge.
                if (item != null && Item?.ItemValidator != null)
                {
                    var name = $"[{i}]";
                    var ic = new PropertyContext<TColl, TItem>(context, item, name, name);
                    var ia = ic.CreateValidationArgs();
                    var ir = await Item.ItemValidator.ValidateAsync(item, ia).ConfigureAwait(false);
                    context.MergeResult(ir);
                    if (ir.HasErrors)
                        hasItemErrors = true;
                }

                i++;
            }

            var text = new Lazy<LText>(() => Text ?? Beef.StringConversion.ToSentenceCase(args?.FullyQualifiedEntityName) ?? StringConversion.ToSentenceCase(Validator.ValueNameDefault)!);
            if (hasNullItem)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.CollectionNullItemFormat, new object?[] { text.Value, null });

            // Check the length/count.
            if (i < MinCount)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.MinCountFormat, new object?[] { text.Value, null, MinCount });
            else if (MaxCount.HasValue && i > MaxCount.Value)
                context.AddMessage(Entities.MessageType.Error, ValidatorStrings.MaxCountFormat, new object?[] { text.Value, null, MaxCount });

            // Check for duplicates.
            if (!hasItemErrors && Item != null)
            {
                var pctx = new PropertyContext<TColl, TColl>(text.Value, context, value);
                Item.DuplicateValidation(pctx, context.Value);
            }

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
        protected virtual Task OnValidateAsync(ValidationContext<TColl> context) => Task.CompletedTask;

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added.
        /// </summary>
        /// <param name="additionalAsync">The asynchronous function to invoke.</param>
        /// <returns>The <see cref="CollectionValidator{TColl, TItem}"/>.</returns>
        public CollectionValidator<TColl, TItem> Additional(Func<ValidationContext<TColl>, Task> additionalAsync)
        {
            Check.NotNull(additionalAsync, nameof(additionalAsync));

            if (_additionalAsync != null)
                throw new InvalidOperationException("Additional can only be defined once for a CollectionValidator.");

            _additionalAsync = additionalAsync;
            return this;
        }
    }
}