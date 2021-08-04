// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;
using System.Threading.Tasks;

namespace Beef.Validation
{
    /// <summary>
    /// Represents the base <see cref="ReferenceDataBase"/> validator with a <see cref="Default"/> instance.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TValidator">The <see cref="ReferenceDataValidatorBase{TEntity, TValidator}"/> <see cref="Type"/>.</typeparam>
    public abstract class ReferenceDataValidatorBase<TEntity, TValidator> : ReferenceDataValidator<TEntity>
        where TEntity : ReferenceDataBase
        where TValidator : ReferenceDataValidatorBase<TEntity, TValidator>, new()
    {
        private static readonly TValidator _default = new();

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
    /// Represents the base <see cref="ReferenceDataBase"/> validator.
    /// </summary>
    /// <typeparam name="TEntity">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
    public class ReferenceDataValidator<TEntity> : Validator<TEntity>
        where TEntity : ReferenceDataBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataValidator{TEntity}"/> class.
        /// </summary>
        public ReferenceDataValidator()
        {
            Property(x => x.Id).Custom(ValidateIdAsync);
            Property(x => x.Code).Mandatory().String(ReferenceDataValidation.MaxCodeLength);
            Property(x => x.Text).Mandatory().String(ReferenceDataValidation.MaxTextLength);
            Property(x => x.Description).String(ReferenceDataValidation.MaxDescriptionLength);
            Property(x => x.EndDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue).CompareProperty(CompareOperator.GreaterThanEqual, x => x.StartDate);
        }

        /// <summary>
        /// Perform more complex mandatory check based on the ReferenceData base ID type.
        /// </summary>
        private Task ValidateIdAsync(PropertyContext<TEntity, object?> context)
        {
            if (context.Value != null)
            {
                if (context.Parent.Value is ReferenceDataBaseInt32 && (int)context.Value != 0)
                    return Task.CompletedTask;

                if (context.Parent.Value is ReferenceDataBaseInt64 && (long)context.Value != 0)
                    return Task.CompletedTask;

                if (context.Parent.Value is ReferenceDataBaseGuid && (Guid)context.Value != Guid.Empty)
                    return Task.CompletedTask;
            }

            context.CreateErrorMessage(ValidatorStrings.MandatoryFormat);
            return Task.CompletedTask;
        }
    }
}