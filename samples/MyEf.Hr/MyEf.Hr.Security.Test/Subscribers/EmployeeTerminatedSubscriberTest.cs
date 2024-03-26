namespace MyEf.Hr.Security.Test.Subscribers;

[TestFixture]
public class EmployeeTerminatedSubscriberTest
{
    [Test]
    public void ValueIsRequired_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var message = test.CreateServiceBusMessage(new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = null! });
        var actions = test.CreateWorkerServiceBusMessageActions();

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertDeadLetter("ValidationError", "Value is required");
    }

    [Test]
    public void ValidationError_DeadLetter()
    {
        using var test = FunctionTester.Create<Startup>();
        var message = test.CreateServiceBusMessage(new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee() });
        var actions = test.CreateWorkerServiceBusMessageActions();

        test.ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("A data validation error occurred")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertDeadLetter("ValidationError", "Email is required");
    }

    [Test]
    public void EmailNotFound_None_Complete()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithJson("[]", HttpStatusCode.OK);

        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Employee 00000001-0000-0000-0000-000000000000 with email bob@email.com not found within OKTA.", "[Source: Subscriber, Handling: CompleteWithWarning]")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
        mcf.VerifyAll();
    }

    [Test]
    public void EmailNotFound_Multi_Complete()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithJson("[{},{}]", HttpStatusCode.OK);

        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Employee 00000001-0000-0000-0000-000000000000 with email bob@email.com has multiple entries within OKTA.", "[Source: Subscriber, Handling: CompleteWithWarning]")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
        mcf.VerifyAll();
    }

    [Test]
    public void OktaForbidden_Retry()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.With(HttpStatusCode.Forbidden);

        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Retry - Service Bus message", "[AuthorizationError]")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertAbandon();
        mcf.VerifyAll();
    }

    [Test]
    public void OktaServiceUnavailable_Retry()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/").WithConfigurations();
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithSequence(x =>
        {
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
            x.Respond().With(HttpStatusCode.ServiceUnavailable);
        });

        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("Retry - Service Bus message", "[TransientError]")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertAbandon();
        mcf.VerifyAll();
    }

    [Test]
    public void SuccessDeactivated_Complete()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithJsonResource("EmployeeTerminatedSubscriberTest_Success.json", HttpStatusCode.OK);
        mc.Request(HttpMethod.Post, "/api/v1/users/00ub0oNGTSWTBKOLGLNR/lifecycle/deactivate?sendEmail=true").Respond.With(HttpStatusCode.OK);

        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
        mcf.VerifyAll();
    }

    [Test]
    public void SuccessAlreadyDeactivated_Complete()
    {
        var mcf = MockHttpClientFactory.Create();
        var mc = mcf.CreateClient("OktaApi", "https://test-okta/");
        mc.Request(HttpMethod.Get, "/api/v1/users?search=profile.email eq \"bob@email.com\"").Respond.WithJson(new [] { new { id = "00ub0oNGTSWTBKOLGLNR", status = "DEACTIVATED" } });

        using var test = FunctionTester.Create<Startup>();
        var actions = test.CreateWorkerServiceBusMessageActions();
        var message = test.CreateServiceBusMessage(
            new EventData<Employee> { Subject = "myef.hr.employee", Action = "terminated", Source = new Uri("test", UriKind.Relative), Value = new Employee { Id = 1.ToGuid(), Email = "bob@email.com", Termination = new() } });

        test.ReplaceHttpClientFactory(mcf)
            .ServiceBusTrigger<SecuritySubscriberFunction>()
            .ExpectLogContains("warn: Employee 00000001-0000-0000-0000-000000000000 with email bob@email.com has User status of DEACTIVATED and is therefore unable to be deactivated.")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
        mcf.VerifyAll();
    }
}