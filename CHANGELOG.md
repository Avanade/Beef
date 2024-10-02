# Change log

Represents the **NuGet** versions.

## v5.16.0
- *Enhancement:* Database code-generation defaults to the use of [JSON](https://learn.microsoft.com/en-us/sql/relational-databases/json/json-data-sql-server)-serialized parameters versus UDT/TVP to minimize the need for additional database objects; specifically [User-Defined Types](https://learn.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql) (UDT).
  - A new `CollectionType` property has been added to the code-generation configuration for query-based collection passing; supports `JSON` (default) and `UDT` (previous) values. Note that for JSON passing a `NVARCHAR(MAX)` type should be used.
  - A merge for a sub-collection would previously require a `UDT` to be created; this is now handled using JSON serialization, unless explicitly required (example YAML `udt: true, tvp: WorkHistory`).
- *Enhancement:* The out-of-the-box _Beef_ `Type` schema objects (`udtBigIntList.sql`, `udtDateTimeList.sql`, `udtIntList.sql`, `udtNVarCharList.sql` and `udtUniqueIdentifierList.sql`) have been removed and will not be automatically included. Where required, these should be manually added to the database project and managed accordingly; use this [`create-beef-user-defined-types.sql`](./tools/Beef.Database.SqlServer/Migrations/create-beef-user-defined-types.sql) migration script to add.
- *Enhancement:* All code-generated SQL objects have been updated to support replacement, for example `CREATE OR ALTER` versus previous `CREATE`; this potentially minimizes the need to drop and recreate each migration. This _DbEx_ behavior is predicated on not having any `Type` (UDT) schema objects which do not support replacement. It is also therefore recommended that all non-generated schema objects support replacement as they all have to adhere to this pattern to avoid unncessary dropping.

## v5.15.2
- *Fixed:* Fixed the event value publish code-generation by enabling an override using `Operation.EventValue` where applicable (i.e. no response).

## v5.15.1
- *Fixed:* Added option `AcceptsBody` to `Parameter.WebApiFrom` to ensure consistent behaviour with `Create` and `Update` operation types in terms of how a body value is handled within the API Controller.
- *Fixed:* Fixed the `value` parameter inference where operation type is `Custom` and the `ValueType` is specified; ensures operation is generated correctly.
- *Fixed:* Fixed the Agent code-generation to enable optional operation parameters where applicable.

## v5.15.0
- *Enhancement:* Added `Operation.Query` boolean to enable support for OData-like query syntax. This leverages the underlying `CoreEx.Data.Querying` (`v3.25.1+`) capabilities to enable. The `Operation.Behavior` has also been extended to support a '`Q`'uery as a shorthand to enable a query-based operation. _Note:_ this is an **awesome** new capability.
- *Enhancement:* Updated the `DatabaseName`, `EntityFrameworkName`, `CosmosName`, `ODataName` and `HttpAgentName` to support both `Type` (existing) and optional `Name` (new). This uses the `Type^Name` syntax supported for other properties with similar purpose. The properties have also had the `Name` suffix renamed to `Type` as this more accurately reflects the property intent (existing names will continue to work with a corresponding warning during code-generation).

## v5.14.2
- *Fixed:* Fixed the data model code-generation to output the `PartitionKey` where specified.
- *Fixed:* Fixed the code-generated `PartitionKey` to be a nullable string. 
- *Fixed:* Fixed the templated `CosmosDb` to ensure lazy-loading of container (versus re-creating on each access). 

## v5.14.1
- *Fixed:* Fixed the manager code-generation to reference parameter `value` as `v`, and output `ThenAsAsync`, correctly. 
- *Fixed:* Fixed the manager code-generation to use the new `Result<T>.Adjusts` to avoid unintended compiler identified casting when using `Then`.
- *Fixed:* Fixed the manager, data service and data code-generation to not output a constructor where there are no constructor parameters; i.e. is not required.
- *Fixed:* Added `ManagerCtorCustom`, `DataSvcCtorCustom` and `DataCtorCustom` to allow the constructor to be implemented as custom, non-generated, by the consuming developer.

## v5.14.0
- *Enhancement:* `Operation.DataSvcCustom` changed from boolean to an option that indicates the level of `DataSvc` customization (invokes `*OnImplementationAsync` method) vs code-generation (automatically invokes data-layer). 
  - Valid values are:
    - `Full` indicates the logic is fully customized (only invocation is code-generated). 
    - `Partial` indicates combination of surrounding code-generation with final custom invocation versus data-layer. 
    - `None` indicates data-layer invocation with _no_ custom invocation (default).
  - Existing configurations of `dataSvcCustom: true` should be changed to `dataSvcCustom: Full` to achieve same behavior. Where not changed a code-generation runtime error will occur.
- *Enhancement:* Added support for `Property`-based `ICacheKey` support using `cacheKey: true` resulting in corresponding entity code-generation.
- *Fixed:* Model code-generation corrected to explicitly output `Newtonsoft.Json.JsonIgnore`.
- *Fixed:* Entity and model templates updated to correctly generate the `PrimaryKey` where the property is reference data.
- *Fixed:* Entity collection code-generation for Dictionary updated to include capabilities to add items using primary key where specified.
- *Fixed:* Upgraded dependencies.

## v5.13.0
- *Enhancement:* Added `dotnet run openapi` option to perform *basic* parsing of an [OpenAPI](https://spec.openapis.org/oas/latest.html) document generating the corresponding `Entity`, `Operation` and `Property` configuration into a temporary YAML file. The contents are expected to then be copied and pasted into the appropriate YAML destination and further configured as necessary.
  - Execute `dotnet run -- --help` to see all command-line capabilities for this.
  - All OpenAPI paths (operations) are generated into a single `Service` entity; this can be further split into multiple entities manually as required.
  - Where a schema (entity) is specified more than once with the same name it will attempt to match the configuration (including properties) to an existing entity and reuse; otherwise, a new entity will be created with a postfix number to ensure uniqueness.
  - The `OpenApiArgs` supports further options to enable additional customization of processing and generated output; this is set using the `CodeGenConsole.WithOpenApiArgs` method. 
  - _Note:_ this is in _preview_ until explicitly noted in a later version; as such, the generated YAML will require manual review and adjustment as required and may not support all features (for some time or ever). This is only intended to help accelerate the initial configuration process where an OpenAPI document is available.
- *Fixed*: Sanitize the generated `Api.Controller` and `Common` summary text comments to remove internal references, etc.

## v5.12.9
- *Fixed:* Enable `text` specification to be used as-is by prefixing with a `+` plus-sign character. 
- *Fixed:* Upgraded `DbEx` (`v2.5.8`) to include all related fixes and improvements.

## v5.12.8
- *Fixed:* Fixed the model code-generation by allowing the `ModelInherits` to be specified within the `Entity` YAML configuration to override the default.
- *Fixed:* Fixed `dotnet run count` to exclude paths that start with `.` (dot) to avoid including hidden files in the count.

## v5.12.7
- *Fixed:* Fixes the model code-generation to auto implement the `ITenantId` and `ILogicallyDeleted` where corresponding properties are defined.
- *Fixed:* Fixed the manager code-generation to output the `IdentifierGenerator` code where inheriting the `Id` property.

## v5.12.6
- *Fixed:* The EF Model generation has had the `ITenantId.TenantId` filtering removed as out-of-the-box EF caches first and uses resulting in an unexpected side-effect. The `CoreEx.EntityFramework` as of `v3.20.0` automatically includes tenant filtering to achieve the desired behavior.
- *Fixed:* Upgraded `CoreEx` (`v3.20.0`) to include all related fixes and improvements.

## v5.12.5
- *[Issue 243](https://github.com/Avanade/Beef/issues/243):* Fixed `[HttpGet("persons/{id}")]` to `[HttpGet("persons/{id}, Name=Entity-Name_Operation-Name)")]` which sets the `OperationId` within the OpenAPI output.
- *[Issue 244](https://github.com/Avanade/Beef/issues/244):* Fixed `Operation.HttpAgentRoute` where specified as a query string `?foo=bar`; was being generated as `prefix/?foo=bar` versus `prefix?foo=bar`.
- *Fixed:* Upgraded `CoreEx` (`v3.18.1`) to include all related fixes and improvements.

## v5.12.4
- *Fixed:* Fixes to the `Template` solution to improve the initial `dotnet new beef` experience, including sample Reference Data API tests.
- *Fixed:* Upgraded `CoreEx` (`v3.18.0`) to include all related fixes and improvements.
- *Fixed:* The `EntityManager_cs.hbs` template has been updated to account for the `CoreEx.Validation.ValueValidator<T>` fixes.

## v5.12.3
- *Fixed:* Fixes to the `Template` solution to improve the initial `dotnet new beef` experience with respect to API health endpoint.

## v5.12.2
- *Fixed:* Fixes to the `Template` solution to improve the initial `dotnet new beef` experience.

## v5.12.1
- *Fixed:* Upgraded `CoreEx` (`v3.15.0`) to include all related fixes and improvements.
- *Fixed:* The API Agent templates have been updated to account for the changes to the `TypedHttpClientBase` constructor signature in `CoreEx` (`v3.15.0`).

## v5.12.0
- *Enhancement:* Added `WebApiTags` code-generation property to enable the specification of `Tags` for the Web API Controller class.
- *Enhancement:* Added `dotnet run endpoints` option to report all configured endpoints providing a means to audit the generated API surface.
- *Enhancement:* Secondary (additional) configuration files `*.entity.beef-5.yaml`, `*.refdata.beef-5.yaml`, `*.datamodel.beef-5.yaml` can be added to the project (including within subfolders) that will be automatically merged into the corresponding primary `entity.beef-5.yaml`, `refdata.beef-5.yaml`, `datamodel.beef-5.yaml` files respectively. The secondary files only support a single root `entities` property/node that merges into the primary's equivalent. This allows the configuration to be broken up logically to minimize challenges related to overall file size and complexity, and minimize potential developer merge conflicts, etc.
- *Fixed:* The `Operation.ReturnType` was incorrectly determining and overridding nullability (`Operation.ReturnTypeNullability`) which has been corrected.

## v5.11.0
- *Enhancement:* Added `dotnet new beef ... --services AzFunction` to enable the templating of a corresponding `Company.AppName.Services` project as an Azure Functions project. This will provide an example of leveraging the shared `Company.AppName.Business` logic and consuming the published events using an `EventSubscriberOrchestrator`.
- *Enhancement:* The `DatabaseMapper` (stored procedures) code-generation logic has been updated to leverage the new extended `DatabaseMapperEx`. This avoids the existing reflection and expression compilation, using explicit code to perform the mapping. Can offer up to 40%+ improvement in some scenarios. Where existing behavior is required then set YAML `databaseMapperEx: false` in the `entity.beef-5.yaml` file (root and/or entity within hierarchy).

## v5.10.0
- *Enhancement:* Added [PostgreSQL](https://www.postgresql.org/) database support: 
  - Leverages both `CoreEx.Database.Postgres` (runtime) and `DbEx.Postgres` (migration) packages; encapsulates the `Npgsql` package.
  - The `Npgsql.EntityFrameworkCore.PostgreSQL` package is used for the entity framework provider.
  - The `dotnet new beef` template updated to support new `datasource` option of `postgres`.
- *Enhancement:* Additional improvements for data migration and code-generation, including `SqlServer` and `MySql`, as a result of `DbEx` (`v2.5.0`) enhancements.

## v5.9.1
- *Fixed:* Simplified YAML specification where _only_ a custom manager is required to be implemented. For an `operation` set `type: CustomManagerOnly`, this is a shorthand for `type: Custom, managerCustom: true, excludeIDataSvc: true, excludeDataSvc: true, excludeIData: true, excludeData: true` (i.e. these other properties will no longer need to be set explicitly).

## v5.9.0
- *Fixed:* Upgraded `CoreEx` ([`v3.9.0`](https://github.com/Avanade/CoreEx/blob/main/CHANGELOG.md#v390)) and `DbEx` ([`v2.4.0`](https://github.com/Avanade/DbEx/blob/main/CHANGELOG.md#v240)) to include all related fixes and improvements; including dependent `UnitTestEx` ([`v4.0.1`](https://github.com/Avanade/UnitTestEx/blob/main/CHANGELOG.md#v410)) and related `NUnit` ([`v4.0.1`](https://docs.nunit.org/articles/nunit/release-notes/Nunit4.0-MigrationGuide.html)) upgrades.
- *Enhancement:* Updated the `dotnet new beef` template to target `net8.0` and updated `NUnit`.
  - _Note:_ `MySQL` includes dependency `Pomelo.EntityFrameworkCore.MySql` (`8.0.0-beta.2`) that enables `net8.0`; this will be updated to the official _release_ version when [ready](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/issues/1746).   

## v5.8.0
- *Fixed:* Upgraded `CoreEx` (`v3.8.0`) to include all related fixes and improvements; `WebApi` class supports returning value of `IActionResult` as-is.
- *Enhancement:* Updated code-generation to support `IActionResult` return type for `WebApi` operations. The following `operation` YAML properties enable:
  - New `webApiProduces: [ 'text/plain' ]` where the value is a `Produces` content type array (supports multiple) to override the default; for example `[Produces("text/plain")]`.
  - New `webApiProducesResponseType: none` indicates that the resulting generated code does _not_ include the response type; for example `[ProducesResponseType((int)HttpStatusCode.OK)]`.

## v5.7.4
- *Fixed:*  Upgraded `CoreEx` (`v3.7.2`) to include all related fixes and improvements; updated reference data code-generation template and samples as a result.

## v5.7.3
- *Fixed:* The `clean` code-generation command supports new path exclusion capabilities; see `dotnet run -- --help` for details.
- *Fixed:* The `count` code-generation command has been added to report the total number of files and lines for all and generated code.

## v5.7.2
- *Fixed:* The `Entity.HttpAgentCustomMapper` property has been added to the schema to correctly include within code-generation.
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
- Represents the initial commit for _Beef_ version 5.x. All assemblies/packages now share the same version and change log; i.e. they are now published as a set versus individually versioned (prior releases). This version is a _major_ refactoring from the prior; to achieve largely the same outcomes, in a modernized decoupled manner.
- As stated in the [README](./README.md), _Beef_ is _now_ (as of version 5.x) ostensibly the code-generation engine, and solution orchestration, that ultimately takes dependencies on the following capabilities to enable the end-to-functionality and testing thereof in a standardized (albeit somewhat opinionated) manner:
  - [CoreEx](https://github.com/Avanade/CoreEx) - provides the core runtime capabilities  (extends .NET core);
  - [UnitTestEx](https://github.com/Avanade/UnitTestEx) - provides extended unit and intra-domain integration testing;
  - [DbEx](https://github.com/Avanade/DbEx) - provides extended database management capabilities ;
  - [OnRamp](https://github.com/Avanade/OnRamp) - provides the underlying code-generation engine functionality.
- Prior to version 5.x, _Beef_ was all encompassing. These capabilities have been extracted, simplified and refactored to be first class frameworks in their own right, and made into the repos listed above. This allows them to be used and maintained independently to _Beef_; therefore, offering greater opportunities for reuse versus all-or-nothing.