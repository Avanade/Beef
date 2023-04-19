namespace MyEf.Hr.Security.Subscriptions;

public class SecuritySubscriberFunction
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber;

    public SecuritySubscriberFunction(ServiceBusOrchestratedSubscriber subscriber) => _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

    [Singleton(Mode = SingletonMode.Function)]
    [FunctionName(nameof(SecuritySubscriberFunction))]
    [ExponentialBackoffRetry(3, "00:02:00", "00:30:00")]
    public Task RunAsync([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
        => _subscriber.ReceiveAsync(message, messageActions);
}