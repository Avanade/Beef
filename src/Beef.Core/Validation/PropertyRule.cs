// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Reflection;
using Beef.Validation.Clauses;
using Beef.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Beef.Validation
{
    /// <summary>
    /// Provides a validation rule for a property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public interface IPropertyRule<TEntity> where TEntity : class
    {
        /// <summary>
        /// Validates an entity given a <see cref="ValidationContext{TEntity}"/>.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/></param>
        void Validate(ValidationContext<TEntity> context);
    }

    /// <summary>
    /// Represents a base validation rule for an entity property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public abstract class PropertyRuleBase<TEntity, TProperty> where TEntity : class
    {
        private readonly List<IValueRule<TEntity, TProperty>> _rules = new List<IValueRule<TEntity, TProperty>>();
        private readonly List<IPropertyRuleClause<TEntity>> _clauses = new List<IPropertyRuleClause<TEntity>>();

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the JSON property name.
        /// </summary>
        public string JsonName { get; protected set; }

        /// <summary>
        /// Gets or sets the friendly text name used in validation messages.
        /// </summary>
        public LText Text { get; set; }

        /// <summary>
        /// Adds a rule (<see cref="IValueRule{TEntity, TProperty}"/>) to the property.
        /// </summary>
        /// <param name="rule">The <see cref="IValueRule{TEntity, TProperty}"/>.</param>
        /// <returns>The <see cref="PropertyRuleBase{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> AddRule(IValueRule<TEntity, TProperty> rule)
        {
            _rules.Add(rule);
            return this;
        }

        /// <summary>
        /// Adds a clause (<see cref="IPropertyRuleClause{TEntity, TProperty}"/>) to the rule.
        /// </summary>
        /// <param name="clause">The <see cref="IPropertyRuleClause{TEntity, TProperty}"/>.</param>
        public void AddClause(IPropertyRuleClause<TEntity> clause)
        {
            if (clause == null)
                return;

            if (_rules.Count == 0)
                _clauses.Add(clause);
            else
                _rules.Last().AddClause(clause);
        }

        /// <summary>
        /// Runs the configured clauses and rules.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        protected void Invoke(PropertyContext<TEntity, TProperty> context)
        {
            Check.NotNull(context, nameof(context));

            // Check all "this" clauses.
            foreach (var clause in _clauses)
            {
                if (!clause.Check(context))
                    return;
            }

            // Check and execute all rules/clauses within the rules stack.
            foreach (var rule in _rules)
            {
                if (rule.Check(context))
                    rule.Validate(context);

                // Stop validating after an error.
                if (context.HasError)
                    break;
            }
        }

        /// <summary>
        /// Runs the validation for the property value.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public abstract ValueValidatorResult<TEntity, TProperty> Run(bool throwOnError = false);

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> where the <typeparamref name="TEntity"/> <paramref name="predicate"/> must be <c>true</c> for the rule to be validated.
        /// </summary>
        /// <param name="predicate">A function to determine whether the preceeding rule is to be validated.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> When(Predicate<TEntity> predicate)
        {
            if (predicate == null)
                return this;

            AddClause(new WhenClause<TEntity, TProperty>(predicate));
            return this;
        }

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> where the <typeparamref name="TProperty"/> <paramref name="predicate"/> must be <c>true</c> for the rule to be validated.
        /// </summary>
        /// <param name="predicate">A function to determine whether the preceeding rule is to be validated.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> WhenValue(Predicate<TProperty> predicate)
        {
            if (predicate == null)
                return this;

            AddClause(new WhenClause<TEntity, TProperty>(predicate));
            return this;
        }

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> where the <typeparamref name="TProperty"/> must have a value (i.e. not the default value for the Type) for the rule to be validated.
        /// </summary>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> WhenHasValue()
        {
            return WhenValue((TProperty pv) => Comparer<TProperty>.Default.Compare(pv, default(TProperty)) != 0);
        }

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> which must be <c>true</c> for the rule to be validated.
        /// </summary>
        /// <param name="when">A function to determine whether the preceeding rule is to be validated.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> When(Func<bool> when)
        {
            if (when == null)
                return this;

            AddClause(new WhenClause<TEntity, TProperty>(when));
            return this;
        }

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> which must be <c>true</c> for the rule to be validated.
        /// </summary>
        /// <param name="when">A <see cref="Boolean"/> to determine whether the preceeding rule is to be validated.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> When(bool when)
        {
            AddClause(new WhenClause<TEntity, TProperty>(() => when));
            return this;
        }

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> that states that the
        /// <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext"/> <see cref="ExecutionContext.OperationType"/> is equal to the specified
        /// (<paramref name="operationType"/>).
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/>.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> WhenOperation(OperationType operationType)
        {
            return When(x => ExecutionContext.Current.OperationType == operationType);
        }

        /// <summary>
        /// Adds a <see cref="WhenClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> that states that the
        /// <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext"/> <see cref="ExecutionContext.OperationType"/> is not equal to the specified
        /// (<paramref name="operationType"/>).
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/>.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> WhenNotOperation(OperationType operationType)
        {
            return When(x => ExecutionContext.Current.OperationType != operationType);
        }

        /// <summary>
        /// Adds a <see cref="DependsOnClause{TEntity, TProperty}"/> to this <see cref="PropertyRule{TEntity, TProperty}"/> which must be <c>true</c> for the rule to be validated.
        /// </summary>
        /// <param name="expression">A depends on expression.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> DependsOn<TDependsProperty>(Expression<Func<TEntity, TDependsProperty>> expression)
        {
            if (expression == null)
                return this;

            AddClause(new DependsOnClause<TEntity, TDependsProperty>(expression));
            return this;
        }
    }

    /// <summary>
    /// Represents a validation rule for an entity property.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class PropertyRule<TEntity, TProperty> : PropertyRuleBase<TEntity, TProperty>, IPropertyRule<TEntity>, IValueRule<TEntity, TProperty> where TEntity : class
    {
        private readonly PropertyExpression<TEntity, TProperty> _property;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRule{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="propertyExpression">The <see cref="LambdaExpression"/> to reference the entity property.</param>
        public PropertyRule(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            _property = PropertyExpression.Create(propertyExpression, true);
            Name = _property.Name;
            JsonName = _property.JsonName;
            Text = _property.Text;
        }

        /// <summary>
        /// Validates an entity given a <see cref="ValidationContext{TEntity}"/>.
        /// </summary>
        /// <param name="context">The <see cref="ValidationContext{TEntity}"/>.</param>
        public void Validate(ValidationContext<TEntity> context)
        {
            Check.NotNull(context, nameof(context));

            if (context.Value == null)
                return;

            // Where validating a specific property then make sure the names match.
            if (context.SelectedPropertyName != null && context.SelectedPropertyName != Name)
                return;

            // Ensure that the property does not already have an error.
            if (context.HasError(_property))
                return;

            // Get the property value and create the property context.
            var value = _property.GetValue(context.Value);
            var ctx = new PropertyContext<TEntity, TProperty>(context, value, this.Name, this.JsonName, this.Text);

            // Run the rules.
            Invoke(ctx);
        }

        /// <summary>
        /// Adds a rule (<see cref="IValueRule{TEntity, TProperty}"/>) to the property.
        /// </summary>
        /// <param name="rule">The <see cref="IValueRule{TEntity, TProperty}"/>.</param>
        /// <returns>The <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public new PropertyRule<TEntity, TProperty> AddRule(IValueRule<TEntity, TProperty> rule)
        {
            base.AddRule(rule);
            return this;
        }

        /// <summary>
        /// Checks the clauses.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        bool IValueRule<TEntity, TProperty>.Check(IPropertyContext context)
        {
            // All good, nothing to see here ;-)
            throw new NotSupportedException("A property value clauses check should not occur directly on a PropertyRule.");
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        void IValueRule<TEntity, TProperty>.Validate(PropertyContext<TEntity, TProperty> context)
        {
            // All good, nothing to see here ;-)
            throw new NotSupportedException("A property value validation should not occur directly on a PropertyRule.");
        }

        /// <summary>
        /// Adds a clause (<see cref="IPropertyRuleClause{TEntity}"/>) to the rule.
        /// </summary>
        /// <param name="clause">The <see cref="IPropertyRuleClause{TEntity}"/>.</param>
        void IValueRule<TEntity, TProperty>.AddClause(IPropertyRuleClause<TEntity> clause)
        {
            AddClause(clause);
        }

        /// <summary>
        /// The <b>Run</b> method is not supported; use <see cref="Validate(ValidationContext{TEntity})"/> instead.
        /// </summary>
        /// <param name="throwOnError">Indicates to throw a <see cref="ValidationException"/> where an error was found.</param>
        /// <returns>A <see cref="ValueValidatorResult{TEntity, TProperty}"/>.</returns>
        public override ValueValidatorResult<TEntity, TProperty> Run(bool throwOnError = false)
        {
            throw new NotSupportedException("The Run method is not supported for a PropertyRule<TEntity, TProperty>.");
        }
    }
}
