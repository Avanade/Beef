// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides mandatory validation; determined as mandatory when it contains its default value.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TProperty">The property <see cref="Type"/>.</typeparam>
    /// <remarks>A value will be determined as mandatory when it contains its default value. For example an <see cref="Int32"/> will trigger when the value is zero; however, a
    /// <see cref="Nullable{Int32}"/> will trigger when null only (a zero is considered a value in this instance).</remarks>
    public class MandatoryRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty> where TEntity : class
    {
        /// <summary>
        /// Validate the property value.
        /// </summary> 
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override Task ValidateAsync(PropertyContext<TEntity, TProperty> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            // Compare the value against its default.
            if (Comparer<TProperty>.Default.Compare(context.Value, default!) == 0)
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MandatoryFormat);
                return Task.CompletedTask;
            }

            // Also check for empty strings.
            if (context.Value is string val && val.Length == 0)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MandatoryFormat);

            return Task.CompletedTask;
        }
    }
}