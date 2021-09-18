# Change log

Represents the **NuGet** versions.

## v4.2.3
- *Enhancement:* Updated `WebApiQueryString.PagingArgsSkipQueryStringNames` to support `offset` value, and 
`WebApiQueryString.PagingArgsTakeQueryStringNames` to support `limit` value.

## v4.2.2
- *Fixed:* Updated internal _Beef_ dependencies to latest.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.

## v4.1.8
- *Enhancement:* Updated `WebApiStartup.ConfigurationBuilder` to finally probe the command-line arguments.
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.7
- *Enhancement:* Issue [116](https://github.com/Avanade/Beef/issues/116). Add capability to set the HTTP `Location` Header value.

## v4.1.6
- *Fixed:* Issue [114](https://github.com/Avanade/Beef/issues/114). 

## v4.1.5
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v4.1.4
- *Enhancement:* Issue [97](https://github.com/Avanade/Beef/issues/97). 
- *Enhancement:* Issue [98](https://github.com/Avanade/Beef/issues/98). 

## v4.1.3
- *Fixed:* Where calculating an `ETag` for a response that does not support `IETag` it was previously using the `Hashcode` to calculate. This is problematic as per this [article](https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/). The new approach is to MD5 hash the serialized JSON. 

## v4.1.2
- *Fixed:* An HTTP Delete will now catch a `NotFoundException` and return an HTTP Status Code 204 (no content) as a delete is considered idempotent.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.7
- *Fixed:* Issue [60](https://github.com/Avanade/Beef/issues/60) fixed; ETag will be generated and returned correctly where underlying `Type` does not implement `IETag`.

## v3.1.6
- *Enhancement:* Updated the `WebApiStartup.ConfigurationBuilder` to probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration), 2) User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 3) environment variable (using specified prefix), 4) `appsettings.{environment}.json`, 5) `appsettings.json`, 6) `webapisettings.{environment}.json` (embedded resource), and 7) `webapisettings.json` (embedded resource).

## v3.1.5
- *Enhancement:* Added `WebApiStartup.BuildWebHost` and `WebApiStartup.ConfigurationBuilder` to provide the following capability in a standard manner: Builds (creates) the `IWebHost` using `WebHost.CreateDefaultBuilder`, and build the configuration probing; will probe in the following order: 1) Azure Key Vault (see https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration) or User Secrets where hosting environment is development (see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets), 2) environment variable (using specified prefix), 3) `appsettings.{environment}.json`, 4) `appsettings.json`, 5) `webapisettings.{environment}.json` (embedded resource), and 6) `webapisettings.json` (embedded resource).

## v3.1.4
- *Enhancement:* `WebApiExecutionContextMiddleware` updated to use the `Cleaner.Clean` when setting the `ExecutionContext.Timestamp`.

## v3.1.3
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.2
- *Enhancement:* Conversion to an intermediary `JToken` will only occur where field filtering is requested; otherwise, it will default to the standard ASP.NET serialization. Given that filtering is infrequently used this avoids an unnecessary conversion, which in micro-benchmark testing resulted in 2-4 times better serialization performance.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Core 3.1.
- *Added:* Nullable rollout phase: https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/

## v2.1.14
- *Fixed:* Decoupled (removed) the `IncludeFields` and `ExcludeFields` from the `PagingArgs` are these relate to any request not those that just include paging; these now exist as properties on the `WebApiActionBase`. 

## v2.1.13
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.12
- *Fixed:* Compile error from Visual Studio v16.4.1 corrected.

## v2.1.11
- *Fixed:* Introduced FxCop Analysis to `Beef.AspNetCore.WebApi`; this version represents the remediation based on the results.
- *Fixed:* Introduced FxCop Analysis to `Beef.Core`; this version represents the remediation based on the results.

## v2.1.10
- *Added:* `WebApiQueryString.` added to parse the query string for `$Text=true`. Where set then the `ExecutionContext.IsRefDataTextSerializationEnabled` will be automatically set to `true` before serializing the result enabling conditional *reference data* text output. An additional `XxxText` property must be explicitly added to enable; `RefDataText` attribute to code-gen to support.

## v2.1.9
- *Added:* `WebApiQueryString.GetReferenceDataSelection` added to parse the query string for the reference data selection/filter.

## v2.1.8
- *Fixed:* A `PATCH` operation was incorrectly attempting to return a `null` value where no changes were made as a result of merging the JSON changes; the result of the `GET` will be returned to ensure a response (content) is returned.

## v2.1.7
- *Fixed:* `ExecutionContext.PagingArgs` always updated regardless of whether paging parameter explicitly defined.
- *Added:* `WebApiActionBase` has new `BodyValue` property that will be updated when `FromBodyAttribute` specified. A number of other properties have been made `public` (from `protected`) - the `WebApiActionBase` is accessible from `ExecutionContext.Properties[WebApiActionBase.ExecutionContextPropertyKey]`.

## v2.1.6
- *Added:* New `WebApiExecutionContextMiddleware` and `WebApiExceptionHandlerMiddleware` added, including `Use*` extension methods to properly integrate into the pipeline as per https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write.

## v2.1.5
- *Enhancement:* An invocation with an `If-Match` will override the value where it implements `IEtag` as this should take precedence over the value inside of the value itself via `WebApiActionBase.Value`. Code-gen has also been updated to take advantage of this; next gen will introduce usage within `XxxApiController` classes.

## v2.1.4
- *Fixed:* `WebApiControllerHelper.SetETag` will not format where already formatted. 

## v2.1.3
- *Fixed:* InvokerBase was non functioning as a generic class; reimplemented. Other Invokers updated accordingly.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.