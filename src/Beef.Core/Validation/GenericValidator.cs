// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Enables a generic validator for the <see cref="ValueType"/>.
    /// </summary>
    public interface IGenericValidator : IValidator
    {
        /// <summary>
        /// Override the names and text for the underlying rules where they currently have default values.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jsonName">The JSON name.</param>
        /// <param name="text">The friendly text.</param>
        void OverrideNamesAndTextForRules(string name, string jsonName, LText text);

        /// <summary>
        /// Create a <see cref="ValidationValue{T}"/> for the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The corresponding <see cref="ValidationValue{T}"/>.</returns>
        object CreateValidationValue(object value);
    }

    /// <summary>
    /// Provides generic value validation.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class GenericValidator<T> : ValidatorBase<ValidationValue<T>>, IGenericValidator, IValidator<T>
    {
        private Func<ValidationContext<ValidationValue<T>>, Task>? _additionalAsync;

        /// <summary>
        /// Gets the key <see cref="Type"/>.
        /// </summary>
        Type IValidator.ValueType => typeof(T);

        /// <summary>
        /// Override the names and text for the underlying rules where they currently have default values.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="jsonName">The JSON name.</param>
        /// <param name="text">The friendly text.</param>
        void IGenericValidator.OverrideNamesAndTextForRules(string name, string jsonName, LText text)
        {
            foreach (var r in Rules.OfType<PropertyRuleBase>())
            {
                if (r.Name == Validator.ValueNameDefault)
                    r.Name = name;

                if (r.JsonName == Validator.ValueNameDefault)
                    r.JsonName = jsonName;

                if (r.Text == StringConversion.ToSentenceCase(Validator.ValueNameDefault))
                    r.Text = text;
            }
        }

        /// <summary>
        /// Create a <see cref="ValidationValue{T}"/> for the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The corresponding <see cref="ValidationValue{T}"/>.</returns>
        object IGenericValidator.CreateValidationValue(object value) => new ValidationValue<T>(null, (T)value);

        /// <summary>
        /// Adds a <see cref="PropertyRule{TEntity, TProperty}"/> to the validator.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public override PropertyRule<ValidationValue<T>, TProperty> Property<TProperty>(Expression<Func<ValidationValue<T>, TProperty>> propertyExpression) => throw new NotSupportedException("Please use the Rule method as there is no direct property access.");

        /// <summary>
        /// Adds a <see cref="PropertyRule{TEntity, TProperty}"/> for the <i>value</i> to the validator.
        /// </summary>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRule<ValidationValue<T>, T> Rule()
        {
            PropertyRule<ValidationValue<T>, T> rule = new(x => x.Value);
            Rules.Add(rule);
            return rule;
        }

        /// <summary>
        /// Adds a <see cref="PropertyRule{TEntity, TProperty}"/> for the <i>value</i> to the validator enabling additional configuration via the specified <paramref name="rule"/> action.
        /// </summary>
        /// <param name="rule">The action to act on the created <see cref="PropertyRule{TEntity, TProperty}"/>.</param>
        /// <returns>The <see cref="GenericValidator{T}"/>.</returns>
        public GenericValidator<T> Rule(Action<PropertyRule<ValidationValue<T>, T>>? rule)
        {
            rule?.Invoke(Rule());
            return this;
        }

        /// <summary>
        /// Validate the <paramref name="value"/> with the specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public async Task<ValidationContext<T>> ValidateAsync(T value, ValidationArgs? args = null)
        {
            var vc = await ValidateAsync(new ValidationValue<T>(null, value), args ??= new ValidationArgs()).ConfigureAwait(false);

            var context = new ValidationContext<T>(value, args);
            context.MergeResult(vc);
            return context;
        }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public override async Task<ValidationContext<ValidationValue<T>>> ValidateAsync(ValidationValue<T> value, ValidationArgs? args = null)
        {
            var context = new ValidationContext<ValidationValue<T>>(value, args ?? new ValidationArgs());

            // Validate each of the property rules.
            foreach (var rule in Rules)
            {
                await rule.ValidateAsync(context).ConfigureAwait(false);
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
        protected virtual Task OnValidateAsync(ValidationContext<ValidationValue<T>> context) => Task.CompletedTask;

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added.
        /// </summary>
        /// <param name="additionalAsync">The asynchronous function to invoke.</param>
        /// <returns>The <see cref="GenericValidator{T}"/>.</returns>
        public GenericValidator<T> Additional(Func<ValidationContext<ValidationValue<T>>, Task> additionalAsync)
        {
            Check.NotNull(additionalAsync, nameof(additionalAsync));

            if (_additionalAsync != null)
                throw new InvalidOperationException("Additional can only be defined once for a Validator.");

            _additionalAsync = additionalAsync;
            return this;
        }
    }
}