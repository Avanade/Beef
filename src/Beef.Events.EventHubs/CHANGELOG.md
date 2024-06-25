# Change log

Represents the **NuGet** versions.

## v4.3.0
- *Enhancement:* Updated to use common version number across all _Beef_ projects; simplifies version and compatibility management.
- *Fixed:* Updated internal dependencies to latest per above framework version.

## v4.2.4
- *Fixed:* Updated internal _Beef_ dependencies to latest.

## v4.2.3
- *Enhancement:* Deprecate `Microsoft.Azure.EventHubs` as per Microsoft [guidance](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/MigrationGuide.md).
- *Fixed:* Updated internal _Beef_ dependencies to latest.

## v4.2.2
- *Enhancement:* Updated for changes to `Beef.Abstractions` and `Beef.Core`.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.4
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.3
- *Enhancement:* Support new `IEventDataContentSerializer` and `IEventDataConverter`.
- *Enhancement:* Added `AzureEventHubsMessageConverter` and `MicrosoftEventHubsMessageConverter` for their respective, different, SDK versions.

## v4.1.2
- *Enhancement:* Leverage the new `Beef.Events.EventMetadata` class that houses the _Beef_ metadata property names.

## v4.1.1
- *New:* Initial publish to GitHub. The Event Hubs specific functionality moved to this assembly. This also coincided with the deprecation of the `EventHubResilientTrigger` functionality. With the introduction of the Azure Functions Retry policies this resiliency functionality has been added into the `EventHubSubsciberHost` with the `SubscriberHostPoisonInvoker`.