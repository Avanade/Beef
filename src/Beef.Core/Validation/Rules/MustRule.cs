// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides validation where the rule predicate <b>must</b> return <c>true</c> to be considered valid.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class MustRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty> where TEntity : class
    {
        private readonly Predicate<TEntity> _predicate;
        private readonly Func<bool> _must;

        /// <summary>
        /// Initializes a new instance of the <see cref="MustRule{TEntity, TProperty}"/> class with a <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The must predicate.</param>
        public MustRule(Predicate<TEntity> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MustRule{TEntity, TProperty}"/> class with a <paramref name="must"/> function.
        /// </summary>
        /// <param name="must">The must function.</param>
        public MustRule(Func<bool> must)
        {
            _must = must ?? throw new ArgumentNullException(nameof(must));
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            if (_predicate != null)
            {
                if (!_predicate.Invoke(context.Parent.Value))
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MustFormat);
            }
            else
            {
                if (!_must.Invoke())
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MustFormat);
            }
        }
    }
}
