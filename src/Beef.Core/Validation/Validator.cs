// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using Beef.Validation.Rules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Provides access to the validator capabilities.
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// Gets or sets the default value name (used by the <see cref="ValueValidator{T}"/>).
        /// </summary>
        public static string ValueNameDefault { get; set; } = "Value";

        /// <summary>
        /// Gets or sets the default key name (used by the <see cref="DictionaryValidator{TDict, TKey, TValue}"/>).
        /// </summary>
        public static string KeyNameDefault { get; set; } = "Key";

        /// <summary>
        /// Creates a <see cref="Validator{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <returns>A <see cref="Validator{TEntity}"/>.</returns>
        public static Validator<TEntity> Create<TEntity>() where TEntity : class => new();

        /// <summary>
        /// Creates (or gets) an instance leveraging the underlying <see cref="ExecutionContext.GetService{T}(bool)">service provider</see> to get the instance.
        /// </summary>
        /// <typeparam name="TValidator">The validator <see cref="Type"/>.</typeparam>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>; defaults to <see cref="ExecutionContext.ServiceProvider"/> where not specified.</param>
        /// <returns>The <typeparamref name="TValidator"/> instance.</returns>
        public static TValidator Create<TValidator>(IServiceProvider? serviceProvider = null) where TValidator : IValidator
            => serviceProvider == null
                ? ExecutionContext.GetService<TValidator>(throwExceptionOnNull: true)!
                : (serviceProvider.GetService<TValidator>() ?? throw new InvalidOperationException($"Attempted to get service '{typeof(TValidator).FullName}' but null was returned; this would indicate that the service has not been configured correctly."));

        /// <summary>
        /// Creates a <see cref="CollectionValidator{TColl, TItem}"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="item">The item <see cref="ICollectionRuleItem"/> configuration.</param>
        /// <param name="allowNullItems">Indicates whether the underlying collection item must not be null.</param>
        /// <returns>The <see cref="CollectionValidator{TColl, TItem}"/>.</returns>
        public static CollectionValidator<TColl, TItem> CreateCollection<TColl, TItem>(int minCount = 0, int? maxCount = null, ICollectionRuleItem? item = null, bool allowNullItems = false) where TColl : class, IEnumerable<TItem> => 
            new() { MinCount = minCount, MaxCount = maxCount, Item = item, AllowNullItems = allowNullItems };

        /// <summary>
        /// Creates a <see cref="DictionaryValidator{TDict, TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TDict">The dictionary <see cref="Type"/>.</typeparam>
        /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValue">The value <see cref="Type"/>.</typeparam>
        /// <param name="minCount">The minimum count.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="item">The item <see cref="ICollectionRuleItem"/> configuration.</param>
        /// <param name="allowNullKeys">Indicates whether the underlying dictionary key can be null.</param>
        /// <param name="allowNullValues">Indicates whether the underlying dictionary value can be null.</param>
        /// <returns>The <see cref="CollectionValidator{TColl, TItem}"/>.</returns>
        public static DictionaryValidator<TDict, TKey, TValue> CreateDictionary<TDict, TKey, TValue>(int minCount = 0, int? maxCount = null, IDictionaryRuleItem? item = null, bool allowNullKeys = false, bool allowNullValues = false) where TDict : class, IDictionary<TKey, TValue> =>
            new() { MinCount = minCount, MaxCount = maxCount, Item = item, AllowNullKeys = allowNullKeys, AllowNullValues = allowNullValues };

        /// <summary>
        /// Creates a <see cref="CommonValidator{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="validator">An action with the <see cref="CommonValidator{T}"/>.</param>
        /// <returns>The <see cref="CommonValidator{T}"/>.</returns>
        public static CommonValidator<T> CreateCommon<T>(Action<CommonValidator<T>> validator) => CommonValidator.Create<T>(validator);
    }

    /// <summary>
    /// Provides entity validation.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public class Validator<TEntity> : ValidatorBase<TEntity>
        where TEntity : class
    {
        private RuleSet<TEntity>? _currentRuleSet;
        private Func<ValidationContext<TEntity>, Task>? _additionalAsync;

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value to validate.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public override async Task<ValidationContext<TEntity>> ValidateAsync(TEntity value, ValidationArgs? args = null)
        {
            var context = new ValidationContext<TEntity>(value, args ?? new ValidationArgs());

            // Validate each of the property rules.
            foreach (var rule in Rules)
            {
                await rule.ValidateAsync(context).ConfigureAwait(false);
            }

            await OnValidateAsync(context).ConfigureAwait(false);
            if (_additionalAsync != null)
                await _additionalAsync(context).ConfigureAwait(false);

            return context;
        }

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added by the inheriting classes.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/>.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        protected virtual Task OnValidateAsync(ValidationContext<TEntity> context) => Task.CompletedTask;

        /// <summary>
        /// Adds a <see cref="PropertyRule{TEntity, TProperty}"/> to the validator.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public override PropertyRule<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            // Depending on the the state update either the ruleset rules or the underlying rules.
            if (_currentRuleSet == null)
                return base.Property(propertyExpression);

            return _currentRuleSet.Property(propertyExpression);
        }

        /// <summary>
        /// Adds the <see cref="PropertyRule{TEntity, TProperty}"/> to the validator enabling additional configuration via the specified <paramref name="property"/> action.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <param name="property">The action to act on the created <see cref="PropertyRule{TEntity, TProperty}"/>.</param>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> HasProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Action<PropertyRule<TEntity, TProperty>>? property = null)
        {
            var p = Property(propertyExpression);
            property?.Invoke(p);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IncludeBaseRule{TEntity, TInclude}"/> to the validator to enable a base validator to be included within the validator rule set.
        /// </summary>
        /// <typeparam name="TInclude">The include <see cref="Type"/> in which <typeparamref name="TEntity"/> inherits from.</typeparam>
        /// <param name="include">The <see cref="IValidator{TInclude}"/> to include (add).</param>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> IncludeBase<TInclude>(IValidator<TInclude> include)
            where TInclude : class
        {
            Check.NotNull(include, nameof(include));

            if (!typeof(TEntity).GetTypeInfo().IsSubclassOf(typeof(TInclude)))
                throw new ArgumentException($"Type {typeof(TEntity).Name} must inherit from {typeof(TInclude).Name}.");

            if (_currentRuleSet == null)
                base.Rules.Add(new IncludeBaseRule<TEntity, TInclude>(include));
            else
                _currentRuleSet.Rules.Add(new IncludeBaseRule<TEntity, TInclude>(include));

            return this;
        }

        /// <summary>
        /// Adds a <see cref="IncludeBaseRule{TEntity, TInclude}"/> to the validator to enable a base validator to be included within the validator rule set leveraging the underlying
        /// <see cref="ExecutionContext.GetService{T}(bool)">service provider</see> to get the instance.
        /// </summary>
        /// <typeparam name="TInclude">The include <see cref="Type"/> in which <typeparamref name="TEntity"/> inherits from.</typeparam>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> IncludeBase<TInclude>()
            where TInclude : class
        {
            return IncludeBase(ExecutionContext.GetService<IValidator<TInclude>>(throwExceptionOnNull: true)!);
        }

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added.
        /// </summary>
        /// <param name="additionalAsync">The asynchronous function to invoke.</param>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> Additional(Func<ValidationContext<TEntity>, Task> additionalAsync)
        {
            Check.NotNull(additionalAsync, nameof(additionalAsync));

            if (_additionalAsync != null)
                throw new InvalidOperationException("Additional can only be defined once for a Validator.");

            _additionalAsync = additionalAsync;
            return this;
        }

        /// <summary>
        /// Adds a <see cref="RuleSet(Predicate{ValidationContext{TEntity}}, Action)"/> that is conditionally invoked where the <paramref name="predicate"/> is true.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="action">The action to invoke where the <see cref="Property"/> method will update the corresponding <see cref="RuleSet(Predicate{ValidationContext{TEntity}}, Action)"/> <see cref="ValidatorBase{TEntity}.Rules">rules</see>.</param>
        /// <returns>The <see cref="Beef.Validation.RuleSet{TEntity}"/>.</returns>
        public RuleSet<TEntity> RuleSet(Predicate<ValidationContext<TEntity>> predicate, Action action)
        {
            Check.NotNull(predicate, nameof(predicate));
            Check.NotNull(action, nameof(action));
            return SetRuleSet(new RuleSet<TEntity>(predicate), (v) => action?.Invoke());
        }

        /// <summary>
        /// Adds a <see cref="RuleSet(Predicate{ValidationContext{TEntity}}, Action)"/> that is conditionally invoked where the <paramref name="predicate"/> is true.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="action">The action to invoke where the passed <see cref="Validator{TEntity}"/> enables the <see cref="RuleSet(Predicate{ValidationContext{TEntity}}, Action)"/> <see cref="ValidatorBase{TEntity}.Rules">rules</see> to be updated.</param>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> HasRuleSet(Predicate<ValidationContext<TEntity>> predicate, Action<Validator<TEntity>> action)
        {
            Check.NotNull(predicate, nameof(predicate));
            Check.NotNull(action, nameof(action));
            SetRuleSet(new RuleSet<TEntity>(predicate), action);
            return this;
        }

        /// <summary>
        /// Sets the rule set and invokes the action.
        /// </summary>
        private RuleSet<TEntity> SetRuleSet(RuleSet<TEntity> ruleSet, Action<Validator<TEntity>> action)
        {
            if (_currentRuleSet != null)
                throw new InvalidOperationException("RuleSets only support a single level of nesting.");

            // Invoke the action that will add the entries to the ruleset not the underlying rules.
            if (action != null)
            {
                _currentRuleSet = ruleSet;
                action(this);
                _currentRuleSet = null;
            }

            // Add the ruleset to the rules.
            Rules.Add(ruleSet);
            return ruleSet;
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> where the <see cref="MessageItem"/> <see cref="MessageItem.Property"/> is set based on the <paramref name="propertyExpression"/>.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <param name="text">The message text.</param>
        public void ThrowValidationException<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText text)
        {
            var p = PropertyExpression.Create(propertyExpression, true);
            throw new ValidationException(MessageItem.CreateErrorMessage(ValidationArgs.DefaultUseJsonNames ? p.JsonName : p.Name, text));
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> where the <see cref="MessageItem"/> <see cref="MessageItem.Property"/> is set based on the <paramref name="propertyExpression"/>. The property
        /// friendly text and <paramref name="propertyValue"/> are automatically passed as the first two arguments to the string formatter.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="propertyValue">The property values (to be used as part of the format).</param>
        /// <param name="values"></param>
        public void ThrowValidationException<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, LText format, TProperty propertyValue, params object[] values)
        {
            var p = PropertyExpression.Create(propertyExpression, true);
            throw new ValidationException(MessageItem.CreateErrorMessage(ValidationArgs.DefaultUseJsonNames ? p.JsonName : p.Name,
                string.Format(System.Globalization.CultureInfo.CurrentCulture, format, new object[] { p.Text, propertyValue! }.Concat(values).ToArray())));
        }
    }
}