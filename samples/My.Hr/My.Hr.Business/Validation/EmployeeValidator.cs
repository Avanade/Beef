namespace My.Hr.Business.Validation;

/// <summary>
/// Represents a <see cref="Employee"/> validator.
/// </summary>
public class EmployeeValidator : Validator<Employee>
{
    private readonly IEmployeeDataSvc _employeeDataSvc;

    // Address validator implemented using fluent-style method-chaining.
    private static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
        .HasProperty(x => x.Street1, p => p.Mandatory().Common(CommonValidators.Street))
        .HasProperty(x => x.Street2, p => p.Common(CommonValidators.Street))
        .HasProperty(x => x.City, p => p.Mandatory().String(50))
        .HasProperty(x => x.State, p => p.Mandatory().IsValid())
        .HasProperty(x => x.PostCode, p => p.Mandatory().String(new Regex(@"^\d{5}(?:[-\s]\d{4})?$")));

    // Emergency Contact validator implemented using fluent-style method-chaining.
    public static readonly Validator<EmergencyContact> _emergencyContactValidator = Validator.Create<EmergencyContact>()
        .HasProperty(x => x.FirstName, p => p.Mandatory().Common(CommonValidators.PersonName))
        .HasProperty(x => x.LastName, p => p.Mandatory().Common(CommonValidators.PersonName))
        .HasProperty(x => x.PhoneNo, p => p.Mandatory().Common(CommonValidators.PhoneNo))
        .HasProperty(x => x.Relationship, p => p.Mandatory().IsValid());

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeValidator"/> class.
    /// </summary>
    /// <param name="employeeDataSvc">The <see cref="IEmployeeDataSvc"/>.</param>
    public EmployeeValidator(IEmployeeDataSvc employeeDataSvc)
    {
        _employeeDataSvc = employeeDataSvc ?? throw new ArgumentNullException(nameof(employeeDataSvc));

        Property(x => x.Email).Mandatory().Email();
        Property(x => x.FirstName).Mandatory().Common(CommonValidators.PersonName);
        Property(x => x.LastName).Mandatory().Common(CommonValidators.PersonName);
        Property(x => x.Gender).Mandatory().IsValid();
        Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => CoreEx.ExecutionContext.Current.Timestamp.AddYears(-18), errorText: "Birthday is invalid as the Employee must be at least 18 years of age.");
        Property(x => x.StartDate).Mandatory().CompareValue(CompareOperator.GreaterThanEqual, new DateTime(1999, 01, 01, 0, 0, 0, DateTimeKind.Utc), "January 1, 1999");
        Property(x => x.PhoneNo).Mandatory().Common(CommonValidators.PhoneNo);
        Property(x => x.Address).Entity(_addressValidator);
        Property(x => x.EmergencyContacts).Collection(maxCount: 5, item: CollectionRuleItem.Create(_emergencyContactValidator).DuplicateCheck(ignoreWhereKeyIsInitial: true));
    }

    /// <summary>
    /// Add further validation logic non-property bound.
    /// </summary>
    protected override async Task OnValidateAsync(ValidationContext<Employee> context, CancellationToken cancellationToken)
    {
        // Ensure that the termination data is always null on an update; unless already terminated then it can no longer be updated.
        switch (ExecutionContext.OperationType)
        {
            case OperationType.Create:
                context.Value.Termination = null;
                break;

            case OperationType.Update:
                var existing = await _employeeDataSvc.GetAsync(context.Value.Id).ConfigureAwait(false);
                if (existing == null)
                    throw new NotFoundException();

                if (existing.Termination != null)
                    throw new ValidationException("Once an Employee has been Terminated the data can no longer be updated.");

                context.Value.Termination = null;
                break;
        }
    }

    /// <summary>
    /// Validator that will be referenced by the Delete operation to ensure that the employee can indeed be deleted.
    /// </summary>
    public static CommonValidator<Guid> CanDelete { get; } = CommonValidator.Create<Guid>(cv => cv.CustomAsync(async (context, _) =>
    {
        // Unable to use inheritance DI for a Common Validator so the ExecutionContext.GetService will get/create the instance in the same manner.
        var existing = await CoreEx.ExecutionContext.GetRequiredService<IEmployeeDataSvc>().GetAsync(context.Value).ConfigureAwait(false);
        if (existing == null)
            throw new NotFoundException();

        if (existing.StartDate <= CoreEx.ExecutionContext.Current.Timestamp)
            throw new ValidationException("An employee cannot be deleted after they have started their employment.");
    }));
}