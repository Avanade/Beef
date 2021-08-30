# Change log

Represents the **NuGet** versions.

## v4.2.7
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.2.6
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.2.5
- *Fixed:* Updated referenced *Beef* NuGet references to latest related to AutoMapper introduction.

## v4.2.4
- *Fixed:* Fixed hard-coded company and appname values.

## v4.2.3
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.2.2
- *Enhancement:* Replace all XML code-gen configurations with YAML equivalents.
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.10
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.9
- *Enhancement:* Changed the default entity code-gen behaviour to be the new `EntityScope = "Autonomous"`.
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.8
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.7
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.6
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.5
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.4
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.3
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.2
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.14
- *Fixed:* Updated referenced *Beef* NuGet references to latest.
- *Enhancement:* Updated the Database/EfDB/CosmosDb names to be prefixed by AppName to avoid potential conflict with Namespaces.

## v3.1.13
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.12
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.11
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.10
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.9
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.8
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.7
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.6
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.5
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.4
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.3
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.2
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v3.1.1
- *Upgrade:* Upgraded the projects to .NET Core 3.1 and amended for any other _Beef_ required changes.
- *Added:* Introduced nullable reference types: https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/

## v2.1.12
- *Fixed:* The database project had an incorrect reference to `Beef.Database.Core`. The **Delete** test now correctly checks for the `Delete` event as it is considered idempotent.

## v2.1.11
- *Enhancement:* Update the sample to include a `POST` operation including corresponding tests.

## v2.1.10
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v2.1.9
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v2.1.8
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v2.1.7
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v2.1.6
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v2.1.5
- *Fixed:* Updated referenced *Beef* NuGet references to latest.
- *Fixed:* `ExpectValidationException.Run` renamed to `ExpectValidationException.Throws` to correct example Test code.

## v2.1.4
- *Fixed:* Updated referenced *Beef* NuGet references to latest.

## v2.1.3
- *Enhancement:* Added `PersonValidator` and `PersonTest` for end-to-end example.

## v2.1.2
- *Enhancement:* Added new `--datasource` parameter. Choices are `Database` (default), `EntityFramework`, `Cosmos` or `None`.

## v2.1.1
- *New:* Initial publish to GitHub.