# Beef.Events

[![NuGet version](https://badge.fury.io/nu/Beef.Events.svg)](https://badge.fury.io/nu/Beef.Events)

To support the goals of an [Event-driven Architecture](https://en.wikipedia.org/wiki/Event-driven_architecture) _Beef_ enables the key capabilities - the publishing and subscribing of events (messages) to and from an event-stream (or equivalent).

![Layers](../../docs/images/EventDrivenArchitecture.png "Event-Driven Architecture")

<br/>

## Azure Event Hubs

_Beef_ is largely agnostic to the underlying event/messaging infrastructure (event-stream) and for the most part this must be implemented by the developer. This is by design, to decouple _Beef_ and enable multiple capabilities to be supported as required.

However, further capabilities have also been enabled within _Beef_, leveraging the agnostic capabilities described above, to directly support [Azure Event Hubs](https://azure.microsoft.com/en-us/services/event-hubs/) as an event-stream.

<br/>

## Event Data

The _Beef_ [`EventData`](../../src/Beef.Core/Events/EventData.cs) provides a standardised, and flexible, message contract that is used to both publish and subscribe to events in a consistent manner. The key properties as follows:

Property | Description
-|-
`Subject` | The subject (noun) that uses dot-notation to uniquely describe an entity instance.
`Action` | The action (verb) that describes what was performed on the subject.
`Key` | The corresponding entity key (could be single value or an array of values).
`Value` | The optional value that contains the underlying message data.

The code-generation of _Beef_ will automatically infer the `Subject` and `Action` for an entity and the operation being performed; this can be overridden. The `Subject` defaults to `<domain>.<entity>.<uniquekey>`; there is a code-gen configuration attribute [`EventSubjectRoot`](../../docs/Entity-CodeGeneration-element.md) that prepends the specified value to the `Subject`. The `Action` defaults to the `Operation` name; there is a code-gen configuration attribute [`EventActionFormat`](../../docs/Entity-CodeGeneration-element.md) that enables automatic reformatting to past tense where required (`Create` would be renamed to `Created` for example).

For example, an invoice creation event could result in the following:
- `Subject:` `Billing.Invoice.7634626f-edee-e911-bd3b-bc8385e26041`
- `Action:` `Create`
- `Key:` `7634626f-edee-e911-bd3b-bc8385e26041`

<br/>

## Publishing

The [`Event`](../../src/Beef.Core/Events/Event.cs) provides the standardised event processing/publishing. The underlying `PublishAsync` set of methods provide the means to publish the event(s). By default, the events are not processed until a publisher has been registered; the `Register` method will allow one or more publishers to be registered.

> To publish to Azure Event Hubs the [`EventHubPublisher`](../../src/Beef.Events/Publish/EventHubPublisher.cs) can be used. 

The publishing of events is integrated into the API processing pipeline; this is enabled within the [Service orchestration](../../docs/Layer-DataSvc.md) layer to ensure consistency of approach. All `Create`, `Update` and `Delete` operations raise events automatically; others will need to be issued directly by the developer.

<br/>

## Subscribing

Within the _Beef_ context each Domain would subscribe (listen) to events and process accordingly. The following components are required:

- **Subscribers** - one or more subscribers;
- **Host** - a host to receive events and route to a subscriber.

<br/>

### Subscribers

To start with a developer would create one or more subcribers; one per `Subject` and `Action` combination. A subscriber is created by inheriting from [`EventSubscriber`](../../src/Beef.Events/Subscribe/EventSubscriber.cs) or [`EventSubscriber<T>`](../../src/Beef.Events/Subscribe/EventSubscriber.cs) (defines the `Event.Value` `Type` to automatically deserialize) specifying the `Subject` template (supports wildcards) and optional `Action`(s); finally implementing the `ReceiveAsync` logic. The `ReceiveAsync` must return a [`Result`](./Subscribe/Result.cs) to describer the processing outcome; being one of: `Success`, `DataNotFound` (also inferred from a [`NotFoundException`](../../src/Beef.Core/NotFoundException.cs)), `InvalidData` (also inferred from a [`ValidationException`](../../src/Beef.Core/ValidationException.cs) or [`BusinessException`](../../src/Beef.Core/BusinessException.cs)). 

There are the following properties that can be set that will change the runtime logic:

Property | Description
-|-
[`UnhandledExceptionHandling`](../../src/Beef.Events/Subscribe/UnhandledExceptionHandling.cs) | Provides the unhandled `Exception` option as either `Stop` (stops and bubbles up the `Exception` allowing the [host](#Host) process to determine the appropriate action) or `Continue` (skips and continues effectively swallowing the `Exception`). Defaults to `Stop`.
[`RunAsUser`](../../src/Beef.Events/Subscribe/RunAsUser.cs) | Provides the run as user option as either `Originating` (originating user being `EventData.Username`) or `System` (`EventSubscriberHost.SystemUsername`). Defaults to `Originating`.
`InvalidEventDataHandling` | Overrides the default [`ResultHandling`](./Subscribe/ResultHandling.cs) as specified for the owning [`EventSubscriberHost`](#Subscriber-Host).
`DataNotFoundHandling` | Overrides the default [`ResultHandling`](./Subscribe/ResultHandling.cs) as specified for the owning [`EventSubscriberHost`](#Subscriber-Host).
`InvalidDataHandling` | Overrides the default [`ResultHandling`](./Subscribe/ResultHandling.cs) as specified for the owning [`EventSubscriberHost`](#Subscriber-Host).

See [example](../../samples/Demo/Beef.Demo.Functions/Subscribers/PowerSourceChangeSubscriber.cs) below:

``` csharp
public class PowerSourceChangeSubscriber : EventSubscriber<string>
{
    // Subscribe to all events with a Subject starting with 'Demo.Robot.' as per wildcard
    // and Action 'PowerSourceChange'.
    public PowerSourceChangeSubscriber() : base("Demo.Robot.*", "PowerSourceChange") { }

    public override async Task<Result> ReceiveAsync(EventData<string> @event)
    {
        var val = @event.Value;
        // Processing logic...
        return Result.Success();
    }
}
``` 

<br/>

### Subscriber Host

The [`EventSubscriberHost`](../../src/Beef.Events/Subscribe/EventSubscriberHost.cs) provides the base capabilities for the host. The host is responsible for receiving (`ReceiveAsync`) each event and converting to an [`EventData`](../../src/Beef.Core/Events/EventData.cs) instance. The host checks whether there is a [subscriber](#subscribers) and will invoke where found; otherwise, the event will be skipped (ignored).

The `EventSubscriberHost` infers the [subscribers](#subscribers) automatically by reflecting on the `Assembly` that instatiates the host. The subscribers can alternatively be specified by using the [`EventSubscriberHostArgs`](../../src/Beef.Events/Subscribe/EventSubscriberHostArgs.cs).

There are the following properties that can be set that will change the runtime logic:

Property | Description
-|-
`AreMultipleMessagesSupported` | Indicates whether multiple messages can be processed; default is `false`.
`InvalidEventDataHandling` | Determines the behaviour ([`ResultHandling`](./Subscribe/ResultHandling.cs)) where the `EventData` is unable to be converted or has no `Subject` specified. Defaults to `Stop`.
`DataNotFoundHandling` | Determines the behaviour ([`ResultHandling`](./Subscribe/ResultHandling.cs)) where the corresponding data is not found. Defaults to `Stop`.
`InvalidDataHandling` | Determines the behaviour ([`ResultHandling`](./Subscribe/ResultHandling.cs)) where the specified data is invalid. Defaults to `Stop`.

_Warning:_ The [`ResultHandling.Stop`](./Subscribe/ResultHandling.cs) implies that the data is currently unable to be processed; and should be attempted again (it could be transient) - this however could result in a _poison_ message that could stop further processing until resolved. Consider the `ResultHandling.ContinueWithAudit`, `ResultHandling.ContinueWithLogging` or `ResultHandling.ContinueSilent` as an alternative.

The [`ExecutionContext`](../../src/Beef.Core/ExecutionContext.cs) creation can be overridden by overridding the `CreateExecutionContext` method where neccessary.

> To subscribe to Azure Event Hubs the [`EventHubSubscriberHost`](../../src/Beef.Events/Subscribe/EventHubSubscriberHost.cs) can be used. 

<br/>

### Azure Function

The process host, for example an [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/), will create/instantiate the [subscriber host](#Subscriber-Host). To set up the likes of connection strings, etc. a [startup](../../samples/Demo/Beef.Demo.Functions/Startup.cs)-style component will be required.

An [example](../../samples/Demo/Beef.Demo.Functions/EventSubscriber.cs) using a [Resilient Event Hub Trigger](#Resilient-Event-Hub-Trigger) is as follows:

``` csharp
public static class EventSubscriber
{
    [FunctionName("EventSubscriber")]
    public static async Task Run([ResilientEventHubTrigger] EventHubs.EventData @event, ILogger log)
    {
        await EventHubSubscriberHost.Create(log).ReceiveAsync(@event);
    }
}
```

<br/>

## Resilient Event Hub Trigger

A new custom Azure Function Trigger, [`ResilientEventHubTrigger`](./Triggers/ResilientEventHubTriggerAttribute.cs), has been created to support greater resiliency when processing (reading from) an Azure Event Hub Consumer Group. This should be used in scenarios where each message **_must be_** processed, unless explicitly skipped.

Unfortunately, probably by design, the out-of-the-box [`EventHubTrigger`](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs) does not have any resiliency built-in. In that, if the underlying Function being executed fails (throws an `Exception`) this is essentially logged and swallowed, moving onto the next message in sequence (for the Consumer Group + Partition Id).

This behaviour is tolerable where message loss is acceptable, for example processing sensor data. Whilst this loss is not ideal it should not result in underlying data inconsistencies, etc.

For the Event-Driven approach between the likes of Domains and/or Applications it is assumed that each message *must* be processed; either explicitly ignored (skipped), or acted on; i.e. message loss is not acceptable.

<br/>

### Logic

The underpinnings of the [`ResilientEventHubTrigger`](./Triggers/ResilientEventHubTriggerAttribute.cs) logic is:

- All unhandled Function exceptions will be caught for a message; and then re-processed (retry) until successfully processed; or alternatively, skipped (explictly).
- The retry logic leverages [Polly](https://github.com/App-vNext/Polly) and uses a base 2 [exponential back-off](https://github.com/App-vNext/Polly/wiki/Retry#exponential-backoff) strategy. Each retry timespan is calculated as 2\^count to calculate the sleep seconds. For example the 3rd retry would be 2\^3, being 8 seconds. There is also a [jitter](https://github.com/App-vNext/Polly/wiki/Retry-with-jitter) to add a random number of milliseconds to minimise bunching. A maximum timespan can also be specified, defaults to 15 minutes - so that it eventually falls back to a standard frequency upon which to retry.
- At times the exception/failure could be transient, and as such, the message is not considered *Poison* until it has been retried a number of times. By default, the message will be logged as *Poison* after 6 attempts (2\^6, being 64 seconds). Initially, warnings will be logged; then they will be treated as errors.
- The *Poison* message will be written to an Azure Storage Table (`EventHubPoisonMessage`) for the Event Hub + Consumer Group + Partition Id, that includes the likes of: `Exception`, `EventData.Body` (as a string), Function Name, etc. There is a property `SkipMessage` that determines whether the message is to be skipped (defaults to `false`).
- Whilst there is a *Poison* message with a `SkipMessage` of `false` it will continue to retry - even if the Function is restarted; i.e. the message will continue to be retried.
- To skip the *Poison* message the `SkipMessage` must be set explicitly to `true`. Each time a *Poison* message is re-tried it looks for (re-reads) the `SkipMessage` value and acts accordingly. Once skipped the Poison messages is removed (deleted) from the storage table; a copy is written to a corresponding skipped Azure Storage Table (`EventHubPoisonMessageSkipped`) as an audit.
- Where the [`ResultHandling`](./Subscribe/ResultHandling.cs) of an `EventSubscriber` is `ResultHandling.ContinueWithAudit` the message will be written to the skipped Azure Storage Table (`EventHubPoisonMessageSkipped`) as the audit. The `SkipMessage` value will be `false` to indicate that this was not explicitly (externally) set due to being *Poison*. 

The [`PoisonMessagePersistence`](./Triggers/PoisonMessages/PoisonMessagePersistence.cs) provides the *Poison* message Azure storage persistence functionality described. This behaviour can be overridden using the [`IPoisonMessagePersistence`](./Triggers/PoisonMessages/IPoisonMessagePersistence.cs) where required.

<br/>

### Configuration options

The following additional configuration options exist:

Option | Description
-|-
`MaxRetryMinutes` | The maximum retry `TimeSpan`. Defaults to 15 minutes. Maximum allowed value is 24 hours.
`LogPoisonMessageAfterRetryCount` | Determines whether a possible Poison message has been encountered and should be logged (persisted) after the specified retry count (must be between 1 and 10, defaults to 6).

<br/>

### Constraints

The following run-time constraints exist:

- Given the required resiliency retry requirements within this trigger it is unable to be executed on an Azure consumption plan; i.e. must be **always-on**.
- The invoked function can only accept a single `EventData` parameter; i.e. arrays are **not** supported.