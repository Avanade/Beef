// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

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
        where TProperty : class?
        where TValidator : IValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRule{TEntity, TProperty, TValidator}"/> class.
        /// </summary>
        /// <param name="validator">The <see cref="Beef.Validation.Validator{TProperty}"/>.</param>
        public EntityRule(TValidator validator) => Validator = validator ?? throw new ArgumentNullException(nameof(validator));

        /// <summary>
        /// Gets the <see cref="Beef.Validation.IValidator"/>.
        /// </summary>
        public TValidator Validator { get; private set; }

        /// <summary>
        /// Overrides the <b>Check</b> method and will not validate where performing a shallow validation.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        /// <returns><c>true</c> where validation is to continue; otherwise, <c>false</c> to stop.</returns>
        public override bool Check(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));
            return !context.Parent.ShallowValidation && base.Check(context);
        }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override async Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            // Exit where nothing to validate.
            Beef.Check.NotNull(context, nameof(context));
            if (context.Value == null)
                return;

            // Create the context args.
            var args = context.CreateValidationArgs();

            // Validate and merge.
            context.MergeResult(await Validator.ValidateAsync(context.Value, args).ConfigureAwait(false));
        }
    }

    /// <summary>
    /// Provides a means to add an <see cref="EntityRule{TEntity, TProperty, TValidator}"/> using a validator <see cref="With"/> a specified validator <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    public class EntityRuleWith<TEntity, TProperty> 
        where TEntity : class
        where TProperty : class?
    {
        private readonly PropertyRuleBase<TEntity, TProperty> _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRuleWith{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="PropertyRuleBase{TEntity, TProperty}"/>.</param>
        public EntityRuleWith(PropertyRuleBase<TEntity, TProperty> parent) => _parent = Check.NotNull(parent, nameof(parent));

        /// <summary>
        /// Adds an <see cref="EntityRule{TEntity, TProperty, TValidator}"/> using a validator <see cref="With"/> a specified <typeparamref name="TValidator"/>
        /// (leverages the underlying <see cref="ExecutionContext.GetService{T}(bool)">service provider</see> to get the instance at runtime).
        /// </summary>
        /// <typeparam name="TValidator">The property validator <see cref="Type"/>.</typeparam>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>; defaults to <see cref="ExecutionContext.ServiceProvider"/> where not specified.</param>
        /// <returns>A <see cref="PropertyRule{TEntity, TProperty}"/>.</returns>
        public PropertyRuleBase<TEntity, TProperty> With<TValidator>(IServiceProvider? serviceProvider = null) where TValidator : IValidator
        {
            _parent.AddRule(new EntityRule<TEntity, TProperty, TValidator>(Validator.Create<TValidator>(serviceProvider)));
            return _parent;
        }
    }
}