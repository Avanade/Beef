// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;

namespace Beef.Validation
{
    /// <summary>
    /// Represents the result of a <see cref="ValueValidator{T}"/> or <see cref="ValueValidator{TEntity, TProperty}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class ValueValidatorResult<TEntity, TProperty> where TEntity : class
    {
        private PropertyContext<TEntity, TProperty> _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueValidatorResult{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public ValueValidatorResult(PropertyContext<TEntity, TProperty> context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public TProperty Value
        {
            get { return _context.Value; }
        }

        /// <summary>
        /// Indicates whether there has been a validation error.
        /// </summary>
        public bool HasError
        {
            get { return _context.HasError; }
        }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection Messages
        {
            get { return _context.Parent.Messages; }
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> where an error was found.
        /// </summary>
        public void ThrowOnError()
        {
            if (HasError)
                throw new ValidationException(Messages);
        }
    }
}
