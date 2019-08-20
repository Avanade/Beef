// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation
{
    /// <summary>
    /// Represents a rule that enables a base validator to be included.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TInclude">The entity base <see cref="Type"/>.</typeparam>
    public class IncludeBaseRule<TEntity, TInclude> : ValidatorBase<TEntity>, IPropertyRule<TEntity>
        where TEntity : class
        where TInclude : class
    {
        private readonly ValidatorBase<TInclude> _include;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludeBaseRule{TEntity, TInclude}"/> class.
        /// </summary>
        /// <param name="include">The base <see cref="ValidatorBase{TInclude}"/>.</param>
        internal IncludeBaseRule(ValidatorBase<TInclude> include)
        {
            _include = Check.NotNull(include, nameof(include));
        }

        /// <summary>
        /// Validates an entity given a <see cref="ValidationContext{TEntity}"/>.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/></param>
        public void Validate(ValidationContext<TEntity> context)
        {
            TInclude val = context.Value as TInclude;
            if (val == null)
                throw new InvalidOperationException($"Type {typeof(TEntity).Name} must inherit from {typeof(TInclude).Name}.");

            var ctx = new ValidationContext<TInclude>(val, new ValidationArgs
            {
                Config = context.Config,
                SelectedPropertyName = context.SelectedPropertyName,
                ShallowValidation = context.ShallowValidation,
                FullyQualifiedEntityName = context.FullyQualifiedEntityName,
                UseJsonNames = context.UseJsonNames
            });

            foreach (var r in _include.Rules)
            {
                r.Validate(ctx);
            }

            context.MergeResult(ctx);
        }
    }
}
