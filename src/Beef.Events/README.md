# Beef.Events

[![NuGet version](https://badge.fury.io/nu/Beef.Events.svg)](https://badge.fury.io/nu/Beef.Events)

To support the goals of an [Event-driven Architecture](https://en.wikipedia.org/wiki/Event-driven_architecture) _Beef_ enables the key capabilities; the publishing and subscribing of events (messages) to and from an event-stream (or equivalent).

![Layers](../../docs/images/EventDrivenArchitecture.png "Event-Driven Architecture")

<br/>

## Azure Event Hubs

_Beef_ is largely agnostic to the underlying event/messaging infrastructure (event-stream) and for the most part this must be implemented by the developer. This is by design, to decouple _Beef_ and enable multiple capabilities to be supported as required.

However, further capabilities have also been enabled within _Beef_, leveraging the agnostic capabilities, to directly support [Azure Event Hubs](https://azure.microsoft.com/en-us/services/event-hubs/) as an event-stream.

<br/>

## Publishing

**UNDER CONSTRUCTION...**

The main class


The publishing of events is integrated into the API processing pipeline; this is enabled within the [Service orchestration](./docs/Layer-DataSvc.md) layer to ensure consistency of approach.




<br/>

## Subscribing

**UNDER CONSTRUCTION...**

<br/>

## Resilient Event Hub Trigger

A new custom Trigger, [`ResilientEventHubTrigger`](./Triggers/ResilientEventHubTriggerAttribute.cs), has been created to support greater resiliency when processing (reading from) an Azure Event Hub Consumer Group. This should be used in scenarios where each message _must be_ processed, unless explicitly skipped.

Unfortunately, probably by design, the out-of-the-box [`EventHubTrigger`](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs) does not have any resiliency built-in. In that, if the underlying Function being executed fails (throws an `Exception`) this is essentially logged and swallowed, moving onto the next message in sequence (for the Consumer Group + Partition Id).

This behaviour is fine where message loss is acceptable, for example processing sensor data. Whilst this loss is not ideal it will not result in underlying data inconsistencies, etc.

For the Event-Driven approach between Domains it is assumed that each message must be processed; either explicitly ignored (skipped), or acted on; i.e. message loss is not acceptable.

<br/>

### Logic

The underpinnings of the [`ResilientEventHubTrigger`](./Triggers/ResilientEventHubTriggerAttribute.cs) logic is:

- All unhandled Function exceptions will be caught for a message; and then re-processed (retry) until successfully processed; or alternatively, skipped.
- The retry logic leverages [Polly](https://github.com/App-vNext/Polly) and uses a base 2 [exponential back-off](https://github.com/App-vNext/Polly/wiki/Retry#exponential-backoff) strategy. Each retry timespan is calculated as 2\^count to calculate the sleep seconds. For example the 3rd retry would be 2\^3, being 8 seconds. There is also a [jitter](https://github.com/App-vNext/Polly/wiki/Retry-with-jitter) added to add a random number of milliseconds to minimise bunching. A maximum timespan can also be specified, defaults to 15 minutes - so that it eventually falls back to a standard frequency upon which to retry.
- At times the exception/failure could be transient, and as such, the message is not considered *Poison* until it has been retried a number of times. By default, the message will be logged as *Poison* after 6 attempts (2^6, being 64 seconds). Initially, warnings will be logged; then they will be treated as errors.
- The *Poison* message will be written to an Azure Storage Table (`EventHubPoisonMessage`) for the Event Hub + Consumer Group + Partition Id, that includes the likes of: `Exception`, `EventData.Body` (as a string), Function Name, etc. There is a property `SkipMessage` that determines whether the message is to be skipped (defaults to `false`).
- Whilst there is a *Poison* message with a `SkipMessage` of `false` it will continue to retry - even if the Function is restarted, the message will continue to be retried.
- To skip the *Poison* message the `SkipMessage` must be set to `true`. Each time a *Poison* message is re-tried it looks for (re-reads) the `SkipMessage` value and acts accordingly. Once skipped the Poison messages is removed (deleted) from the storage table; a copy is written to a corresponding skipped Azure Storage Table (`EventHubPoisonMessageSkipped`).

The []() provides the 

<br/>

### Configuration options

The following additional configuration options exist:

Option | Description
-|-
`MaxRetryMinutes` | The maximum retry `TimeSpan`. Defaults to 15 minutes. Maximum allowed value is 24 hours.
`LogPoisonMessageAfterRetryCount` | Determines whether a possible Poison message has been encountered and should be logged (persisted) after the specified retry count (must be between 1 and 10, defaults to 6).

### Constraints

The following run-time constraints exist:

- Given the required resiliency retry requirements within this trigger it is unable to be executed on an Azure consumption plan; i.e. must be **always-on**.
- The invoked function can only accept a single `EventData` parameter; i.e. arrays are **not** supported.