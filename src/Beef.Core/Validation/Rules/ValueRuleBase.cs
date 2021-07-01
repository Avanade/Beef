// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation.Clauses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides the base functionality for a property value rule.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="System.Type"/>.</typeparam>
    public abstract class ValueRuleBase<TEntity, TProperty> : IValueRule<TEntity, TProperty> where TEntity : class
    {
        private readonly List<IPropertyRuleClause<TEntity>> _clauses = new();

        /// <summary>
        /// Gets or sets the error message format text (overrides the default).
        /// </summary>
        public LText? ErrorText { get; set; }

        /// <summary>
        /// Adds a clause (<see cref="IPropertyRuleClause{TEntity, TProperty}"/>) to the rule.
        /// </summary>
        /// <param name="clause">The <see cref="IPropertyRuleClause{TEntity, TProperty}"/>.</param>
        public void AddClause(IPropertyRuleClause<TEntity> clause)
        {
            if (clause == null)
                return;

            _clauses.Add(clause);
        }

        /// <summary>
        /// Checks the clause.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        public virtual bool Check(PropertyContext<TEntity, TProperty> context)
        {
            foreach (var clause in _clauses)
            {
                if (!clause.Check(context))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the clause.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        bool IValueRule<TEntity, TProperty>.Check(IPropertyContext context)
        {
            return Check((PropertyContext<TEntity, TProperty>)context);
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        public abstract Task ValidateAsync(PropertyContext<TEntity, TProperty> context);
    }
}