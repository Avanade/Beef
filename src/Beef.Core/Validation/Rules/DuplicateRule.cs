// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides validation where the rule predicate <b>must</b> return <c>false</c> to not be considered a duplicate.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class DuplicateRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty> where TEntity : class
    {
        private Predicate<TEntity> _predicate;
        private Func<bool> _duplicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateRule{TEntity, TProperty}"/> class with a <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The must predicate.</param>
        public DuplicateRule(Predicate<TEntity> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateRule{TEntity, TProperty}"/> class with a <paramref name="duplicate"/> function.
        /// </summary>
        /// <param name="duplicate">The duplicate function.</param>
        public DuplicateRule(Func<bool> duplicate)
        {
            _duplicate = duplicate ?? throw new ArgumentNullException(nameof(duplicate));
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            if (_predicate != null)
            {
                if (_predicate(context.Parent.Value))
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.DuplicateFormat);
            }
            else
            {
                if (_duplicate())
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.DuplicateFormat);
            }
        }
    }
}
