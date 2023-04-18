namespace MyEf.Hr.Security.Subscriptions.Subscribers;

[EventSubscriber("MyEf.Hr.Employee", "Terminated")]
public class EmployeeTerminatedSubcriber : SubscriberBase<Employee>
{
    private static readonly Validator<Employee> _employeeValidator = Validator.Create<Employee>()
        .HasProperty(x => x.Id, p => p.Mandatory())
        .HasProperty(x => x.Email, p => p.Mandatory().Email())
        .HasProperty(x => x.Termination, p => p.Mandatory());

    private readonly OktaHttpClient _okta;

    public EmployeeTerminatedSubcriber(OktaHttpClient okta)
    {
        _okta = okta ?? throw new ArgumentNullException(nameof(okta));
        ValueValidator = _employeeValidator;
    }

    public override ErrorHandling SecurityHandling => ErrorHandling.TransientRetry;

    public override ErrorHandling NotFoundHandling => ErrorHandling.CompleteWithWarning;

    public override async Task ReceiveAsync(EventData<Employee> @event, CancellationToken cancellationToken)
    {
        var identifer = await _okta.GetIdentifier(@event.Value.Email!).ConfigureAwait(false) ?? throw new NotFoundException($"Employee {@event.Value.Id} with email {@event.Value.Email} not found in OKTA.");
        await _okta.DeactivateUser(identifer).ConfigureAwait(false);
    }
}