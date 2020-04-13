# Change log

Represents the **NuGet** versions.

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
- *Fixed:* InvokerBase was non functioning as a generic class; reimplemented. Other Invokers updated accordingly.

## v2.1.4
- *New:* Added SqlTransientErrorNumbers to DatabaseBase; standardised list that can be used for retries, etc.

## v2.1.3
- *Fixed:* ETag not returned for Reference Data items.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.