# Transactional Outbox Pattern

This article will describe the process to implement the _Transactional Outbox Pattern_ leveraging _Beef_. 

See the following articles that describe the challenge and the architectural solution:
- https://microservices.io/patterns/data/transactional-outbox.html
- https://www.kamilgrzybek.com/design/the-outbox-pattern/

As _Beef_ enables the creation of _microservices_ and the ability for these to publish _events_, there may be a requirement for these to be sent _reliably_ (with no message loss). Therefore, _Beef_ provides a means using provided capabilities to enable.

<br/>

## Dependency

It is assumed for this article that the Outbox Pattern is being introduced to an existing _Beef_ solution; therefore the solution structure and code-generation set up is in place. 

<br/>

## Code generation

To enable the transactional nature of the _outbox_ there is the requirement to enqueue and dequeue the events within the same database as the functional/business data that is being updated. The required artefacts can be code generated to achieve the required functionality. 

<br/>

### Database tables

The first requirement is to persist/store the event data within the database using tables:
- `EventOutbox` - primary table that provides the basic data to enable enqueueing and dequeueing. 
- `EventOutboxData` - secondary table that stores the event metadata and data.

These two tables are created using your database code generation tooling project. This will be named similar to `Company.AppName.Database`. From the command line execute the following.

```
dotnet run codegen --script DatabaseEventOutbox.xml
```

This should create two migrations script files with names similar as follows (where `xxx` is the `AppName` specified within the code generation tooling).

```
└── Migrations
  └── 20210430-170605-create-xxx-eventoutbox.sql
  └── 20210430-170605-create-xxx-eventoutboxdata.sql
```

<br/>

### Stored procedures and .NET

Once the tables have been created the enqueue and dequeue stored procedures are required. Generally speaking, and in the case of this implementation, the events must follow strict FIFO (first-in, first-out) rules. To enable, the dequeue functionality must only allow a single consumer; versus, multiple concurrent consumers. The dequeue must only commit once the corresponding events have been successfully sent. On any failure, the dequeue must be rolled back. This will have the potential side-effect on at least-once delivery, i.e. it is possible that one or more events could be sent more than once. The dequeue, will not affect enqueue concurrency. Internally, the dequeue stored procedure uses `ROWLOCK` and `UPDLOCK` [hints](https://docs.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table) to achieve.

To enable the code generation of the stored procedures and corresponding consuming .NET code the `EventOutbox` property must be added to the database code generation configuration. Update the `database.beef.xml` (or `database.beef.yaml`) file adding the global configuration. 

``` xml
<CodeGeneration EventOutbox="true" ... />
```

Once configured, re-execute the code-generation to generate the required artefacts.

```
dotnet run codegen
```

The code generation will output the following artefacts.

Name | Type | Description
-|-|-
`spEventOutboxEnqueue.sql` | Stored procedure (DB) | Enqueue one or more passed events.
`spEventOutboxDequeue.sql` | Stored procedure (DB) | Enqueue one or more events (number is specified).
`udtEventOutboxList.sql` | User-defined type (DB) | Enables the passing of one or more events.
`DatabaseEventOutbox.cs` | Data-layer (.NET) | Provides the .NET enqueue and dequeue capability (encapsulating the execution of the above stored procedures), and adds `AddGeneratedDatabaseEventOutbox` extension method (for Dependency Injection).
`DatabaseExtensions.cs` | Data-layer (.NET) | Adds `AddBeefDatabaseEventOutboxPublisherService` extension method (for Dependency Injection).

</br>

### Enqueue entity events

_Beef_, by default when [sending events](../src/Beef.Core/Events/IEventPublisher.cs) implements the logic within the [DataSvc-layer](./Layer-DataSvc.md). As _Beef_ is agnostic to the data persistence technology this default enables the sending of events outside of any Data-layer logic (none or more); however, in a non-transactional manner. This means the default logic is susceptible to potential message loss on failure.

To leverage the _Outbox_ capabilites created above the entity code generation must be configured to move the logic for sending of events into the [Data-layer](./Layer-Data.md) and ensure this occurs transactionally. To enable the `EventOutbox` property must be set to `Database`. Where specified at the global or `Entity` element this indicates that when the operation type is `Create`, `Update` or `Delete` that this behavior is required. Additionally, this can be set directly on the `Operation` element to override for the specific operation; this is required for the other operation types when this behavior is required. 

``` xml
<CodeGeneration EventOutbox="Database" ... />
```

Once configured (`entity.beef.xml` or `entity.beef.yaml`) re-execute the entity code-generation to create and/or update the required artefacts.

```
dotnet run entity
```

The new [logic](../samples/My.Hr/My.Hr.Business/Data/Generated/PerformanceReviewData.cs) within the Data-layer will be similar as follows.

``` csharp
public Task<PerformanceReview> CreateAsync(PerformanceReview value)
{
    return _ef.EventOutboxInvoker.InvokeAsync(this, async () =>
    {
        var __dataArgs = EfMapper.Default.CreateArgs();
        var __result = await _ef.CreateAsync(__dataArgs, Check.NotNull(value, nameofvalue))).ConfigureAwait(false);
        _evtPub.PublishValue(__result, new Uri($"my/hr/performancereview/{_evtPub.FormatKey__result)}", UriKind.Relative), $"My.Hr.PerformanceReview", "Created");
        return __result;
    });
}
```

The transactional outbox capability is orchestrated by the [`DatabaseEventOutboxInvoker`](../src/Beef.Data.Database/DatabaseEventOutboxInvoker.cs) within the _Beef_ [database](../src/Beef.Data.Database/DatabaseBase.cs) (or [Entity Framework](../src/Beef.Data.EntityFrameworkCore/EfDbBase.cs)) classes. Essentially, this replaces the default [`DataInvoker`](../src/Beef.Core/Business/DataInvoker.cs), ensuring that a [TransactionScope](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscope) wraps the enclosed data logic. This will occur just prior to committing the transaction automatically enqueueing all previously published (but not sent) events.

The event publish logic is also moved from the DataSvc-layer to the Data-layer to ensure the generated events are published for inclusion within the outbox.

<br/>

## Dequeue and send

The dequeueing and sending of events must occur independently of the enqueue, generally within a separate process that polls the event data and sends based on some configured interval. As the enqueue and dequeue are independent there will be a delay between the actual database update and the corresponding event(s) send. Depending on overall system requirements this latency may need to be minimized.

To simplify the hosting of the dequeue and send functionality this has been implemented as an [`IHostedService`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) using [`TimerHostedServiceBase`](../src/Beef.Core/Hosting/TimerHostedServiceBase.cs) so that the logic will be invoked at a pre-configured interval. The [`DatabaseEventOutboxPublisherService`](../src/Beef.Data.Database/DatabaseEventOutboxPublisherService.cs) provides the requisite functionality.

This will manage the dequeue and event send transactionally, in that the dequeue will _only_ be committed once all events have been sent successfully. On send failure the database dequeue will be rolled back. This will guarantee all events are successfully sent but may result in events potentially being sent multiple times; i.e. guarantee at-least-once sent semantics. The corresponding receiver/consumer(s) are then responsible for ensuring at-most-once processing semantics where applicable.

By hosting within the same process as the API, the `DatabaseEventOutboxPublisherService` will more readily send events as it links itself to any outbox enqueues, that in turn can advance the underlying timer interval where known _work_ exists to reduce the delay between enqueue and dequeue/send.

To enable within the API the following is required within the `Startup.cs`.

``` csharp
services.AddGeneratedDatabaseEventOutbox();
services.AddBeefDatabaseEventOutboxPublisherService();
```