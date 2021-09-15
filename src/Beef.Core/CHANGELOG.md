# Change log

Represents the **NuGet** versions.

## v4.2.4
- *Enhancement:* Added `HttpAgentBase` (and other related classes) as the base class to enable a `SendAsync` of an optional JSON request and return an optional JSON response (where applicable) to/from an HTTP endpoint. Leverages the base capabilites of `WebApiAgentBase`. Also, supports AutoMapper mapping of request and response values where required. This functionality is needed to support the new code-generated auto implementation of `HttpAgent` as an alternate data source.

## v4.2.3
- *Enhancement:* Add support for using [AutoMapper](https://docs.automapper.org/en/stable/index.html) for the entity-to-entity based mapping (except the database stored procedure mapping which will remain as-is). This has the advantage of broad industry support, and based on initial performance testing offers around a ~90% mapping performance improvement (after first execution).
  - The existing `EntityMapper` has been removed. All capabilities to support `Beef.Data.Database.DatabaseMapper` have been moved to that `Assembly`.
  - The existing `Converters` have been extended so that they can be used for `AutoMapper` and existing `Beef.Data.Database.DatabaseMapper`.
  - Added a new `AutoMapperExtensions` class to add helper extension methods: `OperationTypes`, `Flatten` and `Unflatten` to simplify/improve usage in a _Beef_ context.
  - Existing `Mapper`-related artefacts relocated from `Beef.Abstractions`.
- *Enhancement:* Added support for the `IInt32Identifier` (rename) and `IInt64Identifier` (new).
- *Enhancement:* Added validation `BetweenRule` to enable a value comparison between a from and to value.

## v4.2.2
- *Enhancement:* After a review of the newly introduced `GenericValidator` and the existing `CommonValidator` it has been decided these will be combined because the functionality was so closely aligned (duplicated). To minimize usage impact, the `GenericValidator` will be deprecated, with its unique functionality migrated into the `CommonValidator`. The `Validator` static class has been extended to support the creation of a `CommonValidator` via a new `CreateCommon` method.
- *Fixed:* There were inconsistencies with the `MessageItem.Property` output from `DictionaryRule` and `DictionaryValidator` with respect to the underlying property names. The property name was sometimes including the key value and suffixing with `Key` and `Value` (e.g. `Foo["bar"].Key` and `Foo["bar"].Value`), and others just the key value and no suffix (e.g. `Foo["bar"]` and `Foo["bar"]`) - the latter will be the standardized output.

## v4.2.1
- *Enhancement:* Re-baseline all _Beef_ components to version v4.2.1 required by `Beef.Abstractions` introduction; including updating all dependent NuGet packages to their latest respective version.
- *Issue [139](https://github.com/Avanade/Beef/issues/139)*. Moved the nucleus of `Beef.Core` into a new `Beef.Abstractions` - see the [issue](https://github.com/Avanade/Beef/issues/139) for the reasoning and the changes required as a result of some minor breaking changes.
- *Issue [138](https://github.com/Avanade/Beef/issues/138)*. This is a minor overhaul to the validation capability to support validation of any types via the new `GenericValidator<T>`. The Collection and Dictionary validations have been updated so intrinsic values can be validated, as well as the existing complex entities. For the Dictionary both the `Key` and `Value` can be validated. The [`PersonValidator`](../../samples/Demo/Beef.Demo.Business/Validation/PersonValidator.cs) example has been updated to demonstrate usage. Although, there are a number of breaking changes below, for the most part there should be little impact given limited usage outside of framework itself.
  - *Enhancement:* Added `GenericValidator<T>` which is similar to `Validator<T>` except it supports any `Type`; it is primarily intended for single intrinsic validations such as `string`, `int` or `struct`. There is a single `Rule` method to enable the additional of fluent rules. Complex `Type` validation should continue to use the existing `Validator<T>`.
  - *Enhancement:* The `CollectionRuleItem.Create` methods have had the `Type` constraint removed; no longer just supports entity classes. Also, renamed `Validator` property to `ItemValidator` (breaking change).
  - *Enhancement:* The `DictionaryRuleItem.Create` method has had the `Type` constraint removed; no longer just supports entity classes. Also, renamed `Validator` property to `ValueValidator` (breaking change), and added corresponding new `KeyValidator` property. `DictionaryRuleValue` has been renamed to	`DictionaryRuleItem` (breaking change).
  - *Enhancement:* Added `ReferenceDataCodeRule` to enable validation of a reference data code, being a `string` value. A corresponding `RefDataCode` validator extension method has been added. Example: `Property(x => x.GenderCode).RefDataCode().As<Gender>()`.
  - *Enhancement:* The `DictionaryValidator.Value` property renamed to  `DictionaryValidator.Item` (breaking change).
  - *Enhancement:* The `Validator` static class has had the `Create` methods for collection and dictionary renamed to `CreateCollection` and `CreateDictionary` respectively (breaking changes). New `CreateGeneric<T>` method added to support the new `GenericValidator<T>`. A new `Create<TValidator>` where `TValidator` is `IValidator` method has also been added to create/get from a `ServiceProvider` (dependency injection).
  - *Enhancement:* To enable the `GenericValidator<T>` the `IValidator.EntityType` has been renamed to `ValueType` (breaking change). The `IValidator<T>` has had the `Type` constraint removed.

## v4.1.15
- *Fixed:* The `AddBeefCachePolicyManager` parameter `useCachePolicyManagerTimer` when set to `false` was incorrectly starting the timer resulting in itself and the `CachePolicyManagerServiceHost` running.
- *Fixed:* The `ReferenceDataBase` should not `CleanUp` the key properties as they are considered immutable.
- *Enhancement:* Added `IWebApiAgent` which `WebApiAgentBase` now implements.
- *Enhancement:* Removed the method parameters `memberName`, `filePath` and `lineNumber` to simplify the `WebApiAgentBase`. It is believed these are not being used by any consumers. *Note:* will look to remove all of these parameters throughout the solution within a future _Beef_ version.
- *Enhancement:* Issue [136](https://github.com/Avanade/Beef/issues/136). Added `CollectionValidator` and `DictionaryValidator` to allow each of these types to be validated directly; versus having to be a property within a parent class.

## v4.1.14
- *Enhancement:* Added new `DictionaryRule` validator.
- *Fixed:* Issue [131](https://github.com/Avanade/Beef/issues/131). The `EntityMapper` did not support properties of Type `IDictionary`; this has also been corrected.
- *Fixed:* `ReferenceDataBase` was not setting `IsActive` to `false` when executing `SetInvalid`.
- *Fixed:* The `AddBeefCachePolicyManager` has a new method parameter `useCachePolicyManagerTimer`. The default (`false`) is to use the `CachePolicyManagerServiceHost`; however, in instances where this fails (i.e. Azure function execution) this should be used (`true`) to leverage an internal timer to perform.

## v4.1.13
- *Fixed:* Issue [131](https://github.com/Avanade/Beef/issues/131). `ComplexTypeReflector` and `JsonEntityMerge` updated to support properties of Type `IDictionary<TKey,TValue>`. FYI: underlying dictionary order is important for `JsonEntityMerge` to determine whether changes made; not just whether same keys and values. A dictionary is treated like an array on merge, it is a full replacement operation only.
- *Enhancement:* The existing `TextProvider` has been split into a static `TextProvider` to provide `Current` instance, with abstract base class now being `TextProviderBase`. The `DefaultTextProvider` updated to inherit from this new abstract base class. `TextProvider.Current` will attempt to use explicit, then `ExecutionContext.GetService<TextProviderBase>(false)`, then use `DefaultTextProvider`. The following `IServiceCollection` extension methods has also been added: `AddBeefTextProviderSingleton` and `AddBeefTextProviderScoped` (allows different localization per request).

## v4.1.12
- *Fixed:* The `EntityBasicBase.NotifyChangesWhenSameValue` should not be included within entity mappings, the `MapperIgnoreAttribute` has been added to the property to exclude/ignore.
- *Enhancement:* Added a `GetProperties` to the `EntityReflector` to enable access to all properties.
- *Enhancement:* Added a readonly `ValueType` property to `EventData` to get the `Type` of the underlying value.
- *Enhancement:* Added a `TimerHostedServiceBase` to provide a timer-based `IHostedService` that is `ExecutionContext` and `ServiceProvider` enabled.
- *Enhancement:* Added `CollectionResult` to act similar to `EntityCollectionResult` without the `EntityBase` constraint. New `IEntityCollectionResult<TColl, TEntity>` also added to enable.
- *Enhancement:* Extended `WebApiAgentBase.GetCollectionResultAsync` to support `CollectionResult` in addition to `EntityCollectionResult`.
- *Enhancement:* Split timer-based flush from `CachePolicyManager` and moved into new `CachePolicyManagerServiceHost` (inherits from `TimerHostedServiceBase`). This now represents the background process to periodically flush the caches.
- *Fixed:* The `!=` operator for `ReferenceDataBase` has been fixed to support nullable parameters.
- *Enhancement:* Added `ReferenceDataBaseString` with an `Id` type of `string`.

## v4.1.11
- *Enhancement:* Added new `EventData.Source` as an `Uri` to define the event source. The `EventData.Create*`, `IEventPublisher.Create*` and `IEventPublisher.Publish*` methods have new overloads to support the source `Uri`. `EventPublisherBase` simplified as the `IEventPublisher` is the primary means to access all methods given Dependency Injection (DI) usage.
- *Enhancement:* Changed `EventData` to inherit from `EventMetadata` to house all the properties; this enables separation of metadata from `EventData` as required.
- *Enhancement:* Added `IEventPublisher.SubjectFormat` and `IEventPublisher.ActionFormat` to enable optional uppercase or lowercase formatting.
- *Enhancement:* Added additional overloads and methods to `IRequestCache` to simplify usage (and code-gen output).
- *Enhancement:* Added `IUniqueKey` support to `ReferenceDataBase`.
- *Enhancement:* Added `BeforeRequestAsync` to `IWebApiAgentArgs` to support asynchronous scenarios.

## v4.1.10
- *Fixed:* Issue [121](https://github.com/Avanade/Beef/issues/121). `SlidingCachePolicy` sliding logic functions as expected.

## v4.1.9
- *Fixed:* Issue [121](https://github.com/Avanade/Beef/issues/121). `SlidingCachePolicy` will now cache correctly after first expiry.

## v4.1.8
- *Enhancement:* Added new `InvokerBase<TParam, TResult>` to enable the ability to specifically define the return `Type`.

## v4.1.7
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.
- *Enhancement:* Added `PartitionKeyGenerator` to generate from object array using similar logic to `ETag` MD5 hash.
- *Enhancement:* `EventData` updated to include `PartitionKey` property.

## v4.1.6
- *Fixed:* Issue [110](https://github.com/Avanade/Beef/issues/110). The `EventPublisherBase` has been corrected to allow mulitple `Publish`/`Send` invocations without error.

## v4.1.5
- *Republish:* Fixed code merge issues.

## v4.1.4
- *Enhancement:* Issue [97](https://github.com/Avanade/Beef/issues/97). 
- *Enhancement:* Issue [98](https://github.com/Avanade/Beef/issues/98). 
- *Fixed:* Issue [100](https://github.com/Avanade/Beef/issues/100) fixed. The `AddBeefLoggerEventPublisher` is now adding the service as scoped, versus a singleton.

## v4.1.3
- *Enhancement:* **Breaking change** to _Validation_ to enable async. The existing methods `Validate` and `Run` have been changed to be `ValidateAsync` and `RunAsync` respectively, with a response of `Task`. A number of the validator `Rules` have been updated to support async overloads.
- *Enhancement:* Dependency Injection (DI) for validation has been enabled.
- *Enhancement:* `IValidator<TEntity>` has been added to enable Dependency Injection (DI) support for validators.
- *Removed:* **Breaking change** with the deprecation of `Validator<TEntity, TValidator>` to eliminate usage of existing static `Default` property (the anti-pattern to DI). The `Validator<TEntity>` must now be used.
- *Removed:* `IUniqueKey.HasUniqueKey` has been removed as was redundant (was always set to `true`); otherwise, the inferface should not have been specified. 
- *Fixed:* `EntityBase` no longer implements `IUniqueKey` by default; each implementing class must specify explicitly. 
- *Enhancement:* Added `UniqueKey.IsInitial` to verify whether the arguments within the unique key all have their default value; therefore, indicating that the unique key has not be set.
- *Enhancement:* Added `UniqueKeyDuplicateCheck(bool ignoreWhereUniqueKeyIsInitial)` to the `CollectionRule`. Indicates whether to ignore the `UniqueKey` where `UniqueKey.IsInitial`; useful where the unique key will be generated by the underlying data source on create.
- *Fixed:* `WebApiAgentResult` was failing where the response `Value` type is a `string` and the response content media type is `text/plain`; was attempting to perform a JSON deserialization which has now been corrected.
- *Fixed:* Fixed new compile-time error for `InvokerBase` introduced with Visual Studio v16.8.2.
- *Enhancement:* Added `EntityBasicBase.GenerateETag` that will generate an ETag for a value by serializing to JSON and performing an MD5 hash. This logic has now been centralized.
- *Enhancement:* Added `LoggerEventPublisher` that simply logs the `Subject`, `Action` and `Value` and then swallows/discards.
- *Removed:* `ShortGuid` has been removed.
- *Enhancement:* Added `IGuidIdentifierGenerator`, `IIntIdentifierGenerator` and `IStringIdentifierGenerator` to enable runtime generation of identifier values. A default `GuidIdentifierGenerator` that uses `Guid.NewGuid` is also provided. To use, leverage the new `Property.IdentifierGenerator` to specify the `Type` for Dependency Injection (DI) support.
- *Enhancement:* Added a unique `EventData.EventId` (`Guid`).
- *Enhancement:* **Breaking change** to `IEventPublisher`. The `PublishAsync` methods have been renamed to `Publish` and are now synchronous as the publish should only store/queue internally. A new `SendAsync` method has been added to perform the send as a single atomic operation; i.e. all events should be sent together, and either succeed or fail together. Where previously implemented `IEventPublisher` the existing `PublishEventsAsync` method should be renamed to `SendEventsAsync` - it is then encouraged that the operation is atomic where possible. The `Send` will throw an exception where attempting to send more than once; or publish further events after a send. Where using Dependency Injection (DI) it must be implemented as a _scoped_ service.

## v4.1.2
- *Enhancement:* The `CodeGen` namespace has been moved to `Beef.CodeGen.Core`. A new `StringConversion` now provides access to the existing string conversion functions (e.g. `ToSentenceCase`). The is the first stage of the custom code-gen capability retirement; to be replaced by [`Handlebars.Net`](https://github.com/rexm/Handlebars.Net) as the code-generation engine.
- *Enhancement:* The `ColoredConsoleLogger` was updated to write using `Console.Error` where the `LogLevel` is either `Error` or `Critical`; otherwise, use `Console.Out`.
- *Fixed:* `PropertyMapper` and `PropertySrceMapper` were not correctly updating the destination value(s) where the source was `null`.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection (DI) support.

## v3.1.14
- *Fixed:* `CollectionRuleItem` updated to have an argument-less constructor where an item validator is not required. The existing constructor will now throw a `NullReferenceException` where no validator has been specified (intended to catch validators that have not been constructed correctly).

## v3.1.13
- *Fixed:* Corrected warnings identified by FxCop.

## v3.1.12
- *Enhancement:* `DateTimeTransform`, `StringTransform` and `StringTrim` enums have a new `UseDefault` value added.
- *Enhancement:* `Cleaner` updated to enable default behaviour for the following: `DateTimeTransform`, `StringTransform` and `StringTrim` (replaces existing `const` values).
- *Enhancement:* `Cleaner.DateTimeTransform` defaults to `DateTimeUtc`.
- *Enhancement:* `Cleaner.Clean` cleaning of `DateTime` and `string` updated to account for the new enum values.
- *Enhancement:* `EntityBasicBase.SetValue` overloads (where applicable) have been updated to default to `UseDefault` for the following: `DateTimeTransform`, `StringTransform` and `StringTrim`.
- *Enhancement:* All references to `DateTime.Now` have been updated to `Cleaner.Clean(DateTime.Now)` to ensure the value is set as configured by default.

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
- *Enhanced:* The `PropertyMapper` will check if a _collection_ property is writeable (i.e. read-only) before overriding value; if not, will using the underlying `Add` method to update.
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
- *Added:* Reference data updated to support multiple runtime providers, versus the previous single only. A new `IReferenceDataProvider` enables a provider to be created (code-gen updated to enable).
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
- *Enhancement:* Support overriding of HttpClient creation through the WebApiServiceAgentManager - enables the likes of HttpClientFactory to be used where required.
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
