namespace MyEf.Hr.Security.Subscriptions.Subscribers;

[EventSubscriber("MyEf.Hr.Employee", "Terminated")]
public class EmployeeTerminatedSubcriber : SubscriberBase<Employee>
{
    private static readonly Validator<Employee> _employeeValidator = Validator.Create<Employee>()
        .HasProperty(x => x.Id, p => p.Mandatory())
        .HasProperty(x => x.Email, p => p.Mandatory().Email())
        .HasProperty(x => x.Termination, p => p.Mandatory());

    private readonly OktaHttpClient _okta;
    private readonly ILogger _logger;

    public EmployeeTerminatedSubcriber(OktaHttpClient okta, ILogger<EmployeeTerminatedSubcriber> logger)
    {
        _okta = okta ?? throw new ArgumentNullException(nameof(okta));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ValueValidator = _employeeValidator;
    }

    public override ErrorHandling SecurityHandling => ErrorHandling.Retry;

    public override ErrorHandling NotFoundHandling => ErrorHandling.CompleteWithWarning;

    public override Task<Result> ReceiveAsync(EventData<Employee> @event, EventSubscriberArgs args, CancellationToken cancellationToken) 
        => Result.GoAsync(_okta.GetUserAsync(@event.Value.Id, @event.Value.Email!))
            .When(user => !user.IsDeactivatable, user => _logger.LogWarning("Employee {EmployeeId} with email {Email} has User status of {UserStatus} and is therefore unable to be deactivated.", @event.Value.Id, @event.Value.Email, user.Status))
            .WhenAsAsync(user => user.IsDeactivatable, user => _okta.DeactivateUserAsync(user.Id!));
}