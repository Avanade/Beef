# Change log

Represents the **NuGet** versions.

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