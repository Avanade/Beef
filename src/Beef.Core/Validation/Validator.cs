// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Beef.Validation
{
    /// <summary>
    /// Provides entity validation with a <see cref="Default"/> instance.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValidator">The validator <see cref="Type"/>.</typeparam>
    public abstract class Validator<TEntity, TValidator> : Validator<TEntity>
        where TEntity : class
        where TValidator : Validator<TEntity, TValidator>, new()
    {
        private readonly static TValidator _default = new TValidator();

#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, results in a consistent static defined default instance without the need to specify generic type to consume.
        /// <summary>
        /// Gets the current instance of the validator.
        /// </summary>
        public static TValidator Default
#pragma warning restore CA1000
        {
            get
            {
                if (_default == null)
                    throw new InvalidOperationException("An instance of this Validator cannot be referenced as it is still being constructed; beware that you may have a circular reference within the constructor.");

                return _default;
            }
        }
    }

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
        /// Creates a <see cref="Validator{TEntity}"/>.
        /// </summary>
        /// <returns>A <see cref="Validator{TEntity}"/>.</returns>
        public static Validator<TEntity> Create<TEntity>() where TEntity : class
        {
            return new Validator<TEntity>();
        }
    }

    /// <summary>
    /// Provides entity validation.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public class Validator<TEntity> : ValidatorBase<TEntity>
        where TEntity : class
    {
        private RuleSet<TEntity>? _currentRuleSet;
        private Action<ValidationContext<TEntity>>? _additional;

        /// <summary>
        /// Validate the entity value with specified <see cref="ValidationArgs"/>.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="args">An optional <see cref="ValidationArgs"/>.</param>
        /// <returns>The resulting <see cref="ValidationContext{TEntity}"/>.</returns>
        public override ValidationContext<TEntity> Validate(TEntity value, ValidationArgs? args = null)
        {
            var context = new ValidationContext<TEntity>(value, args ?? new ValidationArgs());

            // Validate each of the property rules.
            foreach (var rule in Rules)
            {
                rule.Validate(context);
            }

            OnValidate(context);
            _additional?.Invoke(context);
            return context;
        }

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added by the inheriting classes.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/>.</param>
        protected virtual void OnValidate(ValidationContext<TEntity> context)
        {
        }

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
        /// Adds the <see cref="PropertyRule{TEntity, TProperty}"/> to the validator allowing enabling additional configuration via the specified <paramref name="property"/> action.
        /// </summary>
        /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression"/> to reference the entity property.</param>
        /// <param name="property">The action to act of the created <see cref="PropertyRule{TEntity, TProperty}"/>.</param>
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
        /// <param name="include">The <see cref="IncludeBaseRule{TEntity, TInclude}"/> to add.</param>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> IncludeBase<TInclude>(ValidatorBase<TInclude> include)
            where TInclude : class
        {
            if (!typeof(TEntity).GetTypeInfo().IsSubclassOf(typeof(TInclude)))
                throw new ArgumentException($"Type {typeof(TEntity).Name} must inherit from {typeof(TInclude).Name}.");

            if (_currentRuleSet == null)
                base.Rules.Add(new IncludeBaseRule<TEntity, TInclude>(include));
            else
                _currentRuleSet.Rules.Add(new IncludeBaseRule<TEntity, TInclude>(include));

            return this;
        }

        /// <summary>
        /// Validate the entity value (post all configured property rules) enabling additional validation logic to be added.
        /// </summary>
        /// <param name="additional">The action to invoke.</param>
        /// <returns>The <see cref="Validator{TEntity}"/>.</returns>
        public Validator<TEntity> Additional(Action<ValidationContext<TEntity>> additional)
        {
            Check.NotNull(additional, nameof(additional));

            if (_additional != null)
                throw new InvalidOperationException("Additional can only be defined once for a Validator.");

            _additional = additional;
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