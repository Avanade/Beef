// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Enables the validation configuration for a value within a <see cref="DictionaryRule{TEntity, TProperty}"/>.
    /// </summary>
    public interface IDictionaryRuleValue
    {
        /// <summary>
        /// Gets the corresponding item <see cref="IValidator"/>.
        /// </summary>
        IValidator? Validator { get; }

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
    /// Provides the means to create a <see cref="DictionaryRuleValue{TKey, TValueEntity}"/> instance.
    /// </summary>
    public static class DictionaryRuleValue
    {
        /// <summary>
        /// Create an instance of the <see cref="DictionaryRuleValue{TKey, TValueEntity}"/> class with no <see cref="Validator"/>.
        /// </summary>
        /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValueEntity">The value entity <see cref="Type"/>.</typeparam>
        /// <returns>The <see cref="DictionaryRuleValue{TKey, TValueEntity}"/>.</returns>
        public static DictionaryRuleValue<TKey, TValueEntity> Create<TKey, TValueEntity>() where TValueEntity : class => new DictionaryRuleValue<TKey, TValueEntity>(null);

        /// <summary>
        /// Create an instance of the <see cref="DictionaryRuleValue{TKey, TValueEntity}"/> class with a corresponding <paramref name="validator"/>.
        /// </summary>
        /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValueEntity">The value entity <see cref="Type"/>.</typeparam>
        /// <param name="validator">The corresponding value <see cref="IValidator{TValueEntity}"/>.</param>
        /// <returns>The <see cref="DictionaryRuleValue{TKey, TValueEntity}"/>.</returns>
        public static DictionaryRuleValue<TKey, TValueEntity> Create<TKey, TValueEntity>(IValidator<TValueEntity> validator) where TValueEntity : class
            => new(validator ?? throw new ArgumentNullException(nameof(validator)));

        /// <summary>
        /// Create an instance of the <see cref="DictionaryRuleValue{TKey, TValueEntity}"/> class leveraging the underlying <see cref="ExecutionContext.GetService{T}(bool)">service provider</see> to get the instance.
        /// </summary>
        /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValueEntity">The value entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The value validator <see cref="Type"/>.</typeparam>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>; defaults to <see cref="ExecutionContext.ServiceProvider"/> where not specified.</param>
        /// <returns>The <see cref="DictionaryRuleValue{TKey, TValueEntity}"/>.</returns>
        public static DictionaryRuleValue<TKey, TValueEntity> Create<TKey, TValueEntity, TValidator>(IServiceProvider? serviceProvider = null) where TValueEntity : class where TValidator : IValidator<TValueEntity>
            => new(serviceProvider == null
                ? ExecutionContext.GetService<TValidator>(throwExceptionOnNull: true)!
                : (serviceProvider.GetService<TValidator>() ?? throw new InvalidOperationException($"Attempted to get service '{typeof(TValidator).FullName}' but null was returned; this would indicate that the service has not been configured correctly.")));
    }

    /// <summary>
    /// Provides validation configuration for a value within a <see cref="DictionaryRule{TEntity, TProperty}"/>.
    /// </summary>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValueEntity">The value entity <see cref="Type"/>.</typeparam>
    public sealed class DictionaryRuleValue<TKey, TValueEntity> : IDictionaryRuleValue where TValueEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryRuleValue{TKey, TValueEntity}"/> class with a corresponding <paramref name="validator"/>.
        /// </summary>
        /// <param name="validator">The corresponding value <see cref="IValidator{TValueEntity}"/>.</param>
        internal DictionaryRuleValue(IValidator<TValueEntity>? validator) => Validator = validator;

        /// <summary>
        /// Gets the corresponding value <see cref="IValidator"/>.
        /// </summary>
        IValidator? IDictionaryRuleValue.Validator => Validator;

        /// <summary>
        /// Gets the corresponding value <see cref="IValidator{TValueEntity}"/>.
        /// </summary>
        public IValidator<TValueEntity>? Validator { get; private set; }

        /// <summary>
        /// Gets the item <see cref="Type"/>.
        /// </summary>
        public Type ItemType => typeof(KeyValuePair<TKey, TValueEntity>);

        /// <summary>
        /// Gets the key <see cref="Type"/>.
        /// </summary>
        public Type KeyType => typeof(TKey);

        /// <summary>
        /// Gets the value <see cref="Type"/>.
        /// </summary>
        public Type ValueType => typeof(TValueEntity);
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
        private readonly Type _valueType;
        private IDictionaryRuleValue? _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryRule{TEntity, TProperty}"/> class.
        /// </summary>
        public DictionaryRule()
        {
            _valueType = ComplexTypeReflector.GetItemType(typeof(TProperty));
        }

        /// <summary>
        /// Indicates whether the underlying dictionary key can be null.
        /// </summary>
        public bool AllowNullKeys { get; set; }

        /// <summary>
        /// Indicates whether the underlying dictionary value can be null.
        /// </summary>
        public bool AllowNullItems { get; set; }

        /// <summary>
        /// Gets or sets the minimum count;
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Gets or sets the dictionary value validation configuration.
        /// </summary>
        public IDictionaryRuleValue? Value
        {
            get => _value;

            set
            {
                if (value == null)
                {
                    _value = value;
                    return;
                }

                if (_valueType != value.ValueType)
                    throw new ArgumentException($"A DictionaryRule TProperty Value type '{_valueType.Name}' must be the same as the Value {value.ValueType.Name}");

                _value = value;
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

                if (!AllowNullItems && de.Value == null)
                    hasNullValue = true;

                // Validate and merge.
                if (de.Value != null && Value?.Validator != null)
                {
                    var r = await Value.Validator.ValidateAsync(de.Value, args).ConfigureAwait(false);
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