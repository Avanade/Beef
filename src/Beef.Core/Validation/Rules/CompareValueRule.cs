// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides a comparision validation against a specified value.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class CompareValueRule<TEntity, TProperty> : CompareRuleBase<TEntity, TProperty>
        where TEntity : class
    {
        private readonly TProperty _compareToValue;
        private readonly Func<TEntity, TProperty> _compareToValueFunction;
        private readonly LText _compareToText;
        private readonly Func<TEntity, LText> _compareToTextFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareValueRule{TEntity, TProperty}"/> class specifying the compare to value.
        /// </summary>
        /// <param name="compareOperator">The <see cref="CompareOperator"/>.</param>
        /// <param name="compareToValue">The compare to value.</param>
        /// <param name="compareToText">The compare to text to be passed for the error message (default is to use <paramref name="compareToValue"/>).</param>
        public CompareValueRule(CompareOperator compareOperator, TProperty compareToValue, LText compareToText = null) : base(compareOperator)
        {
            _compareToValue = compareToValue;
            _compareToText = compareToText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareValueRule{TEntity, TProperty}"/> class specifying the compare to value function.
        /// </summary>
        /// <param name="compareOperator">The <see cref="CompareOperator"/>.</param>
        /// <param name="compareToValueFunction">The compare to value function.</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunction"/>).</param>
        public CompareValueRule(CompareOperator compareOperator, Func<TEntity, TProperty> compareToValueFunction, Func<TEntity, LText> compareToTextFunction = null) : base(compareOperator)
        {
            _compareToValueFunction = compareToValueFunction ?? throw new ArgumentNullException(nameof(compareToValueFunction));
            _compareToTextFunction = compareToTextFunction;
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            var compareToValue = _compareToValueFunction == null ? _compareToValue : _compareToValueFunction.Invoke(context.Parent.Value);
            if (!Compare(context.Value, compareToValue))
            {
                string compareToText = _compareToText ?? (compareToValue?.ToString());
                if (_compareToTextFunction != null)
                    compareToText = _compareToTextFunction(context.Parent.Value);

                CreateErrorMessage(context, compareToText);
            }
        }
    }
}