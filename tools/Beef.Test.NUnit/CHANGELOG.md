# Change log

Represents the **NuGet** versions.

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