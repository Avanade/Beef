# Change log

Represents the **NuGet** versions.

## v3.1.11
- *Enhancement:* `Event.PublishAsync()`, `Event.PublishAsync<T>()`, `EventData.Create` and `EventData.Create<T>()` methods have been obsoleted (and will be removed in the future). These have been replaced with `Event.PublishEventAsync()`, `Event.PublishValueEventAsync<T>()`, `EventData.CreateEvent()` and `EventData.CreateValueEvent<T>()`. The `template` and `KeyValuePair` approach has been replaced with C# [string interpolation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated); e.g. `Event.PublishEventAsync($"Company.Entity.{id}", "Delete", id);`. Code-generation should be performed to update the generated codebase to support the new methods and approach.
- *Enhancement:* Added `Event.EventSubjectPrefix` which when specified will be prepended to all `EventData.Subject` values when created (`EventData.Create*`). 
- *Enhancement:* Added `WebApiArgFormatterAttribute` that can be applied to an entity property to enable conversion (`IPropertyMapperConverter`) from .NET `Type` to a corresponding URL query string value.

## v3.1.10
- *Fixed:* The automatic connection management/sharing does not support multiple concurrent threads - the internal stack was being unwound in the incorrect sequence closing the wrong connection. To support this a new `ExecutionContext.FlowSuppression` method has been added to assist the creation of new `ExecutionContext` instances to enable (and ensure separation). This is an opt-in requirement to enable.

## v3.1.9
- *Fixed:* The `IPropertySrceMapper` updated to fully support nullable reference types.
- *Fixed:* Added `lock` to `DataContextScope.GetContext` to ensure thread-safety for parallel executions. 

## v3.1.8
- *Fixed:* The `PropertyMapper` will only auto map sub-entities at mapping execution time where no converter or mapper has been specified; was previously always auto mapping at construction, often unnecessarily where a converter or mapper was later being specified.
- *Enhanced:* The `PropertyMapper` will check if a _collection_ property is writeable (i.e. read-only) before overridding value; if not, will using the underlying `Add` method to update.
- *Enhanced:* The `ComplexTypeReflector` now exposes a `MethodInfo? AddMethod` property for an underlying _collection_ (where found). 

## v3.1.7
- *Enhanced:* Added additional transformations `StartsWith`, `EndsWith`, `Contains`, `TrimStart`, `TrimEnd` and `Remove` to the code-generation templating.

## v3.1.6
- *Enhanced:* Added `IEquatable<T>` to `EntityBase`, `EntityBaseCollection` and `ReferenceDataBase`. Enables support for full property, sub entity and collection equality `Equals` checking and `GetHashCode` calculation. 

## v3.1.5
- *Fixed:* `ReferenceDataConverterUtils.CheckIsValid` validation logic fixed.

## v3.1.4
- *Enhanced:* Added `AuthenticationException` to enable standardized handling of this exception similar to the existing `AuthorizationException`. This allows for an _authentication_ exception to be thrown which in turn will result in an `HttpStatusCode.Unauthorized (401)`.
- *Enhanced:* Added property `UserId` to `ExecutionContext`.

## v3.1.3
- *Enhanced:* Added _model_ representations of `ReferenceDataBase` and `ChangeLog`. 
- *Enhanced:* Added `CustomConverter` to simplify process of creating converters. Added `GuidToStringConverter` and `NullableGuidToStringConverter`.
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.2
- *Enhanced:* Added `ApplyAsObject` to `JsonPropertyFilter` which will only return a `JToken` where filtering is performed; otherwise, will return the originating object value. This will avoid a JSON serialization where not needed.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).
- *Added:* Nullable rollout phase: https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/
- *Enhanced:* The `BusinessInvokerBase` when beginning a transaction will now pass `TransactionScopeAsyncFlowOption.Enabled` to ensure that the database transaction will flow asynchronously.
- *Removed:* The `PerformanceTimer` and `WebApiPerformanceTimer` have been removed. Using other tools such as AppInsights provides this insight.
- *Enhanced:* Code-generation now executes asynchronously; previous synchronous operations have been removed and replaced with `xxxAsync` versions.
- *Removed:* The `Beef.FlatFile` namespace and classes have been removed; this capability will be migrated to a new `Beef.FlatFile` assembly at a later date.

## v2.1.28
- *Fixed:* Decoupled (removed) the `IncludeFields` and `ExcludeFields` from the `PagingArgs` are these relate to any request not those that just include paging; these now exist as properties on the `WebApiRequestOptions`. Apologies, if used this will result in a breaking change.

## v2.1.27
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.26
- *Enhanced*: `EntityMapper` and `EntitySrceMapper` support new `GetBySrceProperty` and `GetByDestProperty` (as applicable) methods that enable using a property expression versus a string.

## v2.1.25
- *Fixed:* Compile error from Visual Studio v16.4.1 corrected.

## v2.1.24
- *Fixed:* Introduced FxCop Analysis to `Beef.Core`; this version represents the remediation based on the results.
- *Fixed:* The `IChangeTracking` has been extended to support tracking through sub-entities and collections. This includes `EntityBase.CopyOrClone` method to copy or clone entity; note that a collection is always cloned. 

## v2.1.23
- *Fixed:* `TwoKeyValueCache` has had the concurrency logic refactored to resolve an issue where the value could be overridden with an incorrect version as the locking between the two keys was not correctly synchronized.

## v2.1.22
- *Added:* `ReferenceDataFilter.ApplyFilter` supports new parameter `isActiveOnly` (defaults to `true`) that indicates whether to include `ReferenceDataBase.IsActive` entries only; otherwise, `false` for all.
- *Added:* `EntityBasicBase.GetRefDataText` added to enable the conditional runtime getting of reference data text during serialization. `ExecutionContext.IsRefDataTextSerializationEnabled` also added to enable.
- *Enhanced:* `ReferenceDataMultiCollection` accepts `Items` as `IEnumerable<T>` versus `IReferenceDataCollection` to enable greater flexibility for the source of data; there is no serialization change.

## v2.1.21
- *Fixed:* `ReferenceDataFilter.ApplyFilter` would throw a `NullReferenceException` where a code was null within the array; this is fixed.

## v2.1.20
- *Added:* Reference data updated to support multiple run-time providers, versus the previous single only. A new `IReferenceDataProvider` enables a provider to be created (code-gen updated to enable).
- *Added:* Reference data now supports a `ReferenceDataFilter` to filter by a list of codes and/or text wildcard. Leveraged by the code-gen `XxxController` to enable filtering against the in-memory cache.

## v2.1.19
- *Added:* Moved `Events.Subscribe` capability to new `Beef.Events` assembly.
- *Added:* `Cleaner.Clean` extended to perform a `EntityBasicBase.AcceptChanges` where appropriate.

## v2.1.18
- *Added:* Support for `IConvertible` added to `ReferenceDataBase` to enable usage of `Convert.ChangeType`.
- *Added:* `PropertyMapper` updated to use `Convert.ChangeType` as last resort property value mapping.

## v2.1.17
- *Added:* New `Beef.Event.Subscribe` namespace added to enable the base capabilities for the subscription of events.

## v2.1.16
- *Added:* The `ReferenceDataSidList` has had a new method `ToCodeList` to get the list of codes added.
- *Added:* A new extenstion method `IQueryable.WhereWith` added to simply the specification of a where clause when the `with` value is not the `default`.
- *Enhanced:* `TypeReflector.GetProperties` added to provide single, shared, approach.
- *Fixed:* `TypeReflector.GetProperty` fixed to ensure only single named get/set property returned.
- *Added:* `ChangeLogMapper` added to ensure consistency mapping `ChangeLog` entity; specifically, the `Created*` and `Updated*` properties for the corresponding mapping operation type.
- *Fixed:* `EntityMapper` had a number of fixes made.

## v2.1.15
- *Fixed:* An `ExecutionContext.Username` get will return `Environment.UserName` as a default where not overridden to ensure a valid value is returned.
- *Fixed:* The `IEnumerable` extensions `WhereWildcard` will correctly construct the internal lambda expression to correctly construct the underlying where statement.
- *Added:* The `CodeGenTemplate` now supports Switch-Case-Default XML-based statements.

## v2.1.14
- *Fixed:* `JsonEntityMerge` was not correctly merging an array where the item inherited from `EntityBase` and the underlying `HasUniqueKey` was `false`.

## v2.1.13
- *New:* Promoted `YamlConverter` into `Beef.Core` from `Beef.Test.NUnit` as it has an application beyond just testing.
- *Fixed:* `Factory.ResetLocal` to clear only for the running thread; not all. This resulted in wonky tests where mocking was not reliably functioning.

## v2.1.12
- *Fixed:* `DictionarySetCache`, `BiDictionarySetCache` and `TwoKeySetCache` were not resetting the cache correctly on a flush ensuring data was reloaded on next hit.
- *Enhancement:* Applied Visual Studio Code Cleanup.

## v2.1.11
- *Fixed:* Constraint removed from `ExecutionContext.SetCurrent` so it can be called even where `HasBeenRegistered` is `true`.

## v2.1.10
- *Enhancement:* `PagingArgs.DefaultIsGetCount` added to enable default to be set globally.

## v2.1.9
- *Fixed:* `PropertyMapper<>` was not correctly identifying/selecting the property where being overridden.

## v2.1.8
- *Enhancement:* `IIdentifier` added to give base capabilites to `IIntIdentifier` and `IGuidIdentifer`. 
- *New:* `IStringIdentifier` added to enable support for a `string`-based identifier.
- *New:* Added `ExecutionContext.PartitionKey` support.

## v2.1.7
- *Enhancement:* Support overridding of HttpClient creation through the WebApiServiceAgentManager - enables the likes of HttpClientFactory to be used where required.
- *Enhancement:* Renamed WebApiInvoker to WebApiServiceAgentInvoker to make its intended purpose more explicit.

## v2.1.6
- *Fixed:* InvokerBase was non functioning as a generic class; reimplemented. Other Invokers updated accordingly.

## v2.1.5
- *Fixed:* FromBody not applied correctly to ServiceAgent.
- *Enhancement:* Code generation updated where using ReferenceDataCodeConverter to use Property SID; also results in minor performance improvement.

## v2.1.4
- *Fixed:* Cache policy configuration loading failed on nullable type.

## v2.1.3
- *Fixed:* JsonEntityMerge did not support a UniqueKey property that was a Reference Data type.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.
