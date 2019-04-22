// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

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
        private static readonly TValidator _default = new TValidator();

        /// <summary>
        /// Gets the current instance of the validator.
        /// </summary>
        public static TValidator Default
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
            Property(x => x.Id).Custom(ValidateId);
            Property(x => x.Code).Mandatory().String(ReferenceDataValidation.MaxCodeLength);
            Property(x => x.Text).Mandatory().String(ReferenceDataValidation.MaxTextLength);
            Property(x => x.Description).String(ReferenceDataValidation.MaxDescriptionLength);
            Property(x => x.EndDate).When(x => x.StartDate.HasValue && x.EndDate.HasValue).CompareProperty(CompareOperator.GreaterThanEqual, x => x.StartDate);
        }

        /// <summary>
        /// Perform more complex mandatory check based on the ReferenceData base ID type.
        /// </summary>
        private void ValidateId(PropertyContext<TEntity, object> context)
        {
            if (context.Value != null)
            {
                if (context.Parent.Value is ReferenceDataBaseInt && (int)context.Value != 0)
                    return;

                if (context.Parent.Value is ReferenceDataBaseGuid && (Guid)context.Value != Guid.Empty)
                    return;
            }

            context.CreateErrorMessage(ValidatorStrings.MandatoryFormat);
        }
    }
}
