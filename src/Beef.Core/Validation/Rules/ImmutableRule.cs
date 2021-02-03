// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides validation where the immutable rule predicate <b>must</b> return <c>true</c> to be considered valid.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class ImmutableRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty> where TEntity : class
    {
        private readonly Predicate<TEntity>? _predicate;
        private readonly Func<bool>? _immutable;
        private readonly Func<Task<bool>>? _immutableAsync;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableRule{TEntity, TProperty}"/> class with a <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The must predicate.</param>
        public ImmutableRule(Predicate<TEntity> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableRule{TEntity, TProperty}"/> class with an <paramref name="immutable"/> function.
        /// </summary>
        /// <param name="immutable">The immutable function.</param>
        public ImmutableRule(Func<bool> immutable)
        {
            _immutable = immutable ?? throw new ArgumentNullException(nameof(immutable));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableRule{TEntity, TProperty}"/> class with an <paramref name="immutableAsync"/> function.
        /// </summary>
        /// <param name="immutableAsync">The immutable function.</param>
        public ImmutableRule(Func<Task<bool>> immutableAsync)
        {
            _immutableAsync = immutableAsync ?? throw new ArgumentNullException(nameof(immutableAsync));
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override async Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            if (_predicate != null)
            {
                if (!_predicate.Invoke(context.Parent.Value))
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.ImmutableFormat);
            }
            else if (_immutable != null)
            {
                if (!_immutable.Invoke())
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.ImmutableFormat);
            }
            else 
            {
                if (!(await _immutableAsync!.Invoke().ConfigureAwait(false)))
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.ImmutableFormat);
            }
        }
    }
}