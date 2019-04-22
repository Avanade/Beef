// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation
{
    /// <summary>
    /// Provides a common value rule that can be used by other validators that share the same <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class CommonValidator<T> : PropertyRuleBase<ValidationValue<T>, T>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CommonValidator{T}"/>.
        /// </summary>
        /// <param name="validator">An action with the <see cref="CommonValidator{T}"/>.</param>
        /// <returns>The <see cref="CommonValidator{T}"/>.</returns>
        public static CommonValidator<T> Create(Action<CommonValidator<T>> validator)
        {
            var cv = new CommonValidator<T>();
            validator?.Invoke(cv);
            return cv;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonValidator{T}"/>.
        /// </summary>
        private CommonValidator() { }

        /// <summary>
        /// The <b>Run</b> method is not supported.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public override ValueValidatorResult<ValidationValue<T>, T> Run(bool throwOnError = false)
        {
            throw new NotSupportedException("The Run method is not supported for a CommonValueRule<T>.");
        }

        /// <summary>
        /// Validates the value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The value name (defaults to <see cref="ValueValidator{T}.ValueNameDefault"/>).</param>
        /// <param name="text">The friendly text name used in validation messages (defaults to <paramref name="name"/> as sentence case where not specified).</param>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public ValueValidatorResult<ValidationValue<T>, T> Validate(T value, string name = null, LText text = null, bool throwOnError = false)
        {
            var vv = new ValidationValue<T>(null, value);
            var ctx = new PropertyContext<ValidationValue<T>, T>(new ValidationContext<ValidationValue<T>>(vv,
                new ValidationArgs()), value, name ?? ValueValidator<T>.ValueNameDefault, null, text);

            Invoke(ctx);
            var res = new ValueValidatorResult<ValidationValue<T>, T>(ctx);
            if (throwOnError)
                res.ThrowOnError();

            return res;
        }

        /// <summary>
        /// Validates the value.
        /// </summary>
        /// <typeparam name="TEntity">The related entity <see cref="Type"/>.</typeparam>
        /// <param name="context">The related <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        internal void Validate<TEntity>(PropertyContext<TEntity, T> context) where TEntity : class
        {
            if (context == null)
                throw new ArgumentNullException("context");

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
            Invoke(ctx);
            context.HasError = ctx.HasError;
            context.Parent.MergeResult(ctx.Parent.Messages);
        }
    }
}