namespace MyEf.Hr.Security.Test;

[TestFixture]
public class SecuritySubscriberFunctionTest
{
    [Test]
    public void InvalidMessage_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage<string>(null!);

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("[EventDataDeserialization]")
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertSuccess();

        actionsMock.Verify(m => m.DeadLetterMessageAsync(message, It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
    }

    [Test]
    public void NotSubscribed_CompleteSilent()
    {
        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(new EventData { Subject = "myef.hr.employee", Action = "updated", Source = new Uri("test", UriKind.Relative) });

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertSuccess();

        actionsMock.Verify(m => m.CompleteMessageAsync(message, default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
    }
}