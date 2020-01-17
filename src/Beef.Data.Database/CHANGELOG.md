# Change log

Represents the **NuGet** versions.

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