// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation
{
    /// <summary>
    /// Represents a validation rule set for an entity, in that it groups one or more <see cref="Rules"/> together for a specified condition.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public class RuleSet<TEntity> : ValidatorBase<TEntity>, IPropertyRule<TEntity> where TEntity : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSet{TEntity}"/> class to be invoked where the predicate is true.
        /// </summary>
        /// <param name="predicate">A function to determine whether the <see cref="RuleSet{TEntity}"/> is to be validated.</param>
        internal RuleSet(Predicate<ValidationContext<TEntity>> predicate)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Gets the function to determine whether the <see cref="RuleSet{TEntity}"/> is to be validated.
        /// </summary>
        public Predicate<ValidationContext<TEntity>> Predicate { get; private set; }

        /// <summary>
        /// Validates an entity given a <see cref="ValidationContext{TEntity}"/>.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/></param>
        public void Validate(ValidationContext<TEntity> context)
        {
            // Check the condition before continuing to validate the underlying rules.
            if (!Predicate(context))
                return;

            // Validate each of the property rules.
            foreach (var rule in Rules)
            {
                rule.Validate(context);
            }
        }
    }
}
