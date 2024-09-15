# Step 8 - Transactional Outbox

To support the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) there is the need to have a backing event queue within the database where the event data is persisted transactionally. This will ensure that the events and corresponding data being manipulated are persisted as a single unit of work; before an attempt is made to publish (send) the events to an underlying messaging subsystem. This will guarantee that there will be _zero_ event loss, and that the events will be stored in the sequence in which they are enqueued (honoring order).

<br/>

## Event interoperability

To provide generic [eventing/messaging](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Events) interoperability the _CoreEx_ [`EventData`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventData.cs) (inherits from [`EventDataBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventDataBase.cs)) provides a messaging subsystem agnostic means to describe the generic characteristics of an event/message.

The pluggable nature of an [`IEventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventPublisher.cs) enables the publishing via the `Publish` method to internally queue the messages, and when ready perform a `SendAsync` to send the one or more published events in an atomic-style operation. The `IEventPublisher` is responsible for orchestrating the [`EventDataFormatter`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventDataFormatter.cs), [`IEventSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSerializer.cs) and [`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs) to enable the publish and send. The [`EventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventPublisher.cs) provides the default implementation.

<br/>

## Previously generated capabilities

Within [Step 2](./2-Employee-DB.md) the transactional outbox capabilities, both database and .NET, were generated and included into the solution when performing the [Event outbox](./Employee-DB.md#Event%20outbox).

There were two tables added to the database [`Outbox.EventOutbox`](../MyEf.Hr.Database/Migrations/20221207-004320-02-create-outbox-eventoutbox-table.sql) and [`Outbox.EventOutboxData`](../MyEf.Hr.Database/Migrations/20221207-004320-03-create-outbox-eventoutboxdata-table.sql) via the corresponding generated migration scripts; these tables provide the underlying transactional persistence.

<br/>

### Enqueue

The following are the key generated Outbox enqueue artefacts; performing the transactional persistence.

Type | Name | Description
-|-|-
Stored procedure | [spEventOutboxEnqueue](../MyEf.Hr.Database/Schema/Outbox/Stored%20Procedures/Generated/spEventOutboxEnqueue.sql) | The stored procedure used to _enqueue_ zero or more events into the database.
Used-defined table type | [udtEventOutboxList](../MyEf.Hr.Database/Schema/Outbox/Types/User-Defined%20Table%20Types/Generated/udtEventOutboxList.sql) | The type used during _enqueue_ as the events collection being passed. By design this is the database representation (column from/to property) of the _CoreEx_ .NET [EventData](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventData.cs) class.
Class | [EventOutboxEnqueue](../MyEf.Hr.Business/Data/Generated/EventOutboxEnqueue.cs) | Provides the [`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs) implementation (inheriting from [EventOutboxEnqueueBase](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxEnqueueBase.cs)) to perform the _enqueue_ using the `spEventOutboxEnqueue` stored procedure.

<br/>

### Dequeue

The following are the key generated Outbox dequeue artefacts.

Type | Name | Description
-|-|-
Stored procedure | [spEventOutboxDequeue](../MyEf.Hr.Database/Schema/Outbox/Stored%20Procedures/Generated/spEventOutboxDequeue.sql) | The stored procedure used to _dequeue_ zero or more events from the database.
Class | [EventOutboxDequeue](../MyEf.Hr.Business/Data/Generated/EventOutboxDequeue.cs) | Provides the _dequeue_ implementation (inheriting from [EventOutboxDequeueBase](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxDequeueBase.cs)) using the `spEventOutboxDequeue` stored procedure. This class is then also responsible for sending the dequeued events (via an [`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs)) to the final messaging subsystem. On successful send, the dequeued events will be committed (within Outbox) as sent, guaranteeing as-least once messaging semantics.

<br/>

## Previously generated publishing

The generated .NET [DataSvc-layer](../../../docs/Layer-DataSvc.md) contains the logic to publish and send the corresponding event(s) managing the event enqueue and underlying business data operations within a database transaction.

The following is a code snippet from the [EmployeeDataSvc](../MyEf.Hr.Business/DataSvc/Generated/EmployeeDataSvc.cs) to demonstrate.
- The `_events.PublishValueEvent` publishes the event to an internal queue ready for send; this allows multiple events/messages to be sent transactionally where required.
- The [`DataSvcInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/DataSvcInvoker.cs) is ultimately responsible for orchestrating (see [InvokerBase](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/InvokerBase.cs)) the database transaction and corresponding send/enqueue given the [`InvokerArgs`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/InvokerArgs.cs) configuration. This is controlled by the `{ IncludeTransactionScope = true, EventPublisher = _events })` properties.

``` csharp
public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id) => DataSvcInvoker.Current.InvokeAsync(this, _ =>
{
    return Result.GoAsync(_data.TerminateAsync(value, id))
                 .Then(r => _events.PublishValueEvent(r, new Uri($"myef/hr/employee/{r.Id}", riKind.Relative), $"MyEf.Hr.Employee", "Terminated"))
                 .Then(r => _cache.SetValue(r));
}, new InvokerArgs { IncludeTransactionScope = true, EventPublisher = _events });
```

<br/>

## Previous Dependency Injection (DI) configuration

The set up of the event publishing is managed using Dependency Injection (DI) configuration within the API [`Startup`](../MyEf.Hr.Api/Startup.cs) class.

When the overall solution was created using the _Beef_ template, the eventing-based DI configuration placeholder code would have been similar to the following. The use of the `AddNullEventPublisher()` will register the [`NullEventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/NullEventPublisher.cs) which simply swallows/discards on send; i.e. does nothing.

``` csharp
// Add event publishing services.
services.AddNullEventPublisher();

// Add transactional event outbox services.
services.AddScoped<IEventSender>(sp =>
{
    var eoe = new EventOutboxEnqueue(sp.GetRequiredService<IDatabase>(), p.GetRequiredService<ILogger<EventOutboxEnqueue>>());
    //eoe.SetPrimaryEventSender(/* the primary sender instance; i.e. service bus */); // This is optional.
    return eoe;
});
```

<br/>

## Event Dependency Injection (DI) configuration

The [event interoperability](#Event-interoperability) requires the following services registered.

Service | Description
-|-
[`EventDataFormatter`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventDataFormatter.cs) | The `AddEventDataFormatter()` registers the `EventDataFormatter` which is responsible for the _formatting_ of the `EventData`; i.e. defaulting and updating it to a consistent state ready for serialization and sending.
[`IEventSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSerializer.cs) | The `AddCloudEventSerializer()` registers the [`CloudEventSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Text/Json/CloudEventSerializer.cs) (see [Cloud Events](https://cloudevents.io/)) that is responsible for the serialization of the formatted `EventData` into the resulting [`EventSendData.Data`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventSendData.cs) required by the configured `IEventSender`. Additionally, the [`EventDataSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Text/Json/EventDataSerializer.cs) enables a basic `EventData.Value` JSON serialization via the `AddEventDataSerializer` registration as an alternate option.
[`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs) | The `AddScoped<IEventSender, EventOutboxEnqueue>()` registers the generated `EventOutboxEnqueue` as the `IEventSender`.
[`IEventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventPublisher.cs) | The `AddEventPublisher` registers the [`EventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventPublisher.cs) responsible for the event publish and send orchestration.

To introduce, replace the existing placeholder code with the following within the API [`Startup`](../MyEf.Hr.Api/Startup.cs) class.

``` csharp
// Add event publishing services.
services.AddEventDataFormatter();
services.AddCloudEventSerializer();
services.AddEventPublisher();

// Add transactional event outbox enqueue services.
services.AddScoped<IEventSender, EventOutboxEnqueue>();
```

<br/>

## Unit testing

Back in [Step 4](./4-Employee-Test.md) unit testing of the API surface was introduced. Within these tests there was an `ExpectEvent` and `ExpectEventValue` that verified that a corresponding event was being published and sent; even though we had configured the API with a `NullEventPublisher`.

Hang on! How was an event verified where configured to discard?

The [`FixtureSetup`](../MyEf.Hr.Test/Apis/FixtureSetup.cs) leveraged a _UnitTestEx_ [TestSetUp](https://github.com/Avanade/UnitTestEx/blob/main/src/UnitTestEx/TestSetUp.cs) capability, being the `ExpectedEventsEnabled` property. Where enabled the `IEventPublisher` will be automatically replaced at runtime with the [`ExpectedEventPublisher`](https://github.com/Avanade/UnitTestEx/blob/main/src/UnitTestEx/Expectations/ExpectedEventPublisher.cs) that is used by the `ExpectEvent` to verify the expected events were sent.

Therefore, _no_ events will be sent to any external eventing/messaging system during unit testing. This has the advantage of decoupling the test execution from the dependent messaging subsystem, minimizing the need for any additional infrastructure to enable the unit tests.

<br/>

## Localized testing

To achieve a basic test within a developers machine then the API should be executed directly. By leveraging the [Swagger](https://swagger.io/) endpoint, or using a tool such as [Postman](https://www.postman.com/), an applicable POST/PUT/DELETE operation should be invoked which will result in the selected data update and corresponding event persisted to the database.

To verify, use a database tool, such as [Azure Data Studio](https://learn.microsoft.com/en-us/sql/azure-data-studio/what-is-azure-data-studio) to query the `Outbox.EventOutbox` and `Outbox.EventOutboxData` tables. The `Outbox.EventOutbox` manages the enqueue and dequeue state via the `EnqueuedDate` and `DequeueDate` columns. Where the `DequeueDate` column is `null` then the event is considered queued and ready for dequeue.

To perform a localized test perform a POST to the `/employees/00000001-0000-0000-0000-000000000000/terminate` endpoint; this should be passed the following JSON body.

``` json
{
  "date": "2023-04-20T17:47:31.898Z",
  "reason": "RE"
}
```


<br/>

## Verify

At this stage we now have our events being persisted to the Transactional Outbox ready for sending to the final messaging subsystem.

<br/>

## Next Step

Next we need to perform the dequeue and [Service Bus Publish](./9-Service-Bus-Publish.md).