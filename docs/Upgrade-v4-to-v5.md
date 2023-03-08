# Upgrade Beef version 4.x to 5.x

_Beef_, as of version 5.x, is ostensibly the code-generation engine, and solution orchestration, that ultimately takes dependencies on the following capabilities to enable the end-to-functionality and testing thereof in a standardized (albiet somewhat opinionated) manner:
- [CoreEx](https://github.com/Avanade/CoreEx) - provides the core runtime capabilties (extends .NET core);
- [UnitTestEx](https://github.com/Avanade/UnitTestEx) - provides extended unit and intra-domain integration testing;
- [DbEx](https://github.com/Avanade/DbEx) - provides extended database management capabilties;
- [OnRamp](https://github.com/Avanade/OnRamp) - provides the underlying code-generation engine functionality.

Prior to version 5.x, _Beef_ was all encompassing. These capabilities have been extracted, simplified and refactored to be first class frameworks in their own right, and made into the repos listed above. This allows them to be used and maintained independently to _Beef_; therefore, offering greater opportunities for reuse versus all-or-nothing.

<br/>

## Breaking changes

Version 5.x is a major refactoring (improvement and simplification) with respect to the underlying runtime primarily, and although effort was made to minimize impacts on upgrading from version 4.x, this was unfortunately unavoidable.

<br/>

### YAML-only code-generation configuration

The XML-based code-generation configuration has been deprecated; the XML _will_ have to be converted to YAML **before** attempting to upgrade. The 4.2.x code-generation console supports `-x2y|--xml-to-yaml` to perform this action.

<br/>

### Autonomous entity scope

In version 4.2.x an `entityScope` property was added to the entity code-generation configuration. Where this was not explicitly defined it would have defaulted to `Common`, that indicated that the rich entities (inheriting from `EntityBase`) were to be generated in the `Common` project. Alternatively, a value of `Business` would indicate that the rich entities were to be generated in the `Business` project.

A further `Autonomous` option was available that generated two contractually-identical entities, the rich entities (inheriting from `EntityBase`) generated in the `Business` project, and corresponding basic entities (no `EntityBase` inheritance) generated in the `Common` project. This was intended to simplify usage, and remove dependencies related to the rich Reference Data, within `Common` where leveraging the `Agent` to invoke the APIs.

The `entityScope` property has been _deprecated_ in 5.x as `Autonomous` is now the **one-and-only** behavior. Additional code-generation properties such as `internalOnly` and `omitEntityBase` further drive how the entity-based artefacts are generated.

<br/>

### JSON serialization

Previously, _Beef_ leveraged _Newtonsoft_ exclusively for JSON serialization. _CoreEx_ is [JSON serializer](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/README.md) implementation agnostic; however, `System.Text.Json` is considered the preferred (default).

From a code-generation perspective a new YAML [`jsonSerializer`](./Entity-CodeGeneration-Config.md#Entity) property can be used to specify preferred; defaults to `SystemText`; otherwise, specify `Newtonsoft`.

<br/>

### Reference data

There has been a significant refactoring of the existing `ReferenceDataManager`; this functionality is now enabled by the _CoreEx_ [`ReferenceDataOrchestrator`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/ReferenceDataOrchestrator.cs). This new class now fully encapsulates the [reference data](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/README.md) access, caching, and loading. The v5.x reference data code generation has been updated accordingly.

<br/>

### Object-property mapping

Where previously leveraging the likes of Entity Framework (EF) object-property mapping between types was managed leveraging _AutoMapper_ to enable. _CoreEx_ provides a flexible implementation agnostic approach to enable [mapping](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Mapping/README.md); however, for _Beef_ the code-generation exclusively leverages the new simpler in-built [`Mapper`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Mapping/README.md#mapper-implementation) functionality.

<br/>

### Async cancellation token

All _CoreEx_ `Async` methods include a [`CancellationToken`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken) as the final parameter to support the recommended .NET asynchronous programming [pattern](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1068).

Where overridding previous `Async`-related methods these will need to be updated to include this new parameter. Generally, where invoking the method the `CancellationToken` will be optional; i.e. will default where not specified.

<br/>

### Check class

_Beef_ previously contained a `Check` class that was used to check method parameters and throw a corresponding `ArgumentNullException` or `ArgumentException`; this class does not exist in _CoreEx_. The code that previously leveraged will need to be refactored, or the developer will need to re-implement the `Check` class.

It is recommended that the developer consider using [`ArgumentNullException.ThrowIfNull`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception.throwifnull) and [`ArgumentException.ThrowIfNullOrEmpty`](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception.throwifnullorempty) (.NET 6+).

<br/>

## Package mapping

Following represents the high-level mapping between the existing _Beef_ packages and the corresponding new (where applicable). Not all packages are functionally equivalent, some capabilities may have been deprecated.

Existing | New
-|-
`Beef.Abstractions` | `CoreEx`
`Beef.AspNetCore.WebApi` | `CoreEx`
`Beef.Core` | `CoreEx`, `CoreEx.Validation`, `CoreEx.Newtonsoft`, `CoreEx.AutoMapper`
`Beef.Data.Database` | `CoreEx.Database.SqlServer` includes `CoreEx.Database`
`Beef.Data.Database.Cdc` | `CoreEx.Database.SqlServer`
`Beef.Data.EntityFrameworkCore` | `CoreEx.EntityFrameworkCore`
`Beef.Data.Cosmos` | `CoreEx.Cosmos`
`Beef.Data.OData` | None (on roadmap)
`Beef.Events` | `CoreEx` (`CoreEx.Events` namespace)
`Beef.Events.EventHubs` | None (on roadmap)
`Beef.Events.ServiceBus` | `CoreEx.Azure` (publishing only)
`Beef.Grpc` | None (consider [Dapr sidecar](https://docs.dapr.io/developing-applications/integrations/grpc-integration/))
- | -
`Beef.CodeGen.Core` | `Beef.CodeGen.Core` upgraded (leverages `OnRamp`)
`Beef.Database.Core` | `Beef.Database.SqlServer` includes `Beef.Database.Core` (leverages `DbEx`)
`Beef.Test.NUnit` | `UnitTestEx`, [`Beef.Test.NUnit`](../tools/Beef.Test.NUnit/README.md) (backwards compatibility)
`Beef.Template.Solution` | `Beef.Template.Solution`

<br/>

## Upgrade guidance approach

This documentation was developed by upgrading the `My.Hr` solution and recording the steps. This attempts to capture the how, and sometimes the why, logically project-by-project.

Section | Project
-|-
[CodeGen](#CodeGen) | [My.Hr.CodeGen](../samples/My.Hr/My.Hr.CodeGen)
[Database](#Database) | [My.Hr.Database](../samples/My.Hr/My.Hr.Database)
[Common](#Common) | [My.Hr.Common](../samples/My.Hr/My.Hr.Common)
[Business](#Business) | [My.Hr.Business](../samples/My.Hr/My.Hr.Business)
[Api](#Api) | [My.Hr.Api](../samples/My.Hr/My.Hr.Api)
[Test](#Test) | [My.Hr.Test](../samples/My.Hr/My.Hr.Test)

<br/>

## CodeGen

The code-generation capabilities continue to leverage _OnRamp_ to provide the code-generation scripting, templating and orchestration. All underlying templates have been updated where applicable to output code supporting the _Beef_ 5.x changes. Update the `Beef.CodeGen.Core` package dependency to the latest 5.x version. The underlying [`Program.cs`](../samples/My.Hr/My.Hr.CodeGen/Program.cs) logic should require no further changes and should compile successfully.

The `*.beef.yaml` configuration files _must_ be renamed to `*.beef-5.yaml`. This is to support new features and ensure the underlying schema validation (intellisense) is targeting version `5.x`.

Within _CoreEx_ the existing `IUniqueKey` interface has been replaced with the [`IPrimaryKey`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IPrimaryKey.cs), as such all references to the code-generation `uniqueKey` property must be renamed to `primaryKey`.

<br/>

### Clean

A new `Clean` command has been added to the code-generator. Where executed the code-generator will recursively discover all `Generated` folders and will delete all files within (including generated [Database](#Database) artefacts where applicable). This will ensure a clean baseline as not all previously generated artefacts will be generated in the new version.

From the command-line execute the clean command.

```
dotnet run clean
```

<br/>

### Re-generate

Re-generate all the existing artefacts leveraging the `All` command. 

```
dotnet run all
```

Error messages similar to the following may appear and will need to be corrected for the code-generation to complete successfully.

```
Config '.\My.Hr\My.Hr.CodeGen\entity.beef-5.yaml' is invalid: [Entity(Name='EmployeeBase').Property(Name='Id').uniqueKey] The 'uniqueKey' configuration has been renamed to 'primaryKey'; please update the configuration accordingly.
```

Warning messages similar to the following may appear to indicate where previous configuration has been deprecated, etc. Generally, it is advisable to correct the configuration to remove all the warnings.

```
Warning: Config [entityScope] has been deprecated and will be ignored.
Warning: Config [Entity(Name='EmployeeBase').iValidator] has been deprecated and will be ignored.
```

<br/>

## Database

The database capabilities have been extended within _DbEx_ to support multiple relational database providers, as such  `Beef.Database.SqlServer` now encapsulates the SQL Server database migration logic. As such, remove all previous `Beef.*` package dependencies and add the latest `Beef.Database.SqlServer` package version.

The underlying [`Program.cs`](../samples/My.Hr/My.Hr.Database/Program.cs) logic has been updated such that a new `ConfigureMigrationArgs` method has been exposed that the `My.Hr.Test` can invoke to minimize duplication of any [`MigrationArgs`](../tools/Beef.Database.Core/MigrationArgs.cs) configuration. 

``` csharp
public class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
    static Task<int> Main(string[] args) => SqlServerMigrationConsole
        .Create("Data Source =.; Initial Catalog = My.Hr; Integrated Security = True; TrustServerCertificate = true", "My", "Hr")
        .Configure(c => ConfigureMigrationArgs(c.Args))
        .RunAsync(args);

    /// <summary>
    /// Configure the <see cref="MigrationArgs"/>.
    /// </summary>
    /// <param name="args">The <see cref="MigrationArgs"/>.</param>
    /// <returns>The <see cref="MigrationArgs"/>.</returns>
    public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args) => args.AddAssembly<Program>().UseBeefSchema();
}
```

The `database.beef.yaml` configuration file _must_ also be renamed to `database.beef-5.yaml`. This is to support new features and ensure the underlying schema validation (intellisense) is targeting version `5.x`.

Finally, recompile and execute. There may have been some minor changes to the generated output from the previous version. 

```
dotnet run all
```

Warning messages similar to the following may appear to indicate where previous configuration has been deprecated, etc. Generally, it is advisable to correct the configuration to remove warnings.

```
Warning: Config [entityScope] has been deprecated and will be ignored.
```

<br/>

## Common

Generally remove/update the package dependencies as follows; check any other dependencies to ensure need and update version accordingly.

Remove | Instructions
-|-
`Beef.*` | Add the latest `CoreEx` package.
`Newtonsoft.Json` | By default `System.Text.Json` is used and is included within `CoreEx`; where `Newtonsoft` is still required add the `CoreEx.Newtonsoft` package.

This project should require no further changes and compile successfully. 

The generated `Common` project output has been changed as follows.

Folder | Leverages | Additional information
-|-|-
`Entities` | [`CoreEx.Entities`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Entities) | See [documentation](./Layer-Entity.md).
`Agents` | [`CoreEx.Http`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Http) | See [documentation](./Layer-ServiceAgent.md). Note that the existing `IXxxWebApiAgentArgs` and `XxxWebApiAgentArgs` have been deprecated.

<br/>

## Business

Generally remove/update the package dependencies as follows; check any other dependencies to ensure need and update version accordingly.

Remove | Instructions
-|-
`Beef.Core` | Add the latest `CoreEx` and `CoreEx.Validation` packages.
`Beef.Data.Database` | Add the latest `CoreEx.Database.SqlServer` package.
`Beef.Data.EntityFrameworkCore` | Add the latest `CoreEx.EntityFrameworkCore` package.
`Beef.*` | Other `Beef` dependencies; see [package mapping](#Package-mapping).
`Newtonsoft.Json` | By default `System.Text.Json` is used and is included within `CoreEx`; where `Newtonsoft` is still required add the `CoreEx.Newtonsoft` package (includes `Newtonsoft.Json`).

<br/>

### Namespaces

C# 10 as part of .NET 6 introduced the concept of [global and implicit usings](https://devblogs.microsoft.com/dotnet/welcome-to-csharp-10/). This feature is leveraged in the code generation, in that the namespace `using` statements are no longer output.

To leverage update the .NET project configuration as follows:

``` xml
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
```

A file similar to this [`GlobalUsings`](../samples/My.Hr/My.Hr.Business/GlobalUsings.cs) must be added to the project, with adjustments within to the `Company.AppName.` namespaces specific to the solution being upgraded.

Any custom, non-generated, classes _can_ remove duplicate `using` statements to simplify code if desired. For the most part, all existing `using` statements can be removed, and the `ImplictUsings` file updated to add any additional as required.

<br/>

### Configuration settings

The [`CoreEx.Configuration`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Configuration/README.md) namespace provides the new [`SettingsBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Configuration/SettingsBase.cs) class that provides a flexible, centralized, means to manage `IConfiguration`.

A new settings class, similar to [`HrSettings`](../samples/My.Hr/My.Hr.Business/HrSettings.cs), must be created. The underlying `SettingsBase` contains a number of pre-configured settings that are leveraged by _CoreEx_ at runtime. Any additional settings, for example connection strings, should be moved to this class where applicable.

<br/>

### Manager-layer logic

See [documentation](./Layer-Manager.md) for details on this specific layer. 

<br/>

### DataSvcs-layer

See [documentation](./Layer-DataSvc.md) for details on this specific layer.

<br/>

### Data-layer logic 

See [documentation](./Layer-Data.md) for details on this specific layer. The following are some of the key changes to the data-layer logic that may be encounted.

Existing | Change required
-|-
`XxxDb` | The existing custom database class that previously inherited from `DatabaseBase` must be updated to inherit from [`SqlServerDatabase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database.SqlServer/SqlServerDatabase.cs). The underlying implementation has changed; see sample [`HrDb`](../samples/My.Hr/My.Hr.Business/Data/HrDb.cs) as a guide.
`XxxEfDb` | Where also leveraging Entity Framework (EF) the equivalent [`HrEfDb`](../samples/My.Hr/My.Hr.Business/Data/HrEfDb.cs) and [`HrEfDbContext`](../samples/My.Hr/My.Hr.Business/Data/HrEfDbContext.cs) will need amending. The `Microsoft.EntityFrameworkCore.SqlServer` package dependency will also need to be added explicitly.
`DatabaseArgs` | Previously, this class was instantiated within the code-generated `partial` class, then passed into the corresponding customized `partial` class. This previously had limited usage and as such has been moved as a property into the owning `Database` class. Therefore, this parameter will need to be removed from the customized non-generated `partial` class.
`StoredProcedure` | The `StoredProcedure` property is no longer included within the `DatabaseArgs`, it is now the primary method (explicitly specified) to be able to perform an operation on the database; for example `_db.StoredProcedure(storedProcedureName).Xxx`.
`GetParamName` | This has been renamed to `GetParameterName`. A developer could choose to implement this as an extension method to enable, this would then minimize changes to existing code where applicable.
`SelectQueryMultiSetAsync` | This has been renamed to `SelectMultiSetAsync`. A developer could choose to implement this as an extension method to enable, this would then minimize changes to existing code where applicable.
`CreateTableValuedParameter` | This now requires the database to be passed as the first paramater; for example `CreateTableValuedParameter(_db, collection)`.

<br/>

### Validation logic

The [`CoreEx.Validation`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx.Validation) is essentially a port of the existing `Beef.Validation` implementation. There have been some minor changes/rationalizations; however, for the most part this should be largely identical in feature and underlying API.

Where inheriting from `Validator<TEntity>` the `OnValidateAsync(ValidationContext<Employee> context)` method signature has been changed to `OnValidateAsync(ValidationContext<Employee> context, CancellationToken cancellation)` and will be need to be updated accordingly.

<br/>

## API

See [documentation](./Layer-ServiceInterface.md) for details on this specific layer.

Remove `Beef.*` packages and replace with `CoreEx` package (will be automatically included given reference to `Business` project); check any other dependencies to ensure need and update version accordingly.

<br/>

### Namespaces

C# 10 as part of .NET 6 introduced the concept of [global and implicit usings](https://devblogs.microsoft.com/dotnet/welcome-to-csharp-10/). This feature is leveraged in the code generation, in that the namespace `using` statements are no longer output.

To leverage update the .NET project configuration as follows:

``` xml
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
```

A file similar to this [`GlobalUsings`](../samples/My.Hr/My.Hr.Api/GlobalUsings.cs) must be added to the project, with adjustments within to the `Company.AppName.` namespaces specific to the solution being upgraded.

<br/>

### Dependency injection

There have been significant changes related to _CoreEx_ and the requirements for Dependency Injection (DI); see [documentation](./Dependency-Injection.md) for further details.

The [event publishing/sending](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/README.md) approach has seen sigificant change within _CoreEx_ and will need to be refactored accordingly. Where leveraging a cloud native [Microsoft Azure messaging capability](https://learn.microsoft.com/en-us/azure/event-grid/compare-messaging-services) review [`CoreEx.Azure`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx.Azure) to determine whether a solution exists; if not, this could be an awesome opportunity to contribute.

See [`Startup.cs`](../samples/My.Hr/My.Hr.Api/Startup.cs) for an example implementation.

<br/>

### Application builder configuration

The underlying ASP.NET [`IApplicationBuilder`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.iapplicationbuilder) logic should largely remain unchanged.

The existing `UseWebApiExceptionHandler` no longer supports parameters; these are now loaded internally leveraging Dependency Injection; see underlying [`WebApiExceptionHandlerMiddleware`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/WebApis/WebApiExceptionHandlerMiddleware.cs). 

After the existing `UseExecutionContext`, an additional `UseReferenceDataOrchestrator` must be added for the `ReferenceDataOrchestrator` to function correctly. Example code is as follows.

``` csharp 
app.UseExecutionContext();
app.UseReferenceDataOrchestrator();
```

<br/>

### Program host

Within the existing `Program` class there was a reference to an existing _Beef_ `WebApiStartup` class; this has been deprecated. During the port to _CoreEx_ it was decided that the set up should be explicitly managed by the developer, and that _Beef_ should not impose any particular approach, etc. including the usage of embedded configuration files. 

The embedded resource usage was needed to support the existing unit testing capabilities; this is no longer required as _UnitTestEx_ has improved functionality to leverage the API as-is without any specific constraints. It is recommended that the developer review the existing embedded configuration file usage and amend/remove where applicable.

See [`Program.cs`](../samples/My.Hr/My.Hr.Api/Program.cs) for an example implementation.

<br/>

## Test

The _Beef v5.x_ testing has **now** been replaced by the functionality available within [UnitTestEx](https://github.com/Avanade/unittestex). Any functionality available within [`Beef.Test.NUnit`](../tools/Beef.Test.NUnit/README.md) is intended to assist with the upgrading from _Beef v4.x_; contains a subset of the previous functionality. This assembly will likely be deprecated at the next major version.

To use update the `Beef.Test.NUnit` package dependency to the latest 5.x version.

<br/>

### Namespaces

C# 10 as part of .NET 6 introduced the concept of [global and implicit usings](https://devblogs.microsoft.com/dotnet/welcome-to-csharp-10/). To leverage update the .NET project configuration as follows:

``` xml
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
```

A file similar to this [`GlobalUsings`](../samples/My.Hr/My.Hr.Test/GlobalUsings.cs) must be added to the project, with adjustments within to the `Company.AppName.` namespaces specific to the solution being upgraded.

_Note_: The `Entities` namespaces should _not_ be included with the `GlobalUsings`; these should be specified per file otherwise an ambiguious name reference will occur.

Any custom, non-generated, classes _can_ remove duplicate `using` statements to simplify code if desired. For the most part, all existing `using` statements can be removed, and the `ImplictUsings` file updated to add any additional as required.

<br/>

### Fixture one-time set-up

The existing `FixtureSetUp.OneTimeSetUp` approach has been updated as a result of `UnitTestEx`. See [`FixtureSetUp.cs`](../samples/My.Hr/My.Hr.Test/Apis/FixtureSetUp.cs) for an example implementation.

<br/>

### API testing

The existing `TestSetUpAttribute` and `AgentTester` have been deprecated during the port to _UnitTestEx_; however, basic proxies have been created for these within the `Beef.Test.NUnit` package. To enable add `using Beef.Test.NUnit`; or, alternatively update to test exclusively using _UnitTestEx_.

See following examples:
- [`PersonTest.cs`](../samples/Demo/Beef.Demo.Test/PersonTest.cs) for `Beef.Test.NUnit` implementation.
- [`EmployeeTest.cs`](../samples/My.Hr/My.Hr.Test/Apis/EmployeeTest.cs) for _UnitTestEx_ exclusive implementation.

Other challenges may include:
- Make sure the `using Company.AppName.Common.Entities;` is declared; otherwise, the entity will not match that returned by the executing _agent_ and an obscure compile error will occur. Note that the common entities do not contain any transformation logic within, so if this was previously assumed then this will need to be manually accounted for; i.e. `DateTimeTransform.DateOnly`.
- The existing `CollectionResult.Result` has been renamed `CollectionResult.Items`; as such any references to the preivous `Result` property will need to be updated to `Items`.
- The `ExpectUniqueKey` method has been deprecated; replace with either `ExpectIdentifier` or `ExpectPrimaryKey` depending on underlying entity implementation.
- The `WebApiRequestOptions` class has been deprecated; replace with `HttpRequestOptions`. The `IncludeRefDataText` property has been renamed to `IncludeText`.
- The `WebApiPatchOption` class has been deprecated; replace with `HttpPatchOption`.
- The `TestSetUp.ConcurrencyErrorETag` has been deprecated; replace with `TestSetUp.Default.ConcurrencyErrorETag`.
- The `ExpectEvent` contract has changed, with `ExpectEventValue`, `ExpectDestinationEvent` and `ExpectDestinationEventValue` also added for more advanced scenarios.

<br/>

### Reference data API testing

For tests that return `IReferenceData` results the _reference data_-specifc serializer (see [`ReferenceDataContentJsonSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Text/Json/ReferenceDataContentJsonSerializer.cs)) must be used to effectively deserialize. This must be set before the test executes.

See [`ReferenceDataTest.cs`](../samples/My.Hr/My.Hr.Test/Apis/ReferenceDataTest.cs) for an example implementation.

Where using the `Beef.Test.NUnit` package see follows.

``` csharp
// Existing
using var agentTester = AgentTester.CreateWaf<Startup>();

// New
using var agentTester = AgentTester.CreateWaf<Startup>(configureTester: t => t.UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
```

<br/>

### Validation testing

The existing `ValidationTester.Test` with corresponding `CreateAndRunAsync` pattern has been deprecated within _UnitTestEx_. The `ValidationTester` must be explicitly created using the `Create` method and finally disposed as it implements `IDisposable`.

``` csharp
// Existing
await ValidationTester.Test()
    .Xxx()
    .CreateAndRunAsync<IValidator<Xxx>, Xxx>(value);

// New
using var test = ValidationTester.Create();

await test.Xxx()
    .RunAsync<IValidator<Xxx>, Xxx>(value);
```

Other challenges may include:
- Where `ConfigureServices` is leveraged this now only supports a parameter with `Action<IServiceCollection>`, and as such the invoking code will need to be updated accordingly.
- The `AddGeneratedValidationServices()` for the `IServiceCollection` has been deprecated; the validators are now added using the new `AddValidators<TAssembly>()`. The `AddValidationTextProvider()` will also need to be added to ensure the validation message text provider is registered.
- The `AddJsonSerializer` and `AddReferenceDataOrchestrator` for the `IServiceCollection` will be required to enable JSON serialization and underlying reference data orchestration.
- The `ValidationTester.Messages` method has been deprecated and replaced with `ValidationTester.Errors` which is functionally equivalent.
- The `ValidationTester.AddScopedService` method has been deprecated and replaced with `ValidationTester.MockScoped` which is functionally equivalent.

See [`EmployeeValidatorTest.cs`](../samples/My.Hr/My.Hr.Test/Validators/EmployeeValidatorTest.cs) for an example implementation.