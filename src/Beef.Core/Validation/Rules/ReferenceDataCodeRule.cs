// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides validation for a <see cref="ReferenceDataBase.Code"/>; validates that the <see cref="ReferenceDataBase.IsValid"/>.
    /// </summary>
    public class ReferenceDataCodeRule<TEntity, TRefData> : ValueRuleBase<TEntity, string?>
        where TEntity : class
        where TRefData : ReferenceDataBase?
    {
        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override Task ValidateAsync(PropertyContext<TEntity, string?> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            if (string.IsNullOrEmpty(context.Value))
                return Task.CompletedTask;

            var rd = ReferenceDataManager.Current[typeof(TRefData)].GetByCode(context.Value);
            if (rd == null || !rd.IsValid)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.InvalidFormat);

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Provides a means to add an <see cref="ReferenceDataCodeRule{TEntity, TRefData}"/> using a validator <see cref="As"/> a specified <see cref="ReferenceDataBase"/> <see cref="Type"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    public class ReferenceDataCodeRuleAs<TEntity>
        where TEntity : class
    {
        private readonly PropertyRuleBase<TEntity, string?> _parent;
        private readonly LText? _errorText;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRuleWith{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="PropertyRuleBase{TEntity, TProperty}"/>.</param>
        /// <param name="errorText">The error message format text <see cref="LText"/> (overrides the default).</param>
        public ReferenceDataCodeRuleAs(PropertyRuleBase<TEntity, string?> parent, LText? errorText = null)
        {
            _parent = Check.NotNull(parent, nameof(parent));
            _errorText = errorText;
        }

        /// <summary>
        /// Adds an <see cref="ReferenceDataCodeRule{TEntity, TRefData}"/> using a validator <see cref="As"/> a specified <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="TRefData">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
        /// <returns>A <see cref="ReferenceDataCodeRule{TEntity, TRefData}"/>.</returns>
        public PropertyRuleBase<TEntity, string?> As<TRefData>() where TRefData : ReferenceDataBase
        {
            _parent.AddRule(new ReferenceDataCodeRule<TEntity, TRefData>() { ErrorText = _errorText });
            return _parent;
        }
    }
}