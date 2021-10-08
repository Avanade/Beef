# Change log

Represents the **NuGet** versions.

## v4.2.3
- *Enhancement:* Renamed `GrpcAgentResult.HttpStatusCode` to `GrpcAgentResult.StatusCode` to accomodate `IWebApiAgentResult.StatusCode`. 

## v4.2.2
- *Enhancement:* `GrpcAgentBase` updated to support _AutoMapper_ mapping (removed existing`EntityMapper` functionality).

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.4
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.3
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v4.1.2
- *Fixed:* An HTTP Delete will now catch a `NotFoundException` and return an HTTP Status Code 204 (no content) as a delete is considered idempotent.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.2
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.1
- *New:* Initial publish to GitHub. New capability to support gRPC.
