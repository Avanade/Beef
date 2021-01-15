// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation.Clauses;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides the rule to <see cref="ValidateAsync">validate</see> a value.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    /// <typeparam name="TProperty">The value <see cref="System.Type"/>.</typeparam>
    public interface IValueRule<TEntity, TProperty> where TEntity : class
    {
        /// <summary>
        /// Adds a <see cref="IPropertyRuleClause{TEntity}"/>.
        /// </summary>
        /// <param name="clause">The <see cref="IPropertyRuleClause{TEntity}"/>.</param>
        void AddClause(IPropertyRuleClause<TEntity> clause);

        /// <summary>
        /// Checks the clauses.
        /// </summary>
        /// <param name="context">The <see cref="IPropertyContext"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        bool Check(IPropertyContext context);

        /// <summary>
        /// Validate the value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        Task ValidateAsync(PropertyContext<TEntity, TProperty> context);
    }
}