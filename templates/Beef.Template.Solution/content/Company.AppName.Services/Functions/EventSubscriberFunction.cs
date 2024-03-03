namespace Company.AppName.Services.Functions;

/// <summary>
/// Provides the event-subscription Azure Function.
/// </summary>
/// <param name="subscriber"></param>
public class EventSubscriberFunction(ServiceBusOrchestratedSubscriber subscriber)
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber = subscriber.ThrowIfNull();

    /// <summary>
    /// Receive and process the event <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The <see cref="ServiceBusReceivedMessage"/>.</param>
    /// <param name="messageActions">The <see cref="ServiceBusMessageActions"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    [Function(nameof(EventSubscriberFunction))]
    public Task RunAsync([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
        => _subscriber.ReceiveAsync(message, messageActions, null, cancellationToken);
}