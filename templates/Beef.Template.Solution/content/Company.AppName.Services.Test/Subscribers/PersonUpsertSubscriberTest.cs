using Company.AppName.Business.Entities;

namespace Company.AppName.Services.Test.Subscribers;

[TestFixture, NonParallelizable]
public class PersonUpsertSubscriberTest
{
#if (!implement_httpagent)
    [OneTimeSetUp]
    public void OneTimeSetUp() => Assert.That(TestSetUp.Default.SetUp(), Is.True);
#endif

    [Test]
    public void A110_ValueIsRequired_LogError()
    {
        using var test = FunctionTester.Create<Startup>();
        var message = test.CreateServiceBusMessage(new EventData<Person> { Subject = "lowercom.lowerapp.person", Action = "created", Source = new Uri("test", UriKind.Relative) });
        var actions = test.CreateWorkerServiceBusMessageActions();

        test.ServiceBusTrigger<EventSubscriberFunction>()
            .ExpectLogContains("fail: Invalid message; body was not provided, contained invalid JSON, or was incorrectly formatted: Value is required.")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
    }

    [Test]
    public void A120_ValidationError_LogError()
    {
        using var test = FunctionTester.Create<Startup>();
        var message = test.CreateServiceBusMessage(new EventData<Person> { Subject = "lowercom.lowerapp.person", Action = "created", Source = new Uri("test", UriKind.Relative), Value = new Person() });
        var actions = test.CreateWorkerServiceBusMessageActions();

        test.ServiceBusTrigger<EventSubscriberFunction>()
            .ExpectLogContains("fail: A data validation error occurred. [id: Identifier is required.] [firstName: First Name is required.] [lastName: Last Name is required.]")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
    }

    [Test]
    public void A130_NotFound_LogWarning()
    {
#if (!implement_mysql && !implement_postgres)
        var p = new Person { Id = 404.ToGuid(), FirstName = "Rachael", LastName = "Browne" };
#endif
#if (implement_mysql || implement_postgres)
        var p = new Person { Id = 404, FirstName = "Rachael", LastName = "Browne" };
#endif

        using var test = FunctionTester.Create<Startup>();
        var message = test.CreateServiceBusMessage(new EventData<Person> { Subject = "lowercom.lowerapp.person", Action = "created", Source = new Uri("test", UriKind.Relative), Value = p });
        var actions = test.CreateWorkerServiceBusMessageActions();

        test.ServiceBusTrigger<EventSubscriberFunction>()
            .ExpectLogContains($"warn: Person does not exist (Id: {p.Id}).")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
    }

    [Test]
    public void A140_Found_Success()
    {
#if (!implement_mysql && !implement_postgres)
        var p = new Person { Id = 3.ToGuid(), FirstName = "Rachael", LastName = "Browne" };
#endif
#if (implement_mysql || implement_postgres)
        var p = new Person { Id = 3, FirstName = "Rachael", LastName = "Browne" };
#endif

        using var test = FunctionTester.Create<Startup>();
        var message = test.CreateServiceBusMessage(new EventData<Person> { Subject = "lowercom.lowerapp.person", Action = "created", Source = new Uri("test", UriKind.Relative), Value = p });
        var actions = test.CreateWorkerServiceBusMessageActions();

        test.ServiceBusTrigger<EventSubscriberFunction>()
            .ExpectLogContains($"info: Person created event received for 'Browne, Rachael' (Id: {p.Id}).")
            .Run(f => f.RunAsync(message, actions, default))
            .AssertSuccess();

        actions.AssertComplete();
    }
}