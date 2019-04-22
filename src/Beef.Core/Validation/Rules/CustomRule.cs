// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides a custom validation against a specified property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="System.Type"/>.</typeparam>
    public class CustomRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
    {
        private readonly Action<PropertyContext<TEntity, TProperty>> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRule{TEntity, TProperty}"/> class specifying the corresponding <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The action to invoke to perform the custom validation.</param>
        public CustomRule(Action<PropertyContext<TEntity, TProperty>> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            _action(context);
        }
    }
}
