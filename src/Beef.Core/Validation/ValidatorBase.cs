// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Enables a <see cref="ValidateAsync(object?, ValidationArgs)"/>.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        Task<IValidationContext> ValidateAsync(object? value, ValidationArgs args);

        /// <summary>
        /// Gets the <see cref="Type"/> for the entity that is being validated.
        /// </summary>
        Type ValueType { get; }
    }

    /// <summary>
    /// Enables a <see cref="ValidateAsync(T, ValidationArgs)"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public interface IValidator<T> : IValidator
    {
        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        Task<ValidationContext<T>> ValidateAsync(T value, ValidationArgs args);
    }

    /// <summary>
    /// Represents the base entity validator.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public abstract class ValidatorBase<TEntity> : IValidator<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the underlying rules collection.
        /// </summary>
        internal protected List<IPropertyRule<TEntity>> Rules { get; } = new List<IPropertyRule<TEntity>>();

#pragma warning disable CA1716 // Identifiers should not match keywords; by-design, best name.
        /// <summary>
        /// Adds a <see cref="PropertyRule{TEntity, TProperty}"/> to the validator.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public virtual PropertyRule<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
#pragma warning restore CA1716
        {
            PropertyRule<TEntity, TProperty> rule = new(propertyExpression);
            Rules.Add(rule);
            return rule;
        }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public virtual Task<ValidationContext<TEntity>> ValidateAsync(TEntity value, ValidationArgs? args = null)
        {
            throw new NotSupportedException("Validate is not supported by this class.");
        }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        async Task<IValidationContext> IValidator.ValidateAsync(object? value, ValidationArgs args)
        {
            return await ValidateAsync((TEntity)value!, args).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the entity that is being validated.
        /// </summary>
        Type IValidator.ValueType => typeof(TEntity);
    }
}