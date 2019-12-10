﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides a means to override a value within a validation context.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class OverrideRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty> where TEntity : class
    {
        private readonly Func<TEntity, TProperty> _func;
        private readonly TProperty _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverrideRule{TEntity, TProperty}"/> class with a <paramref name="func"/>.
        /// </summary>
        /// <param name="func">The override function.</param>
        public OverrideRule(Func<TEntity, TProperty> func)
        {
            _func = Beef.Check.NotNull(func, nameof(func));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverrideRule{TEntity, TProperty}"/> class with a <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The override value.</param>
        public OverrideRule(TProperty value)
        {
            _value = value;
        }

        /// <summary>
        /// Indicates whether the value is only overridden where the current value is the default value for the type.
        /// </summary>
        public bool OnlyOverrideDefault { get; set; } = false;

        /// <summary>
        /// Validate the property value.
        /// </summary> 
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));
            var overrideVal = _func != null ? _func(context.Parent.Value) : _value;

            // Compare the value against override to see if there is a difference.
            if (OnlyOverrideDefault && Comparer<TProperty>.Default.Compare(context.Value, default) != 0)
                return;

            context.OverrideValue(overrideVal);
        }
    }
}
