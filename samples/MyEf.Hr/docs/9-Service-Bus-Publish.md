# Step 9 - Service Bus Publish

To further support the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) the events need to be dequeued from the database and sent to the final messaging subsystem via a _message relay_ process.

<br/>

## Message relay

This is achieved by dequeuing a set of events from the database, again within the context of a database transaction, and forwarding the events/messages to the messaging subsystem. On successful completion, the database transaction can be committed; otherwise, on error the dequeue should be rolled back, and the forwarding re-attempted.

This _message relay_ process will result in at least once publishing semantics; i.e. where there was an error and retry the same events/messages may be sent again. It is the responsibility of the end subscriber to handle multiple events/messages; being the requirement for [duplicate](https://learn.microsoft.com/en-us/azure/service-bus-messaging/duplicate-detection) checking. The [`EventData.Id`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventDataBase.cs) by default is unique and should be used for this purpose.

To achieve in-order publishing the _message relay_ process should execute as a [singleton](https://en.wikipedia.org/wiki/Singleton_pattern); i.e. only a single (synchronized) process can execute to guarantee in-order sequencing. Within an event-driven architecture the order in which the events/messages are generated is critical, and as such this order _must_ be maintained (at least from a publishing perspective).

<br/>

## Separation and synchronization

The dequeuing and forwarding should occur in a separate process to that in which they are generated; the success or failure of the  _message relay_ should not impact the originating operation.

_CoreEx_ provides the [`EventOutboxHostedService`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxHostedService.cs) that enables the _message relay_ processing; in that it hosts the previously generated [`EventOutboxDequeue`](../MyEf.Hr.Business/Data/Generated/EventOutboxDequeue.cs) and will execute (`DequeueAndSendAsync`) on a configured interval.

The `EventOutboxHostedService` inherits from the [`SynchronizedTimerHostedServiceBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/SynchronizedTimerHostedServiceBase.cs) which requires an [`IServiceSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/IServiceSynchronizer.cs) that is responsible for enabling the synchronized singleton behaviour.

The following synchronizers are provided.

Class | Description
-|-
[`ConcurrentSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/ConcurrentSynchronizer.cs) | Enables concurrency; i.e. there is _no_ synchronization.
[`FileLockSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/FileLockSynchronizer.cs) | Performs synchronization by taking an exclusive lock on a [file](https://learn.microsoft.com/en-us/dotnet/api/system.io.file.create) (Windows or Linux).
[`BlobLeaseSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/Storage/BlobLeaseSynchronizer.cs) | Performs synchronization by acquiring a [lease](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-lease) on an Azure Storage Blob.

<br/>

## Host agnostic

The [`EventOutboxDequeue`](../MyEf.Hr.Business/Data/Generated/EventOutboxDequeue.cs) and [`EventOutboxHostedService`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxHostedService.cs) are host agnostic, in that they can be hosted by any of the following.

- [ASP.NET](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [Console application](https://learn.microsoft.com/en-us/visualstudio/get-started/csharp/tutorial-console)
- [Windows service](https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service)
- [Cloud background job](https://learn.microsoft.com/en-us/azure/architecture/best-practices/background-jobs)

<br/>

## Azure Service Bus Sender

The _CoreEx_ [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSender.cs) enables the sending ([`IEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs)) of events/messages to [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview). As an `IEventSender` can send none or more events/messages these will be sent leveraging batches (improves performance and resiliency) as per the Microsoft [guidance](https://github.com/Azure/azure-sdk-for-net/tree/Azure.Messaging.ServiceBus_7.1.0/sdk/servicebus/Azure.Messaging.ServiceBus/#send-and-receive-a-batch-of-messages).

<br>

## Implement as ASP.NET host

For the purposes of the `MyEf.Hr` solution the Service Bus Publishing will be hosted within the `MyEf.Hr.Api` host process; and it is assumed it will be deployed to Azure and as such the `BlobLeaseSynchronizer` will be leveraged.

<br/>

### Dependency Injection (DI) configuration

The Service Bus Publishing requires the following services registered.

Service | Description
-|-
[`Az.ServiceBusClient`](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusclient) | The `AddSingleton()` is used to register the `Az.ServiceBusClient` as a singleton service, leveraging the `ServiceBusConnectionString` configuration setting.
[`IServiceSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Hosting/IServiceSynchronizer.cs) | The `AddSingleton()` is used to register the [`BlobLeaseSynchronizer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/Storage/BlobLeaseSynchronizer.cs) as a singleton service. An Azure [`BlobContainerClient`](https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient) instance is required leveraging the `StorageConnectionString` configuration setting and specifying the storage container name.
[`IServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/IServiceBusSender.cs) | The `AddScoped()` is used to register the [`ServiceBusSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSender.cs) as a scoped service.
[`IHostedService`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostedservice) | The `AddSqlServerEventOutboxHostedService()` registers the [`EventOutboxHostedService`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/Outbox/EventOutboxHostedService.cs) as an `IHostedService` that runs in the background for the life of the ASP.NET host. During registeration a new [`EventOutboxDequeue`](../MyEf.Hr.Business/Data/Generated/EventOutboxDequeue.cs) instantiation is defined to set the underlying `EventOutboxDequeueFactory` property; as a new internally managed [_scoped_](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicescopefactory.createscope) instance is required per invocation).

To introduce, append the following within the API [`Startup`](../MyEf.Hr.Api/Startup.cs) class after the earlier event service registration. 

``` csharp
// Add transactional event outbox dequeue dependencies.
services.AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<HrSettings>().ServiceBusConnectionString));
services.AddSingleton<IServiceSynchronizer>(sp => new BlobLeaseSynchronizer(new Azure.Storage.Blobs.BlobContainerClient(sp.GetRequiredService<HrSettings>().StorageConnectionString, "event-synchronizer")));
services.AddScoped<IServiceBusSender, ServiceBusSender>();

// Add transactional event outbox dequeue hosted service (_must_ be explicit with the IServiceBusSender as he IEventSender).
services.AddSqlServerEventOutboxHostedService(sp =>
{
    return new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<IServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>());
});
```

The latest `CoreEx.Azure` NuGet [package](https://www.nuget.org/packages/CoreEx.Azure) will need to be added to the `MyEf.Hr.Api` project. Additionally, `Startup` will require the following `using` statements added.

``` csharp
using CoreEx.Azure.ServiceBus;
using CoreEx.Azure.Storage;
using CoreEx.Database;
using CoreEx.Hosting;
using Az = Azure.Messaging.ServiceBus;
```

<br/>

### Configuration settings

The [`HrSettings`](../MyEf.Hr.Business/HrSettings.cs) class should have the following properties added.

``` csharp
/// <summary>
/// Gets the Azure service bus connection string.
/// </summary>
public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");

/// <summary>
/// Gets the Azure storage connection string.
/// </summary>
public string StorageConnectionString => GetRequiredValue<string>("ConnectionStrings__Storage");
```

<br/>

The corresponding [`appsettings.json`](../MyEf.Hr.Api/appsettings.json) also needs to be updated to provide the requisite configuration; replace the existing JSON with the following. The '*' within denotes that the configuration settings are accessed internally by _CoreEx_ at runtime and therefore do not need to be specifically defined as `HrSettings` properties.

Setting | Description
-|-
`ConnectionStrings:ServiceBus` | The Azure Service Bus connection string.
`ConnectionStrings:Storage` | The Azure Storage connection string.
`ServiceBusSender:QueueOrTopicName`* | The Azure Service Bus Queue or Topic name depending on requirements.
`EventOutboxHostedService:MaxDequeueSize`* | The maximum number of events to dequeue and send per batch.
`EventOutboxHostedService:Interval`* | The interval ([TimeSpan](https://learn.microsoft.com/en-us/dotnet/api/system.timespan)) to poll the queue and send; set to poll every 10 seconds.
 
``` json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "Database": "Data Source=.;Initial Catalog=MyEf.Hr;Integrated Security=True;TrustServerCertificate=true",
    "ServiceBus": "add-top-secret-connection-string",
    "Storage": "add-top-secret-connection-string"
  },
  "ServiceBusSender": {
    "QueueOrTopicName": "event-stream"
  },
  "EventOutboxHostedService": {
    "MaxDequeueSize": "10",
    "Interval": "00:00:10"
  }
}
```

<br/>

## Unit testing

There are no specific provisions for the unit testing of the Service Bus Publishing as it requires a dependent messaging subsystem, being Azure Service Bus.

However, to minimize any impact to the other existing unit tests the `EventOutboxHostedService` should be disabled. To disable, add a new `appsettings.unittest.json` file to `MyEf.Hr.Test` project with the following contents. Go to the file properties and set _Copy to Output Directory_ to _Copy if newer_. 

``` json
{
  "EventOutboxHostedService": {
    "Enabled": false
  }
}
```

<br/>

## Localized testing

To achieve a basic test within a developers machine then the API host should be started. By navigating to the [Swagger](https://swagger.io/) page this will result in the ASP.NET host starting, which in turn will execute the registered `IHosterService`. After a small delay the `EventOutboxHostedService` will begin processing and the following should occur.

- Database dequeue - the entries previously enqueued within the `Outbox.EventOutbox` will have the `DequeueDate` column updated signalling that the event was dequeued and sent successfully.
- Service Bus Message - the corresponding events/messages should appear within the Azure Service Bus (queue or topic).

<br/>

## Verify

At this stage we now have our events being published to Azure Service Bus. This essentially concludes the `Hr` domain functionality, with respect to enabling the requisite APIs and publishing of corresponding events (where applicable).

*Verification Steps TBD*

## Next Step

Next we will create a new _Security_ domain that will perform a [Service Bus Subscribe](./Service-Bus-Subscribe.md) of the _Termination_ related events and proxy [Okta](https://www.okta.com/) (as the fictitious company's identity solution) automatically _Deactivating_ the Employee's account.