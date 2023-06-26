namespace MyEf.Hr.Security.Subscriptions;

public class SecuritySubscriberFunction
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber;

    public SecuritySubscriberFunction(ServiceBusOrchestratedSubscriber subscriber) => _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

    [Singleton(Mode = SingletonMode.Function)]
    [FunctionName(nameof(SecuritySubscriberFunction))]
    public Task RunAsync([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
        => _subscriber.ReceiveAsync(message, messageActions, null, cancellationToken);
}