# Change log

Represents the **NuGet** versions.

## v4.2.3
- *Enhancement:* Updated for changes to `Beef.Abstractions` and `Beef.Core`.

## v4.2.2
- *Enhancement:* Upgraded the `CloudNative.CloudEvents` NuGet packages to v2.0.0. This required code changes due to the breaking changes to the underlying API. The approach to writing the `EventMetadata` as extension attributes had to be changed to align correctly with the [specification](https://github.com/cloudevents/spec/blob/master/spec.md#extension-context-attributes).

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.9
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.8
- *Fixed:* Corrected issue where converting JSON deserialized `EventData.Value` to an intrinsic type such as a `Guid`.
- *Enhancement:* Added new `IEventSubscriber.ConsiderNullValueAsInvalidData` which defaults to `true`. Will automatically result in an `InvalidData` result where the `EventData.Value` is `null`.
- *Enhancement:* Added new `IEventSubscriber.OriginatingData` and corresponding `EventSubscriber.GetOriginatingData` to enable access to the originating `IEventSubscriberData` where required in advanced scenarios.

## v4.1.7
- *Enhancement:* Added new `IEventDataContentSerializer` and `IEventDataConverter` to more easily facilitate multiple serializers and converters over time.
- *Enhancement:* Leverage `IEventDataContentSerializer` to support [CloudEvents](https://github.com/cloudevents/sdk-csharp) with new `NewtonsoftJsonCloudEventSerializer`. This is the default.
- *Enhancement:* Leverage `IEventDataContentSerializer` to support existing `EventData` format with `NewtonsoftJsonEventDataSerializer` for backwards compatibility.

## v4.1.6
- *Enhancement:* Added new `EventMetadata` class to house the _Beef_ metadata property names.

## v4.1.5
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.
- *Enhancement:* Moved all Event Hubs specific functionality to `Beef.Events.EventHubs`.

## v4.1.4
- *Fixed:* **Breaking change**. The `ResultHandling.Stop` has been renamed to `ResultHandling.ThrowException` as this is more aligned to what is happening. The corresponding`EventSubscriberStopException` has been renamed `EventSubscriberUnhandledException`.

## v4.1.3
- *Enhancement:* Added support for the new unique `EventData.EventId`.
- *Enhancement:* `EventHubPublisher` updated to reflect `IEventHubPublisher` changes.

## v4.1.2
- *Enhancement:* Moved all subscriber host arguments to `EventSubscriberHostArgs` to centralize and enable simple configuration via DI.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.6
- *Enhancement:* Updated the `WebJobsBuilderExtensions.GetConfiguration` to probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration), 2) User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 3) environment variable (using specified prefix), 4) `appsettings.{environment}.json`, 5) `appsettings.json`, 6) `webjobs.{environment}.json` (embedded resource), and 7) `webjobs.json` (embedded resource).

## v3.1.5
- *Enhancement:* Updated `WebJobsBuilderExtensions.GetConfiguration` to build the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration) or User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 2) environment variable (using specified prefix), 3) `appsettings.{environment}.json`, 4) `appsettings.json`, 5) `webjobs.{environment}.json` (embedded resource), and 6) `webjobs.json` (embedded resource).

## v3.1.4
- *Enhancement:* All references to `DateTime.Now` have been updated to `Cleaner.Clean(DateTime.Now)`.

## v3.1.3
- *Enhancement:* The `EventSubscriberHost` has been futher extended to support `InvalidEventData()`. This occurs where the `EventData` is not considered valid, or the `Value` is unable to be deserialized.
- *Fixed:* The `ResilientEventHubProcessor` was not always catching and actioning the internal `EventSubscriberStopException`.

## v3.1.2
- *Enhancement:* Added `EventHubSubscriberHost.ExecutionContext(createFunc)` to more easily support the creation of a customised `ExecutionContext` instance.
- *Enhancement:* The `IEventSubscriber.ReceiveAsync` must now (**breaking change**) return a `Result`; these include `Success()`, `DataNotFound()` (also automatically inferred from a `NotFoundException`), `InvalidData()` (also automatically inferred from a `ValidationException` or `BusinessException`). Otherwise, for anything else, just allow an `Exception` to bubble out.
- *Enhancement:* `EventSubscriberHost` has been extended to support `NotSubscribedHandling` (defaults to `ContinueSilent`), `DataNotFoundHandling` (defaults to `Stop`) and `InvalidDataHandling` (defaults to `Stop`). `ResultHandling` options are `Stop`, `ContinueSilent`, `ContinueWithLogging` and `ContinueWithAudit`.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).

## v2.1.3
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.2
- *Fixed:* Introduced FxCop Analysis to `Beef.Events`; this version represents the remediation based on the results.

## v2.1.1
- *New:* Initial publish to GitHub. New capability to support an Event-driven Architecture; specifically leveraging Azure EventHubs.