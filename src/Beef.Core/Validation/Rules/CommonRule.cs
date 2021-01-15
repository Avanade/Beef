// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides for integrating a common validation against a specified property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="System.Type"/>.</typeparam>
    internal class CommonRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
    {
        private readonly CommonValidator<TProperty> _commonValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonRule{TEntity, TProperty}"/> class specifying the corresponding <paramref name="commonValidator"/>.
        /// </summary>
        /// <param name="commonValidator">The <see cref="CommonValidator{T}"/>.</param>
        public CommonRule(CommonValidator<TProperty> commonValidator)
        {
            _commonValidator = commonValidator ?? throw new ArgumentNullException(nameof(commonValidator));
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            return _commonValidator.ValidateAsync(context);
        }
    }
}
