# Change log

Represents the **NuGet** versions.

## v4.2.3
- *Enhancement:* Add support for using [AutoMapper](https://docs.automapper.org/en/stable/index.html) for the entity-to-entity based mapping (remove existing `EntityMapper`-based functionality) - may result in _breaking changes_:
  - `EfDbArgs<T, TModel>` renamed to `EfDbArgs` and updated to support the new AutoMapper requirements.
  - `IEfDbArgs` removed to simplify.
  - `EfDbBase` and `EfDbQuery` updated to support AutoMapper mappings.
  - `EfMapper` was deleted.
  - _Note:_ All code-generated artefacts must be re-generated.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.4
- *Enhancement:* Added new `IEfDbContext` to enable access to the underlying `IDatabase` instance.
- *Enhancement:* Added `IEfDb.EventOutboxInvoker` to access the corresponding event outbox capability.
- *Enhancement:* Added `ILogicallyDeleted` interface. Where implemented the `EfDbBase.Delete` will perform a logical delete versus a physical delete.
- *Enhancement:* Added `IMultiTenant` interface. Where implemented the `EfDbBase.Create` will automatically update the `TenantId` from the `ExecutionContent.Current.TenantId`.
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.3
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v4.1.2
- *Fixed:* A `NotFoundException` will be thrown on a delete if it does not exist; otherwise, the application will assume it deleted successfully and the likes of a related event could be raised incorrectly.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

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