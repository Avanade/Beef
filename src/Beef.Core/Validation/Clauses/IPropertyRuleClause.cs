// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Validation.Clauses
{
    /// <summary>
    /// Enables a <see cref="PropertyRule{TEntity, TProperty}"/> clause.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    public interface IPropertyRuleClause<TEntity> where TEntity : class
    {
        /// <summary>
        /// Checks the clause.
        /// </summary>
        /// <param name="context">The <see cref="IPropertyContext"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        bool Check(IPropertyContext context);
    }

    /// <summary>
    /// Enables a typed <see cref="PropertyRule{TEntity, TProperty}"/> clause.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="System.Type"/>.</typeparam>
    public interface IPropertyRuleClause<TEntity, TProperty> : IPropertyRuleClause<TEntity> where TEntity : class { }
}
