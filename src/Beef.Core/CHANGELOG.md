# Change log

Represents the **NuGet** versions.

## v2.1.20
- *Added:* Reference data updated to support multiple providers. A new `IReferenceDataProvider`

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
