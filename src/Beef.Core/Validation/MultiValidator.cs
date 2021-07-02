// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Represents the result of a <see cref="MultiValidator"/> <see cref="MultiValidator.RunAsync(bool)"/>.
    /// </summary>
    public class MultiValidatorResult
    {
        private readonly MessageItemCollection _messages = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiValidatorResult"/> class.
        /// </summary>
        public MultiValidatorResult()
        {
            _messages.CollectionChanged += Messages_CollectionChanged;
        }

        /// <summary>
        /// Indicates whether there has been a validation error.
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection Messages { get { return _messages; } }

        /// <summary>
        /// Handle the add of a message.
        /// </summary>
        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (HasErrors)
                return;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var m in e.NewItems)
                    {
                        MessageItem mi = (MessageItem)m;
                        if (mi.Type == MessageType.Error)
                        {
                            HasErrors = true;
                            return;
                        }
                    }

                    break;

                default:
                    throw new InvalidOperationException("Operation invalid for Messages; only add supported.");
            }
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> where an error was found.
        /// </summary>
        public void ThrowOnError()
        {
            if (HasErrors)
                throw new ValidationException(Messages);
        }
    }

    /// <summary>
    /// Enables multiple validations to be performed (<see cref="RunAsync"/>) resulting in a single result.
    /// </summary>
    public class MultiValidator
    {
        private readonly List<Func<Task<MessageItemCollection>>> _validators = new();

        /// <summary>
        /// Creates a new <see cref="MultiValidator"/> instance.
        /// </summary>
        /// <returns>The <see cref="MultiValidator"/> instance.</returns>
        public static MultiValidator Create()
        {
            return new MultiValidator();
        }

        /// <summary>
        /// Adds a <see cref="ValueValidator{T}"/>. 
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="validator">The <see cref="ValueValidator{T}"/>. </param>
        /// <returns>The (this) <see cref="MultiValidator"/>.</returns>
        public MultiValidator Add<T>(ValueValidator<T> validator)
        {
            _validators.Add(async () => (await validator.RunAsync().ConfigureAwait(false)).Messages);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="ValidationValue{T}"/> <see cref="PropertyRuleBase{TEntity, TProperty}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="validator">The property rule validator.</param>
        /// <returns>The (this) <see cref="MultiValidator"/>.</returns>
        public MultiValidator Add<T>(PropertyRuleBase<ValidationValue<T>, T> validator)
        {
            _validators.Add(async () => (await validator.RunAsync().ConfigureAwait(false)).Messages);
            return this;
        }

        /// <summary>
        /// Adds an entity <see cref="ValidatorBase{TEntity}"/> with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The validator <see cref="Type"/>.</typeparam>
        /// <param name="validator">The <see cref="ValidatorBase{TEntity}"/>.</param>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The (this) <see cref="MultiValidator"/>.</returns>
        public MultiValidator Add<TEntity, TValidator>(ValidatorBase<TEntity> validator, TEntity value, ValidationArgs args) where TEntity : class where TValidator : ValidatorBase<TEntity>
        {
            _validators.Add(async () => (await validator.ValidateAsync(value, args).ConfigureAwait(false)).Messages);
            return this;
        }

        /// <summary>
        /// Adds an entity <see cref="ValidatorBase{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The validator <see cref="Type"/>.</typeparam>
        /// <param name="validator">The <see cref="ValidatorBase{TEntity}"/>.</param>
        /// <param name="value">The entity value.</param>
        /// <returns>The (this) <see cref="MultiValidator"/>.</returns>
        public MultiValidator Add<TEntity, TValidator>(ValidatorBase<TEntity> validator, TEntity value) where TEntity : class where TValidator : ValidatorBase<TEntity>
        {
            _validators.Add(async () => (await validator.ValidateAsync(value).ConfigureAwait(false)).Messages);
            return this;
        }

        /// <summary>
        /// Adds (chains) a child <see cref="MultiValidator"/>.
        /// </summary>
        /// <param name="validator">The child <see cref="MultiValidator"/>.</param>
        /// <returns>The (this) <see cref="MultiValidator"/>.</returns>
        public MultiValidator Add(MultiValidator validator)
        {
            _validators.Add(async () => (await validator.RunAsync().ConfigureAwait(false)).Messages);
            return this;
        }

        /// <summary>
        /// Runs the validations.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>The <see cref="MultiValidatorResult"/>.</returns>
        public async Task<MultiValidatorResult> RunAsync(bool throwOnError = false)
        {
            var res = new MultiValidatorResult();

            foreach (var v in _validators)
            {
                var msgs = await v.Invoke().ConfigureAwait(false);
                if (msgs != null && msgs.Count > 0)
                    res.Messages.AddRange(msgs);
            }

            if (throwOnError)
                res.ThrowOnError();

            return res;
        }

        /// <summary>
        /// Defines an <paramref name="action"/> to enable additional validations to be added (see <see cref="Add"/>).
        /// </summary>
        /// <param name="action">The custom action.</param>
        /// <returns>The (this) <see cref="MultiValidator"/>.</returns>
        public MultiValidator Additional(Action<MultiValidator> action)
        {
            action?.Invoke(this);
            return this;
        }
    }
}