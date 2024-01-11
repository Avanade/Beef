namespace MyEf.Hr.Security.Test;

[TestFixture]
public class SecuritySubscriberFunctionTest
{
    [Test]
    public void InvalidMessage_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWebJobsServiceBusMessageActions();
        var message = test.CreateServiceBusMessageFromValue<string>(null!);

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertDeadLetter("EventDataDeserialization", "Cannot decode JSON element of kind 'Null' as CloudEvent");
    }

    [Test]
    public void NotSubscribed_CompleteSilent()
    {
        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWebJobsServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(new EventData { Subject = "myef.hr.employee", Action = "updated", Source = new Uri("test", UriKind.Relative) });

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
    }
}