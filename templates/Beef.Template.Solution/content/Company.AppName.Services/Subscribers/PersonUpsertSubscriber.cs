namespace Company.AppName.Services.Subscribers;

/// <summary>
/// Subscribe to the <see cref="Person"/> <b>created</b> and <b>updated</b> events.
/// </summary>
/// <param name="manager">The <see cref="IPersonManager"/>.</param>
/// <param name="logger">The <see cref="ILogger"/>.</param>
[EventSubscriber("lowercom.lowerapp.person", "created", "updated")]
public class PersonUpsertSubscriber(IPersonManager manager, ILogger<PersonUpsertSubscriber> logger) : SubscriberBase<Person>(_validator)
{
    // Instantiate a basic validator to ensure the Person has the required properties to proceed.
    private static readonly Validator<Person> _validator = Validator.Create<Person>()
        .HasProperty(x => x.Id, p => p.Mandatory())
        .HasProperty(x => x.FirstName, p => p.Mandatory())
        .HasProperty(x => x.LastName, p => p.Mandatory());

    private readonly IPersonManager _manager = manager.ThrowIfNull();
    private readonly ILogger _logger = logger.ThrowIfNull();

    /// <inheritdoc/>
    /// <remarks>Log an error and complete (i.e. do not dead-letter).</remarks>
    public override ErrorHandling InvalidDataHandling => ErrorHandling.CompleteWithError;

    /// <inheritdoc/>
    /// <remarks>Log a warning where not found and complete (i.e. do not dead-letter).</remarks>
    public override ErrorHandling NotFoundHandling => ErrorHandling.CompleteWithWarning;

    /// <inheritdoc/>
    /// <remarks>For the purposes of the template checks that the person still exists (reuses manager logic) and logs a message; otherwise, results in not found error logged as a warning.</remarks>
    public override Task<Result> ReceiveAsync(EventData<Person> @event, EventSubscriberArgs args, CancellationToken cancellationToken)
        => Result.GoAsync(_manager.GetAsync(@event.Value.Id))
            .When(p => p is null, _ => Result.NotFoundError($"Person does not exist (Id: {@event.Value.Id})."))
            .ThenAs(_ => _logger.LogInformation($"Person {@event.Action} event received for '{@event.Value.LastName}, {@event.Value.FirstName}' (Id: {@event.Value.Id})."));
}