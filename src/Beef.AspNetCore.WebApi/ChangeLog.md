# Change log

Represents the **NuGet** versions.

## v3.1.4
- *Enhancment:* `WebApiExecutionContextMiddleware` updated to use the `Cleaner.Clean` when setting the `ExecutionContext.Timestamp`.

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