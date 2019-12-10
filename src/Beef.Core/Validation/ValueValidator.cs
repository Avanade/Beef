// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq.Expressions;

namespace Beef.Validation
{
    /// <summary>
    /// Represents a validation value.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class ValidationValue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationValue{T}"/> class.
        /// </summary>
        /// <param name="entity">The parent entity value.</param>
        /// <param name="value">The value.</param>
        internal ValidationValue(object entity, T value)
        {
            Entity = entity;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the entity value.
        /// </summary>
        public object Entity { get; }

        /// <summary>
        /// Gets or sets the entity property value.
        /// </summary>
        public T Value { get; }
    }

    /// <summary>
    /// Enables validation for a value.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class ValueValidator<T> : PropertyRuleBase<ValidationValue<T>, T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueValidator{T}"/> class.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The value name (defaults to <see cref="Validator.ValueNameDefault"/>).</param>
        /// <param name="text">The friendly text name used in validation messages (defaults to <paramref name="name"/> as sentence case where not specified).</param>
        public ValueValidator(T value, string name = null, LText text = null)
        {
            ValidationValue = new ValidationValue<T>(null, value);
            Name = string.IsNullOrEmpty(name) ? Validator.ValueNameDefault : name;
            Text = text ?? Beef.CodeGen.CodeGenerator.ToSentenceCase(Name);
        }

        /// <summary>
        /// Gets the <see cref="ValidationValue{T}"/>.
        /// </summary>
        public ValidationValue<T> ValidationValue { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value { get => ValidationValue.Value; }

        /// <summary>
        /// Runs the validation for the <see cref="Value"/>.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public override ValueValidatorResult<ValidationValue<T>, T> Run(bool throwOnError = false)
        {
            var ctx = new PropertyContext<ValidationValue<T>, T>(new ValidationContext<ValidationValue<T>>(null, new ValidationArgs()), Value, this.Name, this.JsonName, this.Text);
            Invoke(ctx);
            var res = new ValueValidatorResult<ValidationValue<T>, T>(ctx);
            if (throwOnError)
                res.ThrowOnError();

            return res;
        }
    }

    /// <summary>
    /// Enables validation for a single entity property value (using the property to determine the error message text).
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class ValueValidator<TEntity, TProperty> : PropertyRule<TEntity, TProperty> where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueValidator{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="value">The value to validate.</param>
        public ValueValidator(Expression<Func<TEntity, TProperty>> propertyExpression, TProperty value) : base(propertyExpression)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public TProperty Value { get; private set; }

        /// <summary>
        /// Runs the validation for the <see cref="Value"/>.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public override ValueValidatorResult<TEntity, TProperty> Run(bool throwOnError = false)
        {
            var ctx = new PropertyContext<TEntity, TProperty>(new ValidationContext<TEntity>(null, new ValidationArgs()), Value, this.Name, this.JsonName, this.Text);
            Invoke(ctx);
            var res = new ValueValidatorResult<TEntity, TProperty>(ctx);
            if (throwOnError)
                res.ThrowOnError();

            return res;
        }
    }
}
