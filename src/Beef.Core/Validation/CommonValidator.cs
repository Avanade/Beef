// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Provides access to the common validator capabilities.
    /// </summary>
    public static class CommonValidator
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CommonValidator{T}"/>.
        /// </summary>
        /// <param name="validator">An action with the <see cref="CommonValidator{T}"/>.</param>
        /// <returns>The <see cref="CommonValidator{T}"/>.</returns>
        public static CommonValidator<T> Create<T>(Action<CommonValidator<T>> validator)
        {
            var cv = new CommonValidator<T>();
            validator?.Invoke(cv);
            return cv;
        }
    }

    /// <summary>
    /// Provides a common value rule that can be used by other validators that share the same <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    /// <remarks>Note: the <see cref="PropertyRuleBase.Name"/>, <see cref="PropertyRuleBase.JsonName"/> and <see cref="PropertyRuleBase.Text"/> initially default to <see cref="Validator.ValueNameDefault"/>.</remarks>
    public class CommonValidator<T> : PropertyRuleBase<ValidationValue<T>, T>, IValidator<T>
    {
        private Func<PropertyContext<ValidationValue<T>, T>, Task>? _additionalAsync;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonValidator{T}"/>.
        /// </summary>
        public CommonValidator() : base(Validator.ValueNameDefault) { }

        /// <summary>
        /// The <b>Run</b> method is not supported.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public override Task<ValueValidatorResult<ValidationValue<T>, T>> RunAsync(bool throwOnError = false)
        {
            throw new NotSupportedException("The Run method is not supported for a CommonValueRule<T>.");
        }

        /// <summary>
        /// Validates the value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The value name (defaults to <see cref="Validator.ValueNameDefault"/>).</param>
        /// <param name="text">The friendly text name used in validation messages (defaults to <paramref name="name"/> as sentence case where not specified).</param>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public Task<ValueValidatorResult<ValidationValue<T>, T>> ValidateAsync(T value, string? name = null, LText? text = null, bool throwOnError = false)
            => ValidateAsync(value, name, name, text, throwOnError);

        /// <summary>
        /// Validates the value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The value name.</param>
        /// <param name="jsonName">The value JSON name.</param>
        /// <param name="text">The friendly text name used in validation messages (defaults to <paramref name="name"/> as sentence case where not specified).</param>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public async Task<ValueValidatorResult<ValidationValue<T>, T>> ValidateAsync(T value, string? name, string? jsonName, LText? text = null, bool throwOnError = false)
        {
            var vv = new ValidationValue<T>(null, value);
            var ctx = new PropertyContext<ValidationValue<T>, T>(new ValidationContext<ValidationValue<T>>(vv,
                new ValidationArgs()), value, name ?? Name, jsonName ?? JsonName, text ?? StringConversion.ToSentenceCase(name) ?? Text);

            await InvokeAsync(ctx).ConfigureAwait(false);
            var res = new ValueValidatorResult<ValidationValue<T>, T>(ctx);

            await OnValidateAsync(ctx).ConfigureAwait(false);
            if (_additionalAsync != null)
                await _additionalAsync(ctx).ConfigureAwait(false);

            if (throwOnError)
                res.ThrowOnError();

            return res;
        }

        /// <summary>
        /// Validates the value.
        /// </summary>
        /// <typeparam name="TEntity">The related entity <see cref="Type"/>.</typeparam>
        /// <param name="context">The related <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        internal async Task ValidateAsync<TEntity>(PropertyContext<TEntity, T> context) where TEntity : class
        {
            Check.NotNull(context, nameof(context));
            var vv = new ValidationValue<T>(context.Parent.Value, context.Value);
            var vc = new ValidationContext<ValidationValue<T>>(vv, new ValidationArgs
            {
                Config = context.Parent.Config,
                SelectedPropertyName = context.Name,
                FullyQualifiedEntityName = context.Parent.FullyQualifiedEntityName,
                FullyQualifiedJsonEntityName = context.Parent.FullyQualifiedJsonEntityName,
                UseJsonNames = context.UseJsonName
            });

            var ctx = new PropertyContext<ValidationValue<T>, T>(vc, context.Value, context.Name, context.JsonName, context.Text);
            await InvokeAsync(ctx).ConfigureAwait(false);

            await OnValidateAsync(ctx).ConfigureAwait(false);
            if (_additionalAsync != null)
                await _additionalAsync(ctx).ConfigureAwait(false);

            context.HasError = ctx.HasError;
            context.Parent.MergeResult(ctx.Parent.Messages);
        }

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added by the inheriting classes.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/>.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        protected virtual Task OnValidateAsync(PropertyContext<ValidationValue<T>, T> context) => Task.CompletedTask;

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added.
        /// </summary>
        /// <param name="additionalAsync">The asynchronous function to invoke.</param>
        /// <returns>The <see cref="CommonValidator{T}"/>.</returns>
        public CommonValidator<T> Additional(Func<PropertyContext<ValidationValue<T>, T>, Task> additionalAsync)
        {
            Check.NotNull(additionalAsync, nameof(additionalAsync));

            if (_additionalAsync != null)
                throw new InvalidOperationException("Additional can only be defined once for a Validator.");

            _additionalAsync = additionalAsync;
            return this;
        }

        #region IValidator<T>

        /// <summary>
        /// Gets the <see cref="Type"/> for the entity that is being validated.
        /// </summary>
        Type IValidator.ValueType => typeof(T);

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        async Task<ValidationContext<T>> IValidator<T>.ValidateAsync(T value, ValidationArgs args)
        {
            var context = new ValidationContext<T>(value, args);
            var ir = await ValidateAsync(value, context.FullyQualifiedEntityName, context.FullyQualifiedEntityName, Text).ConfigureAwait(false);
            context.MergeResult(ir.Messages);
            return context;
        }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        async Task<IValidationContext> IValidator.ValidateAsync(object? value, ValidationArgs args) => await ((IValidator<T>)this).ValidateAsync((T)value!, args).ConfigureAwait(false);

        #endregion
    }
}