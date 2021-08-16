// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides a comparision validation between two specified values.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class BetweenRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
    {
        private readonly TProperty _compareFromValue;
        private readonly Func<TEntity, TProperty>? _compareFromValueFunction;
        private readonly Func<TEntity, Task<TProperty>>? _compareFromValueFunctionAsync;
        private readonly LText? _compareFromText;
        private readonly Func<TEntity, LText>? _compareFromTextFunction;
        private readonly TProperty _compareToValue;
        private readonly Func<TEntity, TProperty>? _compareToValueFunction;
        private readonly Func<TEntity, Task<TProperty>>? _compareToValueFunctionAsync;
        private readonly LText? _compareToText;
        private readonly Func<TEntity, LText>? _compareToTextFunction;
        private readonly bool _exclusiveBetween;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetweenRule{TEntity, TProperty}"/> class specifying the between from and to values.
        /// </summary>
        /// <param name="compareFromValue">The compare from value.</param>
        /// <param name="compareToValue">The compare to value.</param>
        /// <param name="compareFromText">The compare from text to be passed for the error message (default is to use <paramref name="compareFromValue"/>).</param>
        /// <param name="compareToText">The compare to text to be passed for the error message (default is to use <paramref name="compareToValue"/>).</param>
        /// <param name="exclusiveBetween">Indicates whether the between comparison is exclusive or inclusive (default).</param>
        public BetweenRule(TProperty compareFromValue, TProperty compareToValue, LText? compareFromText = null, LText? compareToText = null, bool exclusiveBetween = false)
        {
            _compareFromValue = compareFromValue;
            _compareFromText = compareFromText;
            _compareToValue = compareToValue;
            _compareToText = compareToText;
            _exclusiveBetween = exclusiveBetween;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetweenRule{TEntity, TProperty}"/> class specifying the between from and to value functions.
        /// </summary>
        /// <param name="compareFromValueFunction">The compare from value function.</param>
        /// <param name="compareToValueFunction">The compare to value function.</param>
        /// <param name="compareFromTextFunction">The compare from text function (default is to use the result of the <paramref name="compareFromValueFunction"/>).</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunction"/>).</param>
        /// <param name="exclusiveBetween">Indicates whether the between comparison is exclusive or inclusive (default).</param>
        public BetweenRule(Func<TEntity, TProperty> compareFromValueFunction, Func<TEntity, TProperty> compareToValueFunction, Func<TEntity, LText>? compareFromTextFunction = null, Func<TEntity, LText>? compareToTextFunction = null, bool exclusiveBetween = false)
        {
            _compareFromValueFunction = compareFromValueFunction ?? throw new ArgumentNullException(nameof(compareFromValueFunction));
            _compareFromTextFunction = compareFromTextFunction;
            _compareFromValue = default!;
            _compareToValueFunction = compareToValueFunction ?? throw new ArgumentNullException(nameof(compareToValueFunction));
            _compareToTextFunction = compareToTextFunction;
            _compareToValue = default!;
            _exclusiveBetween = exclusiveBetween;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetweenRule{TEntity, TProperty}"/> class specifying the between from and to value async functions.
        /// </summary>
        /// <param name="compareFromValueFunctionAsync">The compare from value function.</param>
        /// <param name="compareToValueFunctionAsync">The compare to value function.</param>
        /// <param name="compareFromTextFunction">The compare from text function (default is to use the result of the <paramref name="compareFromValueFunctionAsync"/>).</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunctionAsync"/>).</param>
        /// <param name="exclusiveBetween">Indicates whether the between comparison is exclusive or inclusive (default).</param>
        public BetweenRule(Func<TEntity, Task<TProperty>> compareFromValueFunctionAsync, Func<TEntity, Task<TProperty>> compareToValueFunctionAsync, Func<TEntity, LText>? compareFromTextFunction = null, Func<TEntity, LText>? compareToTextFunction = null, bool exclusiveBetween = false)
        {
            _compareFromValueFunctionAsync = compareFromValueFunctionAsync ?? throw new ArgumentNullException(nameof(compareFromValueFunctionAsync));
            _compareFromTextFunction = compareFromTextFunction;
            _compareFromValue = default!;
            _compareToValueFunctionAsync = compareToValueFunctionAsync ?? throw new ArgumentNullException(nameof(compareToValueFunctionAsync));
            _compareToTextFunction = compareToTextFunction;
            _compareToValue = default!;
            _exclusiveBetween = exclusiveBetween;
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override async Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            var compareFromValue = _compareFromValueFunction != null
                ? _compareFromValueFunction(context.Parent.Value)
                : (_compareFromValueFunctionAsync != null
                    ? await _compareFromValueFunctionAsync(context.Parent.Value).ConfigureAwait(false)
                    : _compareFromValue);

            var compareToValue = _compareToValueFunction != null
                ? _compareToValueFunction(context.Parent.Value)
                : (_compareToValueFunctionAsync != null
                    ? await _compareToValueFunctionAsync(context.Parent.Value).ConfigureAwait(false)
                    : _compareToValue);

            var comparer = Comparer<TProperty>.Default;
            if ((_exclusiveBetween && (comparer.Compare(context.Value, compareFromValue) <= 0 || comparer.Compare(context.Value, compareToValue) >= 0))
                || (!_exclusiveBetween && (comparer.Compare(context.Value, compareFromValue) < 0 || comparer.Compare(context.Value, compareToValue) > 0)))
            {
                string? compareFromText = _compareFromText ?? compareFromValue?.ToString() ?? new LText("null");
                if (_compareFromTextFunction != null)
                    compareFromText = _compareFromTextFunction(context.Parent.Value);

                string? compareToText = _compareToText ?? compareToValue?.ToString() ?? new LText("null");
                if (_compareToTextFunction != null)
                    compareToText = _compareToTextFunction(context.Parent.Value); 

                context.CreateErrorMessage(ErrorText ?? (_exclusiveBetween ? ValidatorStrings.BetweenExclusiveFormat : ValidatorStrings.BetweenInclusiveFormat), (string)compareFromText, (string)compareToText);
            }
        }
    }
}