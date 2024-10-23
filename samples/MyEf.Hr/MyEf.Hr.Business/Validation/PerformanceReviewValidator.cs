namespace MyEf.Hr.Business.Validation;

/// <summary>
/// Represents a <see cref="PerformanceReview"/> validator.
/// </summary>
public class PerformanceReviewValidator : Validator<PerformanceReview>
{
    private readonly IPerformanceReviewManager _performanceReviewManager;
    private readonly IEmployeeManager _employeeManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewValidator"/> class.
    /// </summary>
    public PerformanceReviewValidator(IPerformanceReviewManager performanceReviewManager, IEmployeeManager employeeManager)
    {
        _performanceReviewManager = performanceReviewManager ?? throw new ArgumentNullException(nameof(performanceReviewManager));
        _employeeManager = employeeManager ?? throw new ArgumentNullException(nameof(employeeManager));

        Property(x => x.EmployeeId).Mandatory();
        Property(x => x.Date).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => ExecutionContext.Current.Timestamp, _ => "today");
        Property(x => x.Notes).String(4000);
        Property(x => x.Reviewer).Mandatory().String(256);
        Property(x => x.Outcome).Mandatory().IsValid();
    }

    /// <summary>
    /// Add further validation logic.
    /// </summary>
    protected override async Task<Result> OnValidateAsync(ValidationContext<PerformanceReview> context, CancellationToken cancellationToken)
    {
        // Exit where the EmployeeId is not valid.
        if (context.HasError(x => x.EmployeeId))
            return Result.Success;

        // Ensure that the EmployeeId, on Update, has not been changed as it is immutable.
        if (ExecutionContext.OperationType == OperationType.Update)
        {
            var prr = await Result.GoAsync(_performanceReviewManager.GetAsync(context.Value.Id))
                                  .When(v => v is null, _ => Result.NotFoundError()).ConfigureAwait(false);

            if (prr.IsFailure)
                return prr.Bind();

            if (context.Value.EmployeeId != prr.Value.EmployeeId)
                return Result.Done(() => context.AddError(x => x.EmployeeId, ValidatorStrings.ImmutableFormat));
        }

        // Check that the referenced Employee exists, and the review data is within the bounds of their employment.
        return await Result.GoAsync(_employeeManager.GetAsync(context.Value.EmployeeId)).Then(e =>
        {
            if (e == null)
                context.AddError(x => x.EmployeeId, ValidatorStrings.ExistsFormat);
            else
            {
                if (!context.HasError(x => x.Date))
                {
                    if (context.Value.Date < e.StartDate)
                        context.AddError(x => x.Date, "{0} must not be prior to the Employee starting.");
                    else if (e.Termination != null && context.Value.Date > e.Termination.Date)
                        context.AddError(x => x.Date, "{0} must not be after the Employee has terminated.");
                }
            }
        }).AsResult().ConfigureAwait(false);
    }
}