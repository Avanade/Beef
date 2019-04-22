// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation.Clauses
{
    /// <summary>
    /// Represents a when test clause; in that the condition must be <c>true</c> to continue.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class WhenClause<TEntity, TProperty> : IPropertyRuleClause<TEntity, TProperty> where TEntity : class
    {
        private Predicate<TEntity> _entityPredicate;
        private Predicate<TProperty> _propertyPredicate;
        private Func<bool> _when;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenClause{TEntity, TProperty}"/> class with a <paramref name="predicate"/> being passed the <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="predicate">The when predicate.</param>
        public WhenClause(Predicate<TEntity> predicate)
        {
            _entityPredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenClause{TEntity, TProperty}"/> class with a <paramref name="when"/> function.
        /// </summary>
        /// <param name="when">The when function.</param>
        public WhenClause(Func<bool> when)
        {
            _when = when ?? throw new ArgumentNullException(nameof(when));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhenClause{TEntity, TProperty}"/> class with a <paramref name="predicate"/> being passed the <typeparamref name="TProperty"/>.
        /// </summary>
        /// <param name="predicate">The when predicate.</param>
        public WhenClause(Predicate<TProperty> predicate)
        {
            _propertyPredicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Checks the clause.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        public bool Check(IPropertyContext context)
        {
            return _when != null ? _when.Invoke()
                : _entityPredicate != null ? _entityPredicate.Invoke((TEntity)context.Parent.Value)
                : _propertyPredicate.Invoke((TProperty)context.Value);
        }
    }
}
