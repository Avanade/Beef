# Change log

Represents the **NuGet** versions.

## v5.7.3
- *Fixed:* The `clean` code-generation command supports new path exclusion capabilities; see `dotnet run -- --help` for details.
- *Fixed:* The `count` code-generation command has been added to report the total number of files and lines for all and generated code.

## v5.7.2
- *Fixed:* The `Entity.HttpAgentCustomMapper` property has been added to the schema for correctly include within code-generation.
- *Fixed:* Upgraded `CoreEx` (`v3.7.0`) to include all related fixes and improvements.

## v5.7.1
- *Fixed:* All related package dependencies updated to latest.

## v5.7.0
- *Enhancement:* `UnitTestEx` as of `v4.0.0` removed all dependencies to `CoreEx`; the equivalent functionality has been included in new `CoreEx.UnitTest` and `CoreEx.UnitTest.NUnit` (`v3.6.0`) packages. Replace existing `UnitTestEx.NUnit` package with the corresponding `CoreEx.UnitTesting.NUnit` package.
  - Unfortunately, this change has resulted in a number of breaking changes that will require manual remediation to existing unit tests; see the `UnitTestEx` [change log](https://github.com/Avanade/UnitTestEx/blob/main/CHANGELOG.md#v400) and `CoreEx` [change log](https://github.com/Avanade/CoreEx/blob/main/CHANGELOG.md#v360) for details and instructions to remediate.
  - Upgraded `CoreEx` (`v3.6.0`) to include all related fixes and improvements.
  - Upgraded `UnitTestEx` (`v4.0.0`) to include all related fixes and improvements 
- *Enhancement:* Added `net7.0` and `net8.0` support.

## v5.6.9
- *Fixed:* The `yaml` sub-command generated code updated to leverage the new `Entity.Behavior`, being `behavior: cgupd`, where CRUD capabilities are requested.

## v5.6.8
- *Fixed:* `Entity.Crud` deprecated and replaced with new `Entity.Behavior` to enable improved flexibility in specifying which CRUD-style operation(s) are to be implemented: 'C'reate, 'G'et (or 'R'ead), 'U'pdate, 'P'atch and 'D'elete, GetByArgs ('B') and GetAll ('A'). Any combination (or order) can be provided to auto-generate versus specifiying individual properties (which can still be used if applicable). Where `crud: true` was previously specified, this should be replaced with `behavior: cgupd` (note that code-generation will error where not updated).
- *Fixed:* `Entity.WebApiRoutePrefix` no longer applied to the `Controller` class using the `Route` attribute; now always included as the full path specified within the HTTP method attribute per operation. The `Operation.WebApiRoute` can now be prefixed with a `!` character to indicate that the `Entity.WebApiRoutePrefix` should not be applied; i.e. the supplied value is leveraged as-is. This enables more scenarios for the `Entity.Behavior` above to be used, as often it was the route value that necessitated full operation specification within YAML.
- *Fixed:* Related `httpAgentRoute` configuration and resulting code-generation implemented similar to above for consistency.
- *Fixed:* Upgraded `CoreEx` (`v3.4.1`) and `DbEx` (`v2.3.12`) to include all related fixes and improvements; update `DataSvc` template to leverage new (simplified) `Result`-based caching capabilities.

## v5.6.7
- *Fixed:* Upgraded `CoreEx` (`v3.4.0`) to include all related fixes and improvements; updated template and samples as a result. 

## v5.6.6
- *Fixed:* The `Operation.Type` entity code-generation attribute explicitly defaults to `Custom` where not specified.
- *Fixed:* Reference data entity code-generation adds `NotNullIfNotNullAtrribute` to correct null compiler warnings. The `System.Diagnostics.CodeAnalysis` namespace may need to be added to the `GlobalUsings` to enable.
- *Fixed:* Improved the default `Parameter.Text` and enabled `Parameter.WebApiText` to override explicity where required.

## v5.6.5
- *Fixed:* Entity was not correctly generated where default value was specified to ensure correct `IsInitial` logic.
- *Fixed:* Entity was not correctly generated where `RefDataList` was specified; code-gen output improved.

## v5.6.4
- *Fixed:* Enhanced the default `Entity.RefDataType` behaviour; will default to root `CodeGeneration.RefDataType` (new), where specified to simplify configuration. 
- *Fixed:* Enhanced the default `Entity.Collection` behaviour; will default to `true,` where `RefDataType` is specified as this is required.
- *Fixed:* Enhanced the default `Entity.AutoImplement` behaviour; will default to root `CodeGeneration.AutoImplement` (new), where specified to simplify configuration.
- *Fixed:* Enhanced the default `Entity.WebApiRoutePrefix` behaviour; will default to `Entity.Name` pluralized and lowercase, where `RefDataType` is specified as this is required.

## v5.6.3
- *Fixed:* Enhanced the default `Entity.Text` and `Property.Text` code-generation attributes where not specified to be more meaningful/descriptive.

## v5.6.2
- *Fixed:* Added new `Entity.Crud` code-generation attribute which represents a shorthand for `Create`, `Get` (read), `Update` (includes `Patch`) and `Delete`.

## v5.6.1
- *Fixed:* The previous `excludeData: RequiresMapper` fix inadventently resulted in errant mapper code for the reference data code-generation which has been corrected.

## v5.6.0
- *Enhancement:* The database code-generation (`Beef.Database.*`) now supports a `yaml` sub-command that will generate the basic _Beef_ Entity YAML, and basic validation logic, by inferring the database configuration for the specified tables into a temporary `temp.entity.beef-5.yaml` file. The developer is then responsible for the copy and paste of the required yaml and .NET code, into their respective artefacts and further amending as appropriate. After use, the developer should remove the `temp.entity.beef-5.yaml` file as it is otherwise not referenced/used. This enhancement by no means endorses the direct mapping between entity and database model as the developer is still encouraged to reshape the entity to take advantage of object-orientation and resulting JSON capabilities. _Beef_ still enforces a separation between entity and model even where a one-to-one match. Finally, use `dotnet run -- --help` to see all command-line options/capabilities for this.
- *Fixed:* The `excludeData: RequiresMapper` no longer needs to be explicitly set as this is now inferred from the usage of an `autoImplement` for the `entity` YAML configuration.

## v5.5.1
- *Fixed:* Updated `DbEx` (`v2.3.8`) and `OnRamp` (`v1.0.8`).
- *Fixed:* Updated `UnitTestEx` (`3.1.0`):
  - The `ApiTester.SetUp.ExpectedEventsEnabled = true` should be replaced with `ApiTester.UseExpectedEvents()` where an event error occurs during test execution.
  - Value-based test expectations are updated to support explicit JSON paths, as a result some new errors may be reported (previously ignored in error) - note that JSON paths for arrays should not include the index.

## v5.5.0
- *Fixed:* Upgraded `CoreEx` to `v3.3.0` to include all related fixes and improvements related to distributed tracing. 
- *Fixed:* Code-generation templates updated to support any breaking changes related to `InvokerBase`.

## v5.4.0
- *Enhancement:* Upgraded `CoreEx` to `v3.0.0` which resulted in a number of breaking changed that will require manual remediation:
  - The `CoreEx.WebApis` capabilities have been moved to the new `CoreEx.AspNetCore.WebApis` namespace; within a new seperate project/package `CoreEx.AspNetCore`. This package will need to be referenced where required, and the namespace updated accordingly.
- *Enhancement:* Leverages the new `Result` and `Result<T>` railway-oriented programming capabilities introduced in `v3` to minimize the number of thrown exceptions (of the non-exceptional variety), to improve performance and, the development and debugging experience.
  - A significant number of the underlying code-generation templates have been modified to support `v3` changes/fixes and general improvements; as such all code-generation should be re-run. Note that by default the generated code will leverage `Result` and `Result<T>`; for backwards compatibility set `withResult: false` within the `entity.beef-5.yaml`.
  - Also, note that `v3` may introduce some breaking changes that will need manual remediation. The validation `OnValidateAsync` and  `CustomRule` _must_ now return a `Result`. Where using, consider leveraging the `Result.XxxError()` methods to return known errors versus throwing the related exception.
- *Enhancement:* Code-generation API log output will state `<NoAuth>` explicitly where no authorization has been specified using the corresponding `webApiAuthorize` YAML configuration.
- *Enhancement:* Where leveraging _MySQL_ the template now uses the [Pomelo.EntityFrameworkCore.MySql](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql) package; this has greater uptake and community supporting than the equivalent Oracle-enabled [MySql.Data.EntityFrameworkCore](https://www.nuget.org/packages/MySql.Data.EntityFrameworkCore) package.
- *Enhancement:* The [preprocessor directives](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives) are no longer generated by default for the C# artefacts, to re-include use the `preprocessorDirectives` YAML configuration. This relates to the existing `#nullable enable` and `#pragma warning disable` preprocessor directives. These will be deprecated in a future release.

## v5.3.1
- *Fixed:* Upgraded `CoreEx` and `UnitTestEx` to latest packages to include all related fixes.

## v5.3.0
- *Enhancement:* Added new code-generation configuration property `ValidationFramework` that supports either `CoreEx` (default) or `FluentValidation` (uses the `CoreEx.FluentValidation` interop wrapping capabilities) to allow entity validation to be performed using either framework. Supports mix-and-matching where required. The `CoreEx.Validation` framework is still leveraged for `IsMandatory` and `ValidatorCode` logic where specified.
- *[Issue 208](https://github.com/Avanade/Beef/issues/208):* `ReferenceDataController.GetNamed` now excluded from Swagger output as results in superfluous types/models.
- *[Issue 209](https://github.com/Avanade/Beef/issues/209):* New `PagingAttribute` added to the `Controller` code-gen where `PagingArgs` is selected to output `PagingArgs` parameters to corresponding Swagger output. Requires `PagingOperationFilter` to be added at start up to function.
- *Fixed:* Upgraded `CoreEx`, `DbEx` and `UnitTestEx` to latest packages to include all related fixes.

## v5.2.1
- *Fixed:* Upgraded `DbEx` to `v2.3.4`; this included fix to assembly management/probing that required minor internal change to enable correctly within `SqlServerMigration` and `MySqlMigration`.

## v5.2.0
- *Enhancement:* The Manager-layer `Clean.CleanUp` is now only performed where explicitly configured; within `CodeGeneration`, `Entity`(s) and/or `Operation`(s) YAML. Cleaning is a feature that is generally infrequently used and is best excluded unless needed.
- *Enhancement:* Code-generation console logging updated to output the generated endpoints to provide an audit of the generated API surface.
- *Fixed:* The `CoreEx.Mapping.Mapper` within `CoreEx v2.6.0` resolved initializing nullable destination properties during a `Flatten`; however, this needed additional mapping configuration generated to enable. The generated mapping code was updated to enable, whilst also simplifing generated output, further enabling opportunities to override methods where required.
- *Fixed:* Entity code generation output updated to assign correct property name (`INotifyPropertyChanged`) for all reference data properties.

## v5.1.2
- *Fixed:* The `CodeGenConfig.WarnWhereDeprecated` was checking some incorrectly cased property names.
- *Fixed:* The `AgentTester<TEntryPoint>` has been updated to allow a parameterless `CreateWaf`, as well as exposing the internal `Parent` property. 

## v5.1.1
- *Fixed:* Upgraded `CoreEx`, `DbEx` and `UnitTestEx` to latest packages to include all related fixes. Template solution updated to leverage `app.UseReferenceDataOrchestrator()` to specifically include.

## v5.1.0
Represents the initial commit for _Beef_ version 5.x. All assemblies/packages now share the same version and change log; i.e. they are now published as a set versus individually versioned (prior releases). This version is a _major_ refactoring from the prior; to achieve largely the same outcomes, in a modernized decoupled manner.

As stated in the [README](./README.md), _Beef_ is _now_ (as of version 5.x) ostensibly the code-generation engine, and solution orchestration, that ultimately takes dependencies on the following capabilities to enable the end-to-functionality and testing thereof in a standardized (albeit somewhat opinionated) manner:
- [CoreEx](https://github.com/Avanade/CoreEx) - provides the core runtime capabilities  (extends .NET core);
- [UnitTestEx](https://github.com/Avanade/UnitTestEx) - provides extended unit and intra-domain integration testing;
- [DbEx](https://github.com/Avanade/DbEx) - provides extended database management capabilities ;
- [OnRamp](https://github.com/Avanade/OnRamp) - provides the underlying code-generation engine functionality.

Prior to version 5.x, _Beef_ was all encompassing. These capabilities have been extracted, simplified and refactored to be first class frameworks in their own right, and made into the repos listed above. This allows them to be used and maintained independently to _Beef_; therefore, offering greater opportunities for reuse versus all-or-nothing.