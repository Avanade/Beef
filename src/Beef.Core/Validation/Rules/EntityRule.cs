// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides entity validation.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValidator">The property validator <see cref="Type"/>.</typeparam>
    public class EntityRule<TEntity, TProperty, TValidator> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
        where TProperty : class
        where TValidator : Validator<TProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRule{TEntity, TProperty, TValidator}"/> class.
        /// </summary>
        /// <param name="validator">The <see cref="Beef.Validation.Validator{TProperty, TValidator}"/>.</param>
        public EntityRule(Validator<TProperty> validator)
        {
            Validator = Beef.Check.NotNull(validator, nameof(validator));
        }

        /// <summary>
        /// Gets the <see cref="Beef.Validation.Validator{TProperty, TValidator}"/>.
        /// </summary>
        public Validator<TProperty> Validator { get; private set; }

        /// <summary>
        /// Overrides the <b>Check</b> method and will not validate where performing a shallow validation.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        public override bool Check(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));
            return context.Parent.ShallowValidation ? false : base.Check(context);
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            // Exit where nothing to validate.
            Beef.Check.NotNull(context, nameof(context));
            if (context.Value == null)
                return;

            // Create the context args.
            var args = context.CreateValidationArgs();

            // Validate and merge.
            context.MergeResult(Validator.Validate(context.Value, args));
        }
    }
}