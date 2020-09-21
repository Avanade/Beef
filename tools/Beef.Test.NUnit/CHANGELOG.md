# Change log

Represents the **NuGet** versions.

## v4.1.2
- *Enhancement:* Added `ValidationTester` to enable simpler unit-tests (with services mocking) of validators external to the API. Provides a simpler and more test run-time performant means to validate versus having to create all the required test data within the underlying data source. 
- *Enhancement* Added `ReplaceSingleton`, `ReplaceScoped` and `ReplaceTransient` extension methods to `IServiceCollection` which will replace if existing; otherwise, add.
- *Enhancement:* Moved all subscriber host arguments to `EventSubscriberHostArgs` to centralize and enable simple configuration via DI.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.9
- *Fixed:* The `EventSubscriberTester` was not outputting all messages to `Out`; was using `Progress` which generated confusing log output.
- *Enhancement* Deprecated/obsoleted `AgentTester.StartupTestServer` and replaced with `AgentTester.TestServerStart` to align the configuration probing with `Beef.AspNetCore.WepApi.WebApiStartup.ConfigurationBuilder` for greater consistency.

## v3.1.8
- *Enhancement:* Added an overload to the `AgentTester.StartupTestServer` to `addUserSecrets`. This will enable: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets.

## v3.1.7
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.6
- *Enhancement:* The `TestEventSubscriberHost` updated to reuse the `ExecutionContext.Current` for the test.
- *Enhancement:* Added additional capability around invalid event data (`SubscriberStatus.InvalidEventData`) conversion and auditing/logging.
- *Enhancement:* Tidied up the test logging code to use `TestContext.Out.WriteLine`.
- *Fixed:* The `EventSubscriberHost` was not correctly validating the `Result.Status` which has been corrected.

## v3.1.5
- *Added:* `TestEventSubscriberHost` to suppoer the intra-domain based testing of event subscribers.
- *Enhancement:* Added support for the use of wildcards where specifying the user name for the `ExpectChangeLogCreated` and `ExpectChangeLogCreated` methods.

## v3.1.4
- *Enhancement:* Added `GrpcAgentTester` to give similar experience to `AgentTester` for invoking the gRPC service agents for intra-domain testing.

## v3.1.3
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.2
- *Enhancement:* Added request and response `ContentType` to the logging to aid debugging.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Core 3.1.

## v2.1.20
- *Added:* `AgentTester.DefaultExpectNoEvents` added to effectively default `ExpectNoEvent` forcing tests to explicitly define all expected events.

## v2.1.19
- *Enhancement:* FxCop version upgrade; new NuGet package released in error as there was no change to runtime funcationality.

## v2.1.18
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.17
- *Fixed:* The `AgentTester.ExpectEvent` will now validate the event value where specified. Also, all expected events will now need to be specified, in the order that they are raised.

## v2.1.16
- *Added:* The `AgentTester` has been updated to reflect the `IReferenceDataProvider` changes.

## v2.1.15
- *Added:* `ExpectEvent`, `ExpectNoEvent` and `ExpectNoEvents` added to `AgentTester`. This is enabled via the new `ExpectEvent` supporting class.

## v2.1.14
- *Added:* `TestSetUp.CreateLogger` enables creation of an `ILogger` that writes directly to the console.

## v2.1.13
- *Added:* `TestSetUp.RegisterSetUp` has had a new overload that supports an asynchronous function.

## v2.1.12
- *Added:* `ExpectException.Throws` has had a new overload added that supports an async method (i.e. returns `Task`).
- *Enhancement:* `ExpectException.Throws` message argument where set to '*' will accept any message text; i.e. just validates the `Type` of `Exception`. 

## v2.1.11
- *Fixed:* Removed any explicit Cosmos logic/dependencies into either `Beef.Core` (YAML) or `Beef.Data.Cosmos`. These should be referenced as required.
- *Added:* `ReturnsWebApiAgentResultAsync` extensions methods (for mocking via MOQ) added to support `XxxServiceAgent` mocking scenarios.

## v2.1.10
- *Fixed:* `Factory.ResetLocal` added to the internal finally (try-catch) for a `TestSetUpAttribute`. This will ensure configuration does not cross invocations; appears to be an edgecase for parallel testing.

## v2.1.9
- *Fixed:* Given the `AsyncLocal` nature of the _Factory Local_ implementation there should be no need to perform a `Factory.ResetLocal` - the invocations have been removed.

## v2.1.8
- *Fixed:* `Factory.ResetLocal` was incorrectly being called for each `AgentTester` tester invocation; accidently resetting previously set mock objects for the test.

## v2.1.7
- *Fixed:* Renamed the `Run` methods to `Throws` within the `ExpectValidationException` to be consistent with `ExpectException`. As this is a rename, will result in a breaking compilation error that will need to be resolved.

## v2.1.6
- *Fixed:* `AgentTester.StartupTestServer` optional method overload changed from `TestServer` to `IWebHostBuilder`.

## v2.1.5
- *Enhancement:* Package `Beef.Core` v2.1.11 change as `ExecutionContext.SetCurrent` constraint removal required.

## v2.1.4
- *Enhancement:* Added asynchronous `RunAsync` methods in `ExpectValidationException`.
- *Enhancement:* New overloads for `AgentTester` and `TestSetupAttribute` to allow user identifier and args; these are then passed through to new `CreateExecutionContext` function to allow creation overriding. 
- *Enhancement:* New `ToLongString` extension method for `char` to assist with creating long strings or repeating characters. 

## v2.1.3
- *New:* Added support for Azure Cosmos testing, specifically creation of Cosmos Containers and related data importing.
- *Enhancement:* Improved the .config file and environment variable support.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.