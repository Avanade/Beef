namespace MyEf.Hr.Security.Subscriptions;

public class SecuritySubscriberFunction(ServiceBusOrchestratedSubscriber subscriber)
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber = subscriber.ThrowIfNull();

    [Function(nameof(SecuritySubscriberFunction))]
    public Task RunAsync([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
        => _subscriber.ReceiveAsync(message, messageActions, null, cancellationToken);
}