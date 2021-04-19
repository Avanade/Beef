# Change log

Represents the **NuGet** versions.

## v4.1.5
- *Enhancement:* Added standardized identifier mapping from local (internal) to global (external) where required.
- *Enhancement:* `CdcBackgroundService` renamed to `CdcHostedService`.

## v4.1.4
- *Enhancement:* Added `ILogicallyDeleted.ClearWhereDeleted()` to clear all properties that should not have a value where logically deleted; as the data is technically considered as non-existing.

## v4.1.3
- *Enhancement:* Added support for `ContinueWithDataLoss`.
- *Enhancement:* Added support for `CdcBackgroundService<T>.RunAsync` to enable execution without the `IHostedService` capabilties. For example, direct invocation from the likes of an Azure Function that would use an Azure `TimerTrigger` to handle the multiple long-running nature.
- *Enhancement:* Required change as a result of `ETag` rename to `ETagGenerator`.

## v4.1.2
- *Fixed:* Given recent changes to the `IEventPublisher` related to `Publish` and `Send` separation there was an issue where although the instance is scoped, it is being reused when executing multiple batches. The `IEventPublisher.Reset` was added after the `Send` invocation to correct.

## v4.1.1
- *New:* Initial publish to GitHub.
