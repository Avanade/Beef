# Change log

Represents the **NuGet** versions.

## v4.1.2
- *Enhancement:* Leverage the new `Beef.Events.EventMetadata` class that houses the _Beef_ metadata property names.

## v4.1.1
- *New:* Initial publish to GitHub. The Event Hubs specific functionality moved to this assembly. This also coincided with the deprecation of the `EventHubResilientTrigger` functionality. With the introduction of the Azure Functions Retry policies this resiliency functionality has been added into the `EventHubSubsciberHost` with the `SubscriberHostPoisonInvoker`.