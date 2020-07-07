# Change log

Represents the **NuGet** versions.

## v3.1.x
- *Enhancement:* Added `IEfDb` and `IEfDbQuery` to support the likes of dependency injection.

## v3.1.4
- *Enhancement:* All references to `DateTime.Now` have been updated to `Cleaner.Clean(DateTime.Now)`.

## v3.1.3
- *Fixed:* A new nullable compile error fixed that appeared with Visual Studio 2019 v16.5.4.

## v3.1.2
- *Fixed:* A query was always applying paging even where not specified. Where no paging is specified all rows will be returned by default (as expected).
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Core 3.1.
- *Enhancement:* Migrated from `System.Data.SqlClient` to `Microsoft.Data.SqlClient`. See https://devblogs.microsoft.com/dotnet/introducing-the-new-microsoftdatasqlclient/.
- *Added:* Nullable rollout phase: https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/

## v2.1.7
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.6
- *Fixed:* Introduced FxCop Analysis to `Beef.Data.EntityFrameworkCore`; this version represents the remediation based on the results.

## v2.1.5
- *Fixed:* Introduced FxCop Analysis to `Beef.Core`; this version represents the remediation based on the results.

## v2.1.4
- *Enhanced:* `EfDbMapper<>.AddStandardProperties` now returns `this` instance to enable fluent-style method-chaining.

## v2.1.3
- *Fixed:* InvokerBase was non functioning as a generic class; reimplemented. Other Invokers updated accordingly.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.