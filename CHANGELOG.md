# Change log

Represents the **NuGet** versions.

## v5.4.0
- *Enhancement:* Upgraded `CoreEx` to `v3` to leverage the `Result` and `Result<T>` railway-oriented programming capabilities to minimize the number of thrown exceptions (of the non-exceptional variety), to improve performance and, the development and debugging experience.
  - A number of the underlying code-generation templates have been modified to support `v3` changes/fixes; as such all code-generation should be re-run. Note that by default the generated code will leverage `Result` and `Result<T>`; for backwards compatibility set `withResult: false` at top of `entity.beef-5.yaml`.
  - Also, note that `v3` may introduce some breaking changes that will need manual remediation. The validation `OnValidateAsync` and  `CustomRule` _must_ now return a `Result`. Where using, consider leveraging the `Result.XxxError` to return known errors versus throwing the related exception.
- *Enhancement:* Code-generation API log output will state `<NoAuth>` explicitly where no authorization has been specified using the corresponding `webApiAuthorize` YAML configuration.

## v5.3.1
- *Fixed:* Upgraded `CoreEx` and `UnitTestEx` to latest packages to include all related fixes.

## v5.3.0
- *Enhancement:* Added new code-generation configuration property `ValidationFramework` that supports either `CoreEx` (default) or `FluentValidation` (uses the `CoreEx.FluentValidation` interop wrapping capabilities) to allow entity validation to be performed using either framework. Supports mix-and-matching where required. The `CoreEx.Validation` framework is still leveraged for `IsMandatory` and `ValidatorCode` logic where specified.
- *[Issue 208](https://github.com/Avanade/Beef/issues/208): `ReferenceDataController.GetNamed` now excluded from Swagger output as results in superfluous types/models.
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