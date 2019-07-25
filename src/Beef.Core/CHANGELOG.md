# Change log

Represents the **NuGet** versions.

## v2.1.8

- *Enhancement:* `IIdentifier` added to give base capabilites to `IIntIdentifier` and `IGuidIdentifer`. 
- *New:* `IStringIdentifier` added to enable support for a `string`-based identifier.

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
