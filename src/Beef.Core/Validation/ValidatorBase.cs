// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Beef.Validation
{
    /// <summary>
    /// Enables a <see cref="Validate(object, ValidationArgs)"/>.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        IValidationContext Validate(object value, ValidationArgs args);

        /// <summary>
        /// Gets the <see cref="Type"/> for the entity that is being validated.
        /// </summary>
        Type EntityType { get; }
    }

    /// <summary>
    /// Represents the base entity validator.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public abstract class ValidatorBase<TEntity> : IValidator
        where TEntity : class
    {
        /// <summary>
        /// Gets the underlying rules collection.
        /// </summary>
        internal protected List<IPropertyRule<TEntity>> Rules { get; } = new List<IPropertyRule<TEntity>>();

        /// <summary>
        /// Adds a <see cref="PropertyRule{TEntity, TProperty}"/> to the validator.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public virtual PropertyRule<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            PropertyRule<TEntity, TProperty> rule = new PropertyRule<TEntity, TProperty>(propertyExpression);
            Rules.Add(rule);
            return rule;
        }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public virtual ValidationContext<TEntity> Validate(TEntity value, ValidationArgs args = null)
        {
            throw new NotSupportedException("Validate is not supported by this class.");
        }

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="IValidationContext"/>.</returns>
        IValidationContext IValidator.Validate(object value, ValidationArgs args)
        {
            return Validate((TEntity)value, args);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the entity that is being validated.
        /// </summary>
        Type IValidator.EntityType => typeof(TEntity);
    }
}
