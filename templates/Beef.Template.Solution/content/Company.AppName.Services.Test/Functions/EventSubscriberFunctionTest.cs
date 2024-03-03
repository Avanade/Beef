namespace Company.AppName.Services.Test.Functions;

[TestFixture, NonParallelizable]
public class EventSubscriberFunctionTest
{
    [Test]
    public void A110_InvalidMessage_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessageFromValue<string>(null!);

        test.ServiceBusTrigger<EventSubscriberFunction>()
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertDeadLetter("EventDataDeserialization", "Cannot decode JSON element of kind 'Null' as CloudEvent");
    }

    [Test]
    public void A120_NotSubscribed_CompleteSilent()
    {
        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(new EventData { Subject = "lowercom.lowerapp.person", Action = "deleted", Source = new Uri("test", UriKind.Relative) });

        test.ServiceBusTrigger<EventSubscriberFunction>()
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
    }
}