using Beef.Validation;
using Cdr.Banking.Common.Entities;
using System;

namespace Cdr.Banking.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="TransactionArgs"/> validator.
    /// </summary>
    public class TransactionArgsValidator : Validator<TransactionArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionArgsValidator"/>.
        /// </summary>
        public TransactionArgsValidator()
        {
            // Default FromDate where not provided, as 90 days less than ToDate; where no ToDate then assume today (now). Make sure FromDate is not greater than ToDate.
            Property(x => x.FromDate)
                .Default(a => (a.ToDate!.HasValue ? a.ToDate.Value : DateTime.Now).AddDays(-90))
                .CompareProperty(CompareOperator.LessThanEqual, y => y.ToDate).DependsOn(y => y.ToDate);

            // Make sure MinAmount is not greater than MaxAmount.
            Property(x => x.MinAmount).CompareProperty(CompareOperator.LessThanEqual, y => y.MaxAmount).DependsOn(y => y.MaxAmount);

            // Make sure the Text does not include the '*' wildcard character.
            Property(x => x.Text).Wildcard(Beef.Wildcard.None);
        }
    }
}