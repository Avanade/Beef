# Change log

Represents the **NuGet** versions.

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