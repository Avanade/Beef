namespace MyEf.Hr.Security.Test;

[TestFixture]
public class SubscribingFunctionTest
{
    [Test]
    public void InvalidMessage_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage<string>(null!);

        test.ServiceBusTrigger<SubscribingFunction>()
            .ExpectLogContains("[EventDataDeserialization]")
            .Run(f => f.RunAsync(message, actionsMock.Object))
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

        test.ServiceBusTrigger<SubscribingFunction>()
            .Run(f => f.RunAsync(message, actionsMock.Object))
            .AssertSuccess();

        actionsMock.Verify(m => m.CompleteMessageAsync(message, default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
    }
}