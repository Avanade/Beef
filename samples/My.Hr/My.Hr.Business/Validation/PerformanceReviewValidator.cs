using Beef;
using Beef.Entities;
using Beef.Validation;
using My.Hr.Common.Entities;
using System;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="PerformanceReview"/> validator.
    /// </summary>
    public class PerformanceReviewValidator : Validator<PerformanceReview, PerformanceReviewValidator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReviewValidator"/> class.
        /// </summary>
        public PerformanceReviewValidator()
        {
            Property(x => x.EmployeeId).Mandatory();
            Property(x => x.Date).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => Cleaner.Clean(DateTime.Now), _ => "today");
            Property(x => x.Notes).String(4000);
            Property(x => x.Reviewer).Mandatory().String(256);
            Property(x => x.Outcome).Mandatory().IsValid();
        }

        /// <summary>
        /// Add further validation logic.
        /// </summary>
        protected override void OnValidate(ValidationContext<PerformanceReview> context)
        {
            if (!context.HasError(x => x.EmployeeId))
            {
                // Ensure that the EmployeeId has not be changed (change back) as it is immutable.
                if (ExecutionContext.Current.OperationType == OperationType.Update)
                {
                    var prm = (IPerformanceReviewManager)context.ServiceProvider.GetService(typeof(IPerformanceReviewManager));
                    var prv = prm.GetAsync(context.Value.Id).GetAwaiter().GetResult();
                    if (prv == null)
                        throw new NotFoundException();

                    if (context.Value.EmployeeId != prv.EmployeeId)
                    {
                        context.AddError(x => x.EmployeeId, ValidatorStrings.ImmutableFormat);
                        return;
                    }
                }

                // Check that the referenced Employee exists, and the review data is within the bounds of their employment.
                var em = (IEmployeeManager)context.ServiceProvider.GetService(typeof(IEmployeeManager));
                var ev = em.GetAsync(context.Value.EmployeeId).GetAwaiter().GetResult();
                if (ev == null)
                    context.AddError(x => x.EmployeeId, ValidatorStrings.ExistsFormat);
                else
                {
                    if (!context.HasError(x => x.Date))
                    {
                        if (context.Value.Date < ev.StartDate)
                            context.AddError(x => x.Date, "{0} must not be prior to the Employee starting.");
                        else if (ev.Termination != null && context.Value.Date > ev.Termination.Date)
                            context.AddError(x => x.Date, "{0} must not be after the Employee has terminated.");
                    }
                }
            }
        }
    }
}