# Change log

Represents the **NuGet** versions.

## v4.2.4
- *Fixed:* Issue [157](https://github.com/Avanade/Beef/issues/157) fixed. Renamed `DatabasePropertyMapper.DbType` method to be `SetDbType` to be consistent with the other related _set_ methods.

## v4.2.3
- *Fixed:* Issue [154](https://github.com/Avanade/Beef/issues/154) fixed. The field name is now included in the exception message when using `DatabaseRecord.GetOrdinal` to improve the usefulness of the message (i.e. it will identify the missing field).

## v4.2.2
- *Enhancement:* Changes related to the introduction of AutoMapper. The _Beef_ custom mapping is now solely maintained for the ADO.NET stored procedure parameter and corresponding data reader mapping only.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.6
- *Enhancement:* Added Event Outbox support with `DatabaseEventOutboxBase`, `DatabaseEventOutboxItem` and `DatabaseEventOutboxInvoker` (enqueue events to the database on publish/send).
- *Enhancement:* Added `IDatabase.EventOutboxInvoker` to access the corresponding event outbox capability.
- *Enhancement:* Added `DatabaseEventOutboxPublisherService` that is the `IHostedService` to dequeue events from the databade and publish/send.
- *Enhancement:* Added supported for `bool?` on `DatabaseParameters.When`.
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.5
- *Fixed:* `DatabaseRowVersionConverter.ConvertToSrce` will now return `null` when the `byte[]` is empty; versus, an empty string.

## v4.1.4
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v4.1.3
- *Enhancement:* Added `DeletedByName` and `DeletedDateName` to `DatabaseColumns`. These are currently required for the database code-generation tooling.
- *Fixed:* `DatabaseWildcard` needed `#pragma warning disable CS8618` added based on errant compiler warning introduced with Visual Studio 2019 v16.8.2.
- *Enhancement:* Added `DatabaseMapper.CreateAuto` where properties are added automatically (assumes the property and column names share the same name).

## v4.1.2
- *Fixed:* The `DatabasePropertyMapper.MapToDb()` where mapping to a sub-property (via a `DatabaseMapper` was mapping each sub-property where the overarching property value was `null`. This resulted in each `DbParameter` being set to its default value (from a .NET perspective) which did not account for database nullability. The underlying `DatabaseMapper` will now _not_ be invoked where `null` and the properties should default as per the invoked stored procedure definition.
- *Fixed:* A `NotFoundException` will be thrown on a delete if it does not exist; otherwise, the application will assume it deleted successfully and the likes of a related event could be raised incorrectly.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.6
- *Enhancement:* `DatabaseBase.DefaultDateTimeKind` replaced with (_breaking change_) `DatabaseBase.DateTimeTransform`, and defaults to `UseDefault`.
- *Enhancement:* `DatabaseRecord.GetValue` updated to use `Cleaner.Clean` when retrieving a `DateTime` value.
- *Enhancement:* All references to `DateTime.Now` have been updated to `Cleaner.Clean(DateTime.Now)` to ensure the value is set as configured by default.

## v3.1.5
- *Fixed:* `DatabaseRecord.GetValue` changed to use the underlying type of nullable types for `IsEnum` checking and parsing.

## v3.1.4
- *Fixed:* A new nullable compile error fixed that appeared with Visual Studio 2019 v16.5.4.

## v3.1.3
- *Enhancement:* `Database.SetSqlSessionContext` now supports the passing of a `UserId`. This will default to the `ExecutionContext.UserId`.

## v3.1.2
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).
- *Added:* Nullable rollout phase: https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/
- *Enhancement:* Migrated from `System.Data.SqlClient` to `Microsoft.Data.SqlClient`. See https://devblogs.microsoft.com/dotnet/introducing-the-new-microsoftdatasqlclient/.
- *Enhancement:* All `DatabaseCommand` and `DatabaseBase` database operations are now all asynchronous and are suffixed by `Async` as per the expected convention. The previous synchronous operations have been removed; this will result in breaking code changes. Operations that previously had an `out int returnValue` are renamed with a `WithValueAsync` suffix and now return the value directly instead.
- *Removed:* The `DatabasePerformanceTimer` has been removed. Using other tools such as AppInsights provides this insight.

## v2.1.8
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.
- *Added:* A new `SqlRetryDatabaseInvoker` is provided for usage with a Microsoft SQL Server to perform a retry (exponential back-off) where a known transient error is encountered.

## v2.1.7
- *Enhanced:* New `MultiSetSingleArgs` and `MultiSetCollArgs` abstract classes added to enable simplier custom implementations. These are now used by the existing generic implementations.
- *Fixed:* Introduced FxCop Analysis to `Beef.Data.Database`; this version represents the remediation based on the results.

## v2.1.6
- *Fixed:* Introduced FxCop Analysis to `Beef.Core`; this version represents the remediation based on the results.

## v2.1.5
- *Fixed:* `InvokerBase` was non functioning as a generic class; reimplemented. Other _Invokers_ updated accordingly.

## v2.1.4
- *New:* Added `SqlTransientErrorNumbers` to `DatabaseBase`; standardised list that can be used for retries, etc.

## v2.1.3
- *Fixed:* `ETag` not returned for Reference Data items.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.