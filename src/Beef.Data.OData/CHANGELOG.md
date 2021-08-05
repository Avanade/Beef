# Change log

Represents the **NuGet** versions.

## v4.2.2
- *Enhancement:* Add support for using [AutoMapper](https://docs.automapper.org/en/stable/index.html) for the entity-to-entity based mapping (remove existing `EntityMapper`-based functionality) - may result in _breaking changes_:
  - `ODataDbArgs<T, TModel>` renamed to `ODataDbArgs` and updated to support the new AutoMapper requirements.
  - `IODataDbArgs` removed to simplify.
  - `ODataBase` and `ODataQuery` updated to support AutoMapper mappings.
  - `ODataMapper` was deleted.
  - _Note:_ All code-generated artefacts must be re-generated.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.4
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.3
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v4.1.2
- *Fixed:* A `NotFoundException` will be thrown on a delete if it does not exist; otherwise, the application will assume it deleted successfully and the likes of a related event could be raised incorrectly.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.x
- *Enhancement:* Added `IOData` to support the likes of dependency injection.

## v3.1.2
- *Enhancement:* Refactored to leverage [Simple.OData.Client](https://github.com/simple-odata-client/Simple.OData.Client/) versus the previously custom version, as this is a far richer and complete OData implementation. This was chosen as it is lightweight and is completely decoupled from the underlying OData endpoint. 

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).

## v2.1.6
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.5
- *Fixed:* Introduced FxCop Analysis to `Beef.Data.OData`; this version represents the remediation based on the results.

## v2.1.4
- *Fixed:* Introduced FxCop Analysis to `Beef.Core`; this version represents the remediation based on the results.

## v2.1.3
- *Fixed:* InvokerBase was non functioning as a generic class; reimplemented. Other Invokers updated accordingly.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.