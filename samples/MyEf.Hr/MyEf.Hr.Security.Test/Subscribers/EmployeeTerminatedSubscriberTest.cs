namespace MyEf.Hr.Security.Test.Subscribers;

[TestFixture]
public class EmployeeTerminatedSubscriberTest
{
    [Test]
    public void ValueIsRequired_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = null! });

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Value is required")
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertSuccess();

        actionsMock.Verify(m => m.DeadLetterMessageAsync(message, It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ValidationError_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee() });

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("A data validation error occurred")
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertSuccess();

        actionsMock.Verify(m => m.DeadLetterMessageAsync(message, It.IsAny<string>(), It.IsAny<string>(), default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
    }

    [Test]
    public void EmailNotFound_Complete()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithJson("[]", HttpStatusCode.OK);

        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("email bob@email.com not found", "[Source: Subscriber, Handling: CompleteWithWarning]")
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertSuccess();

        actionsMock.Verify(m => m.CompleteMessageAsync(message, default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
        mcf.VerifyAll();
    }

    [Test]
    public void OktaForbidden_Retry()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.With(HttpStatusCode.Forbidden);

        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Retry - Service Bus message", "[AuthenticationError]")
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertException<EventSubscriberException>();

        actionsMock.VerifyNoOtherCalls();
        mcf.VerifyAll();
    }

    [Test]
    public void OktaServiceUnavailable_Retry()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithSequence(x =>
        {
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
        });

        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Retry - Service Bus message", "[TransientError]")
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertException<EventSubscriberException>();

        actionsMock.VerifyNoOtherCalls();
        mcf.VerifyAll();
    }

    [Test]
    public void Success_Complete()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithJsonResource("EmployeeTerminatedSubscriberTest_Success.json", HttpStatusCode.OK);
        mc.Request(HttpMethod.Post, "/api/v1/users/00ub0oNGTSWTBKOLGLNR/lifecycle/deactivate?sendEmail=true").Respond.With(HttpStatusCode.OK);

        using var test = FunctionTester.Create<Startup>();
        var actionsMock = new Mock<ServiceBusMessageActions>();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .Run(f => f.RunAsync(message, actionsMock.Object, default))
            .AssertSuccess();

        actionsMock.Verify(m => m.CompleteMessageAsync(message, default), Times.Once);
        actionsMock.VerifyNoOtherCalls();
        mcf.VerifyAll();
    }
}