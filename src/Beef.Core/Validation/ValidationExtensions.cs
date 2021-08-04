// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using Beef.Validation.Rules;
using Beef.WebApi;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Provides extension methods required by the validation framework (including support for fluent-style method chaining).
    /// </summary>
    public static class ValidationExtensions
    {
        #region Text

        /// <summary>
        /// Updates the rule friendly name text used in validation messages (see <see cref="PropertyRuleBase.Text"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="text">The text for the rule.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Text<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, LText text) where TEntity : class
        {
            Beef.Check.NotNull(rule, nameof(rule));
            rule.Text = text;
            return rule;
        }

        #endregion

        #region Mandatory

        /// <summary>
        /// Adds a mandatory validation (<see cref="MandatoryRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Mandatory<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MandatoryRule<TEntity, TProperty> { ErrorText = errorText });
        }

        #endregion

        #region Must

        /// <summary>
        /// Adds a validation where the rule <paramref name="predicate"/> <b>must</b> return <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="predicate">The must predicate.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Must<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Predicate<TEntity> predicate, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(predicate) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="must"/> function <b>must</b> return <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="must">The must function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Must<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<bool> must, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(must) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="must"/> value be <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="must">The must value.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Must<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, bool must, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(() => must) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="mustAsync"/> function <b>must</b> return <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="mustAsync">The must function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Must<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<Task<bool>> mustAsync, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(mustAsync) { ErrorText = errorText });
        }

        #endregion

        #region Exists

        /// <summary>
        /// Adds a validation where the rule <paramref name="predicate"/> <b>exists</b> return <c>true</c> to verify it exists (see <see cref="ExistsRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="predicate">The exists predicate.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Exists<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Predicate<TEntity> predicate, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ExistsRule<TEntity, TProperty>(predicate) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="exists"/> function <b>exists</b> return <c>true</c> to verify it exists (see <see cref="ExistsRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="exists">The exists function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Exists<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, Task<bool>> exists, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ExistsRule<TEntity, TProperty>(exists) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="exists"/> value is <c>true</c> to verify it exists (see <see cref="ExistsRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="exists">The exists value.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Exists<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, bool exists, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ExistsRule<TEntity, TProperty>(_ => Task.FromResult(exists)) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="exists"/> function must return <b>not null</b> to verify it exists (see <see cref="ExistsRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="exists">The exists function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Exists<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, Task<object>> exists, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ExistsRule<TEntity, TProperty>(exists) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="exists"/> is <b>not null</b> to verify it exists (see <see cref="ExistsRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="exists">The exists function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Exists<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, object exists, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ExistsRule<TEntity, TProperty>(_ => Task.FromResult(exists != null)) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="agentResult"/> function must return a successful response to verify it exists (see <see cref="ExistsRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="agentResult">The <see cref="WebApiAgentResult"/> function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> AgentExists<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, Task<WebApiAgentResult>> agentResult, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ExistsRule<TEntity, TProperty>(agentResult) { ErrorText = errorText });
        }

        #endregion

        #region Duplicate

        /// <summary>
        /// Adds a validation where the rule <paramref name="predicate"/> <b>must</b> return <c>false</c> to not be considered a duplicate (see <see cref="DuplicateRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="predicate">The must predicate.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Duplicate<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Predicate<TEntity> predicate, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DuplicateRule<TEntity, TProperty>(predicate) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="duplicate"/> function <b>must</b> return <c>false</c> to not be considered a duplicate (see <see cref="DuplicateRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="duplicate">The duplicate function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Duplicate<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<bool> duplicate, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DuplicateRule<TEntity, TProperty>(duplicate) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="duplicate"/> value must be <c>false</c> to not be considered a duplicate (see <see cref="DuplicateRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="duplicate">The duplicate value.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Duplicate<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, bool duplicate, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DuplicateRule<TEntity, TProperty>(() => duplicate) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where considered a duplicate (see <see cref="DuplicateRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Duplicate<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DuplicateRule<TEntity, TProperty>(() => true) { ErrorText = errorText });
        }

        #endregion

        #region Immutable

        /// <summary>
        /// Adds a validation where the rule <paramref name="predicate"/> <b>must</b> return <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="predicate">The must predicate.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Immutable<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Predicate<TEntity> predicate, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(predicate) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="immutable"/> function <b>must</b> return <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="immutable">The must function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Immutable<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<bool> immutable, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(immutable) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="immutableAsync"/> function <b>must</b> return <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="immutableAsync">The must function.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Immutable<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<Task<bool>> immutableAsync, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(immutableAsync) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where the rule <paramref name="immutable"/> value be <c>true</c> to be considered valid (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="immutable">The must value.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Immutable<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, bool immutable, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(() => immutable) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a validation where considered immutable (see <see cref="MustRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Immutable<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new MustRule<TEntity, TProperty>(() => true) { ErrorText = errorText });
        }

        #endregion

        #region Between

        /// <summary>
        /// Adds a between comparision validation against a specified from and to value (see <see cref="CompareValueRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareFromValue">The compare from value.</param>
        /// <param name="compareToValue">The compare to value.</param>
        /// <param name="compareFromText">The compare from text to be passed for the error message (default is to use <paramref name="compareFromValue"/>).</param>
        /// <param name="compareToText">The compare to text to be passed for the error message (default is to use <paramref name="compareToValue"/>).</param>
        /// <param name="exclusiveBetween">Indicates whether the between comparison is exclusive or inclusive (default).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Between<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, TProperty compareFromValue, TProperty compareToValue, LText? compareFromText = null, LText? compareToText = null, bool exclusiveBetween = false, LText ? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new BetweenRule<TEntity, TProperty>(compareFromValue, compareToValue, compareFromText, compareToText, exclusiveBetween) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a between comparision validation against from and to values returned by functions (<see cref="CompareValueRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareFromValueFunction">The compare from value function.</param>
        /// <param name="compareToValueFunction">The compare to value function.</param>
        /// <param name="compareFromTextFunction">The compare from text function (default is to use the result of the <paramref name="compareFromValueFunction"/>).</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunction"/>).</param>
        /// <param name="exclusiveBetween">Indicates whether the between comparison is exclusive or inclusive (default).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Between<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, TProperty> compareFromValueFunction, Func<TEntity, TProperty> compareToValueFunction, Func<TEntity, LText>? compareFromTextFunction = null, Func<TEntity, LText>? compareToTextFunction = null, bool exclusiveBetween = false, LText? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new BetweenRule<TEntity, TProperty>(compareFromValueFunction, compareToValueFunction, compareFromTextFunction, compareToTextFunction, exclusiveBetween) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a between comparision validation against from and to values returned by async functions (<see cref="CompareValueRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareFromValueFunctionAsync">The compare from value function.</param>
        /// <param name="compareToValueFunctionAsync">The compare to value function.</param>
        /// <param name="compareFromTextFunction">The compare from text function (default is to use the result of the <paramref name="compareFromValueFunctionAsync"/>).</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunctionAsync"/>).</param>
        /// <param name="exclusiveBetween">Indicates whether the between comparison is exclusive or inclusive (default).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Between<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, Task<TProperty>> compareFromValueFunctionAsync, Func<TEntity, Task<TProperty>> compareToValueFunctionAsync, Func<TEntity, LText>? compareFromTextFunction = null, Func<TEntity, LText>? compareToTextFunction = null, bool exclusiveBetween = false, LText? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new BetweenRule<TEntity, TProperty>(compareFromValueFunctionAsync, compareToValueFunctionAsync, compareFromTextFunction, compareToTextFunction, exclusiveBetween) { ErrorText = errorText });
        }

        #endregion

        #region CompareValue

        /// <summary>
        /// Adds a comparision validation against a specified value (see <see cref="CompareValueRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareOperator">The <see cref="CompareOperator"/>.</param>
        /// <param name="compareToValue">The compare to value.</param>
        /// <param name="compareToText">The compare to text <see cref="LText"/> to be passed for the error message (default is to use <paramref name="compareToValue"/>).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> CompareValue<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, CompareOperator compareOperator, TProperty compareToValue, LText? compareToText = null, LText? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new CompareValueRule<TEntity, TProperty>(compareOperator, compareToValue, compareToText) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a comparision validation against a value returned by a function (<see cref="CompareValueRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareOperator">The <see cref="CompareOperator"/>.</param>
        /// <param name="compareToValueFunction">The compare to function.</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunction"/>).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> CompareValue<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, CompareOperator compareOperator, Func<TEntity, TProperty> compareToValueFunction, Func<TEntity, LText>? compareToTextFunction = null, LText? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new CompareValueRule<TEntity, TProperty>(compareOperator, compareToValueFunction, compareToTextFunction) { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a comparision validation against a value returned by an async function (<see cref="CompareValueRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareOperator">The <see cref="CompareOperator"/>.</param>
        /// <param name="compareToValueFunctionAsync">The compare to function.</param>
        /// <param name="compareToTextFunction">The compare to text function (default is to use the result of the <paramref name="compareToValueFunctionAsync"/>).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> CompareValue<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, CompareOperator compareOperator, Func<TEntity, Task<TProperty>> compareToValueFunctionAsync, Func<TEntity, LText>? compareToTextFunction = null, LText? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new CompareValueRule<TEntity, TProperty>(compareOperator, compareToValueFunctionAsync, compareToTextFunction) { ErrorText = errorText });
        }

        #endregion

        #region CompareProperty

        /// <summary>
        /// Adds a comparision validation against a specified property (see <see cref="ComparePropertyRule{TEntity, TProperty, TProperty2}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <typeparam name="TCompareProperty">The compare to property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="compareOperator">The <see cref="CompareOperator"/>.</param>
        /// <param name="compareToPropertyExpression">The <see cref="Expression"/> to reference the compare to entity property.</param>
        /// <param name="compareToText">The compare to text <see cref="LText"/> to be passed for the error message (default is to use <paramref name="compareToPropertyExpression"/>).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> CompareProperty<TEntity, TProperty, TCompareProperty>(this PropertyRuleBase<TEntity, TProperty> rule, CompareOperator compareOperator, Expression<Func<TEntity, TCompareProperty>> compareToPropertyExpression, LText? compareToText = null, LText? errorText = null)
            where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ComparePropertyRule<TEntity, TProperty, TCompareProperty>(compareOperator, compareToPropertyExpression, compareToText) { ErrorText = errorText });
        }

        #endregion

        #region String

#pragma warning disable CA1720 // Identifier contains type name; by-design, best name.
        /// <summary>
        /// Adds a <see cref="string"/> validation with a maximum length (see <see cref="StringRule{TEntity}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, String}"/> being extended.</param>
        /// <param name="maxLength">The maximum string length.</param>
        /// <param name="regex">The <see cref="Regex"/>.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, String}"/>.</returns>
        public static PropertyRuleBase<TEntity, string?> String<TEntity>(this PropertyRuleBase<TEntity, string?> rule, int maxLength, Regex? regex = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new StringRule<TEntity> { MaxLength = maxLength, Regex = regex, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="string"/> validation with a minimum and maximum length (see <see cref="StringRule{TEntity}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, String}"/> being extended.</param>
        /// <param name="minLength">The minimum string length.</param>
        /// <param name="maxLength">The maximum string length.</param>
        /// <param name="regex">The <see cref="Regex"/>.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, String}"/>.</returns>
        public static PropertyRuleBase<TEntity, string?> String<TEntity>(this PropertyRuleBase<TEntity, string?> rule, int minLength, int? maxLength, Regex? regex = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new StringRule<TEntity> { MinLength = minLength, MaxLength = maxLength, Regex = regex, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="string"/> validation with a <paramref name="regex"/> (see <see cref="StringRule{TEntity}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, String}"/> being extended.</param>
        /// <param name="regex">The <see cref="Regex"/>.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, String}"/>.</returns>
        public static PropertyRuleBase<TEntity, string?> String<TEntity>(this PropertyRuleBase<TEntity, string?> rule, Regex? regex = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new StringRule<TEntity> { Regex = regex, ErrorText = errorText });
        }
#pragma warning restore CA1720 

        #endregion

        #region Wildcard

        /// <summary>
        /// Adds a <see cref="string"/> <see cref="Wildcard"/> validation (see <see cref="WildcardRule{TEntity}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, String}"/> being extended.</param>
        /// <param name="wildcard">The <see cref="Wildcard"/> configuration (defaults to <see cref="Wildcard.Default"/>).</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, String}"/>.</returns>
        public static PropertyRuleBase<TEntity, string?> Wildcard<TEntity>(this PropertyRuleBase<TEntity, string?> rule, Wildcard? wildcard = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new WildcardRule<TEntity> { Wildcard = wildcard, ErrorText = errorText });
        }

        #endregion

        #region Numeric

        /// <summary>
        /// Adds a <see cref="Int32"/> validation (see <see cref="DecimalRule{TEntity, Int32}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Int32}"/> being extended.</param> 
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Int32}"/>.</returns>
        public static PropertyRuleBase<TEntity, int> Numeric<TEntity>(this PropertyRuleBase<TEntity, int> rule, bool allowNegatives = false, int? maxDigits = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, int> { AllowNegatives = allowNegatives, MaxDigits = maxDigits, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Nullable{Int32}"/> validation (see <see cref="DecimalRule{TEntity, Int32}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Int32}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Int32}"/>.</returns>
        public static PropertyRuleBase<TEntity, int?> Numeric<TEntity>(this PropertyRuleBase<TEntity, int?> rule, bool allowNegatives = false, int? maxDigits = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, int?> { AllowNegatives = allowNegatives, MaxDigits = maxDigits, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Int64"/> validation (see <see cref="DecimalRule{TEntity, Int64}"/>);
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Long}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Int64}"/>.</returns>
        public static PropertyRuleBase<TEntity, long> Numeric<TEntity>(this PropertyRuleBase<TEntity, long> rule, bool allowNegatives = false, int? maxDigits = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, long> { AllowNegatives = allowNegatives, MaxDigits = maxDigits, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Nullable{Int64}"/> validation (see <see cref="DecimalRule{TEntity, Int64}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Int64}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Int64}"/>.</returns>
        public static PropertyRuleBase<TEntity, long?> Numeric<TEntity>(this PropertyRuleBase<TEntity, long?> rule, bool allowNegatives = false, int? maxDigits = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, long?> { AllowNegatives = allowNegatives, MaxDigits = maxDigits, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Decimal"/> validation (see <see cref="DecimalRule{TEntity, Decimal}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Decimal}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits (including decimal places).</param>
        /// <param name="decimalPlaces">The maximum number of decimal places.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Decimal}"/>.</returns>
        public static PropertyRuleBase<TEntity, decimal> Numeric<TEntity>(this PropertyRuleBase<TEntity, decimal> rule, bool allowNegatives = false, int? maxDigits = null, int? decimalPlaces = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, decimal> { AllowNegatives = allowNegatives, MaxDigits = maxDigits, DecimalPlaces = decimalPlaces, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Nullable{Decimal}"/> validation (see <see cref="DecimalRule{TEntity, Decimal}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Decimal}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits (including decimal places).</param>
        /// <param name="decimalPlaces">The maximum number of decimal places.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Decimal}"/>.</returns>
        public static PropertyRuleBase<TEntity, decimal?> Numeric<TEntity>(this PropertyRuleBase<TEntity, decimal?> rule, bool allowNegatives = false, int? maxDigits = null, int? decimalPlaces = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, decimal?> { AllowNegatives = allowNegatives, MaxDigits = maxDigits, DecimalPlaces = decimalPlaces, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Single"/> validation (see <see cref="NumericRule{TEntity, Single}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Single}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Single}"/>.</returns>
        public static PropertyRuleBase<TEntity, float> Numeric<TEntity>(this PropertyRuleBase<TEntity, float> rule, bool allowNegatives = false, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new NumericRule<TEntity, float> { AllowNegatives = allowNegatives, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Nullable{Single}"/> validation (see <see cref="NumericRule{TEntity, Single}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Single}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Single}"/>.</returns>
        public static PropertyRuleBase<TEntity, float?> Numeric<TEntity>(this PropertyRuleBase<TEntity, float?> rule, bool allowNegatives = false, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new NumericRule<TEntity, float?> { AllowNegatives = allowNegatives, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Double"/> validation (see <see cref="NumericRule{TEntity, Double}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Double}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Double}"/>.</returns>
        public static PropertyRuleBase<TEntity, double> Numeric<TEntity>(this PropertyRuleBase<TEntity, double> rule, bool allowNegatives = false, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new NumericRule<TEntity, double> { AllowNegatives = allowNegatives, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="Nullable{Double}"/> validation (see <see cref="NumericRule{TEntity, Double}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Double}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Double}"/>.</returns>
        public static PropertyRuleBase<TEntity, double?> Numeric<TEntity>(this PropertyRuleBase<TEntity, double?> rule, bool allowNegatives = false, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new NumericRule<TEntity, double?> { AllowNegatives = allowNegatives, ErrorText = errorText });
        }

        #endregion

        #region Currency

        /// <summary>
        /// Adds a currency (<see cref="Decimal"/>) validation (see <see cref="DecimalRule{TEntity, Decimal}"/> for an <see cref="Decimal"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Decimal}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits (including decimal places).</param>
        /// <param name="currencyFormatInfo">The <see cref="NumberFormatInfo"/> that the <see cref="NumberFormatInfo.CurrencyDecimalDigits">decimal places</see> will be derived from;
        /// where <c>null</c> <see cref="NumberFormatInfo.CurrentInfo"/> will be used as a default.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Decimal}"/>.</returns>
        public static PropertyRuleBase<TEntity, decimal> Currency<TEntity>(this PropertyRuleBase<TEntity, decimal> rule, bool allowNegatives = false, int? maxDigits = null, NumberFormatInfo? currencyFormatInfo = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, decimal>
            {
                AllowNegatives = allowNegatives,
                MaxDigits = maxDigits,
                DecimalPlaces = currencyFormatInfo == null ? NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits : currencyFormatInfo.CurrencyDecimalDigits,
                ErrorText = errorText
            });
        }

        /// <summary>
        /// Adds a currency <see cref="Nullable{Decimal}"/> validation (see <see cref="DecimalRule{TEntity, Decimal}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, Decimal}"/> being extended.</param>
        /// <param name="allowNegatives">Indicates whether to allow negative values.</param>
        /// <param name="maxDigits">The maximum digits (including decimal places).</param>
        /// <param name="currencyFormatInfo">The <see cref="NumberFormatInfo"/> that the <see cref="NumberFormatInfo.CurrencyDecimalDigits">decimal places</see> will be derived from;
        /// where <c>null</c> <see cref="NumberFormatInfo.CurrentInfo"/> will be used as a default.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, Decimal}"/>.</returns>
        public static PropertyRuleBase<TEntity, decimal?> Currency<TEntity>(this PropertyRuleBase<TEntity, decimal?> rule, bool allowNegatives = false, int? maxDigits = null, NumberFormatInfo? currencyFormatInfo = null, LText? errorText = null) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new DecimalRule<TEntity, decimal?>
            {
                AllowNegatives = allowNegatives,
                MaxDigits = maxDigits,
                DecimalPlaces = currencyFormatInfo == null ? NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits : currencyFormatInfo.CurrencyDecimalDigits,
                ErrorText = errorText
            });
        }

        #endregion

        #region ReferenceData

        /// <summary>
        /// Adds a <see cref="ReferenceDataBase"/> validation (see <see cref="ReferenceDataRule{TEntity, TProperty}"/>) to ensure the value is valid.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/> (must inherit from <see cref="ReferenceDataBase"/>).</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> IsValid<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, LText? errorText = null)
            where TEntity : class
            where TProperty : ReferenceDataBase?
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ReferenceDataRule<TEntity, TProperty> { ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="ReferenceDataSidListBase"/> validation (see <see cref="ReferenceDataRule{TEntity, TProperty}"/>) to ensure the list of SIDs are valid.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/> (must inherit from <see cref="ReferenceDataSidListBase"/>).</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="allowDuplicates">Indicates whether duplicate values are allowed.</param>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty?> AreValid<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty?> rule, bool allowDuplicates = false, int minCount = 0, int? maxCount = null, LText? errorText = null)
            where TEntity : class
            where TProperty : ReferenceDataSidListBase
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new ReferenceDataSidListRule<TEntity, TProperty> { AllowDuplicates = allowDuplicates, MinCount = minCount, MaxCount = maxCount, ErrorText = errorText });
        }

        /// <summary>
        /// Adds a <see cref="ReferenceDataBase.Code"/> validation (see <see cref="ReferenceDataCodeRule{TEntity, TRefData}"/> to ensure the <c>Code</c> is valid.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static ReferenceDataCodeRuleAs<TEntity> RefDataCode<TEntity>(this PropertyRuleBase<TEntity, string?> rule, LText? errorText = null)
            where TEntity : class
        {
            return new ReferenceDataCodeRuleAs<TEntity>(Check.NotNull(rule, nameof(rule)), errorText);
        }

        #endregion

        #region Collection

        /// <summary>
        /// Adds a collection (<see cref="System.Collections.IEnumerable"/>) validation (see <see cref="CollectionRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="item">The item <see cref="ICollectionRuleItem"/> configuration.</param>
        /// <param name="allowNullItems">Indicates whether the underlying collection item must not be null.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Collection<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, int minCount = 0, int? maxCount = null, ICollectionRuleItem? item = null, bool allowNullItems = false)
            where TEntity : class
            where TProperty : System.Collections.IEnumerable?
        {
            var cr = new CollectionRule<TEntity, TProperty> { MinCount = minCount, MaxCount = maxCount, Item = item, AllowNullItems = allowNullItems };
            return Check.NotNull(rule, nameof(rule)).AddRule(cr);
        }

        #endregion

        #region Dictionary

        /// <summary>
        /// Adds a dictionary (<see cref="System.Collections.IDictionary"/>) validation (see <see cref="DictionaryRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="item">The item <see cref="IDictionaryRuleItem"/> configuration.</param>
        /// <param name="allowNullKeys">Indicates whether the underlying dictionary keys must not be null.</param>
        /// <param name="allowNullValues">Indicates whether the underlying dictionary values must not be null.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Dictionary<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, int minCount = 0, int? maxCount = null, IDictionaryRuleItem? item = null, bool allowNullKeys = false, bool allowNullValues = false)
            where TEntity : class
            where TProperty : System.Collections.IDictionary?
        {
            var cr = new DictionaryRule<TEntity, TProperty> { MinCount = minCount, MaxCount = maxCount, Item = item, AllowNullKeys = allowNullKeys, AllowNullValues = allowNullValues };
            return Check.NotNull(rule, nameof(rule)).AddRule(cr);
        }

        #endregion

        #region Entity

        /// <summary>
        /// Adds an entity validation (see <see cref="EntityRule{TEntity, TProperty, TValidator}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The validator <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="validator">The validator.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Entity<TEntity, TProperty, TValidator>(this PropertyRuleBase<TEntity, TProperty> rule, TValidator validator)
            where TEntity : class
            where TProperty : class?
            where TValidator : IValidator
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new EntityRule<TEntity, TProperty, TValidator>(validator));
        }

        /// <summary>
        /// Enables the addition of an <see cref="EntityRule{TEntity, TProperty, TValidator}"/> using a validator <see cref="EntityRuleWith{TEntity, TProperty}.With{TValidator}"/> a specified validator <see cref="Type"/>
        /// (leverages the underlying <see cref="ExecutionContext.GetService{T}(bool)">service provider</see> to get the instance at runtime).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <returns>An <see cref="EntityRuleWith{TEntity, TProperty}"/>.</returns>
        public static EntityRuleWith<TEntity, TProperty> Entity<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule)
            where TEntity : class
            where TProperty : class?
        {
            return new EntityRuleWith<TEntity, TProperty>(Check.NotNull(rule, nameof(rule)));
        }

        #endregion

        #region Custom

        /// <summary>
        /// Adds a <paramref name="custom"/> validation (see <see cref="CustomRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="custom">The custom action.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Custom<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Action<PropertyContext<TEntity, TProperty>> custom) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new CustomRule<TEntity, TProperty>(custom));
        }

        /// <summary>
        /// Adds a <paramref name="customAsync"/> validation (see <see cref="CustomRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="customAsync">The custom function.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Custom<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<PropertyContext<TEntity, TProperty>, Task> customAsync) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new CustomRule<TEntity, TProperty>(customAsync));
        }

        #endregion

        #region Common

        /// <summary>
        /// Adds a common validation (see <see cref="CommonRule{TEntity, TProperty}"/>).
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param>
        /// <param name="validator">The <see cref="CommonValidator{T}"/>.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Common<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, CommonValidator<TProperty> validator) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new CommonRule<TEntity, TProperty>(validator));
        }

        #endregion

        #region ValueValidator

        /// <summary>
        /// Enables (sets up) validation for a value.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The value name (defaults to <see cref="Validator.ValueNameDefault"/>).</param>
        /// <param name="text">The friendly text name used in validation messages (defaults to <paramref name="name"/> as sentence case where not specified).</param>
        /// <returns>A <see cref="ValueValidator{T}"/>.</returns>
        public static ValueValidator<T> Validate<T>(this T value, string? name = null, LText? text = null)
        {
            return new ValueValidator<T>(value, name, text);
        }

        #endregion

        #region ToSentenceCase

        /// <summary>
        /// Converts a <see cref="string"/> into sentence case.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The <see cref="string"/> as sentence case.</returns>
        /// <remarks>For example a value of 'VarNameDB' would return 'Var Name DB'.</remarks>
        public static string? ToSentenceCase(this string? value)
        {
            return StringConversion.ToSentenceCase(value);
        }

        #endregion

        #region Override/Default

        /// <summary>
        /// Adds a value override (see <see cref="OverrideRule{TEntity, TProperty}"/>) using the specified <paramref name="overrideFunc"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param> 
        /// <param name="overrideFunc">The override function.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Override<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, TProperty> overrideFunc) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new OverrideRule<TEntity, TProperty>(overrideFunc));
        }

        /// <summary>
        /// Adds a value override (see <see cref="OverrideRule{TEntity, TProperty}"/>) using the specified <paramref name="overrideFuncAsync"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param> 
        /// <param name="overrideFuncAsync">The override function.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Override<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, Task<TProperty>> overrideFuncAsync) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new OverrideRule<TEntity, TProperty>(overrideFuncAsync));
        }

        /// <summary>
        /// Adds a value override (see <see cref="OverrideRule{TEntity, TProperty}"/>) using the specified <paramref name="overrideValue"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param> 
        /// <param name="overrideValue">The override value.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Override<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, TProperty overrideValue) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new OverrideRule<TEntity, TProperty>(overrideValue));
        }

        /// <summary>
        /// Adds a default (see <see cref="OverrideRule{TEntity, TProperty}"/>) using the specified <paramref name="defaultFunc"/> (overrides only where current value is the default for <see cref="Type"/>) .
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param> 
        /// <param name="defaultFunc">The override function.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Default<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, TProperty> defaultFunc) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new OverrideRule<TEntity, TProperty>(defaultFunc) { OnlyOverrideDefault = true });
        }

        /// <summary>
        /// Adds a default (see <see cref="OverrideRule{TEntity, TProperty}"/>) using the specified <paramref name="defaultFuncAsync"/> (overrides only where current value is the default for <see cref="Type"/>) .
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param> 
        /// <param name="defaultFuncAsync">The override function.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Default<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, Func<TEntity, Task<TProperty>> defaultFuncAsync) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new OverrideRule<TEntity, TProperty>(defaultFuncAsync) { OnlyOverrideDefault = true });
        }

        /// <summary>
        /// Adds a default override (see <see cref="OverrideRule{TEntity, TProperty}"/>) using the specified <paramref name="defaultValue"/> (overrides only where current value is the default for <see cref="Type"/>) .
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="rule">The <see cref="PropertyRule{TEntity, TProperty}"/> being extended.</param> 
        /// <param name="defaultValue">The override value.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public static PropertyRuleBase<TEntity, TProperty> Default<TEntity, TProperty>(this PropertyRuleBase<TEntity, TProperty> rule, TProperty defaultValue) where TEntity : class
        {
            return Check.NotNull(rule, nameof(rule)).AddRule(new OverrideRule<TEntity, TProperty>(defaultValue) { OnlyOverrideDefault = true });
        }

        #endregion
    }
}