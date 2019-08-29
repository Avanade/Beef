# HTTP PATCH

In addition to HTTP `GET/POST/PUT/DELETE` there is support for HTTP `PATCH`. This allows an update to an entity value where only the properties that have changed are passed to the API. This has the added benefit of being less brittle with respect to versioning as not all properties are required as per a `POST/PUT`.

<br/>

## Approaches

There are two approaches that are supported:
- **JSON Merge Patch** - https://tools.ietf.org/html/rfc7396 - content type: application/merge-patch+json
- **JSON Patch** - https://tools.ietf.org/html/rfc6902 - content type: application/json-patch+json

<br/>

## JSON Merge Patch

This is likely to be the most common approach that will be leveraged. Which is simply the passing of properties that are to be updated within the JSON payload. To nullify a property then it must be explicitly set to null. To support, the [`JsonEntityMerge`](../src/Beef.Core/Json/JsonEntityMerge.cs) is provided to enable.

``` json
{
  "title": "Hello!",
  "phoneNumber": "+01-123-456-7890",
  "author": {
    "familyName": null
  },
  "tags": [ "example" ]
}
```

<br/>

### Arrays

However, the _gotcha_ is with arrays, in that all items must be passed as this is a replacement operation only. Within the specification there is no means to reference a specific item for change; however, a **JSON Patch** enables through array indexing.

_Beef_ has extended support to enable an indexing of sorts through the use of the [`IUniqueKey`](../src/Beef.Core/Entities/IUniqueKey.cs) which all generated entities can support. By enabling one or more properties as forming the `UniqueKey` this is used to match within the existing collection. All items still need to be passed so that the add, change and delete can be determined; however, then only the properties to change for each item need to be provided.

<br/>

### JSON Patch

This is unlikely to be used but is supported if needed.

``` json
[
  { "op": "test", "path": "/a/b/c", "value": "foo" },
  { "op": "remove", "path": "/a/b/c" },
  { "op": "add", "path": "/a/b/c", "value": [ "foo", "bar" ] },
  { "op": "replace", "path": "/a/b/c", "value": 42 },
  { "op": "move", "from": "/a/b/c", "path": "/a/b/d" },
  { "op": "copy", "from": "/a/b/d", "path": "/a/b/e" }
]
```

<br/>

## Code-generation

The [code generation](../tools/Beef.CodeGen.Core/README.md) supports `PATCH` operations.

This is a special case operation in that only `XxxAgent`, `XxxServiceAgent` and `XxxController` classes are ever generated. Under the covers the `XxxController` uses the corresponding `Get` operation to validate existence, and validate `etag` for concurrency. The JSON patching then occurs on this entity value. Assuming changes are made, and are valid, the corresponding `Update` operation is used to perform the actual update; as such, all existing validation and updating logic is reused.

The basics for the `Patch` operation is as follows. Note that it is similar to the `Update` operation in that the `UniqueKey` and `WebApiRoute` are the key attributes.

``` xml
<Operation Name="Update" OperationType="Update" UniqueKey="true" Validator="UserValidator" WebApiRoute="{id}" AutoImplement="None" />
<Operation Name="Patch" OperationType="Patch" UniqueKey="true" WebApiRoute="{id}" />
``` 

<br/>

## Collection uniqueness

To enable a collection to be updated using a key the `UniqueKey` attribute needs to be defined for one or more of the properties within an entity. Once this has been done, and re-generated, then support for key merges will be operational.


For example a `ContractAddress` entity could have a `Type` property to uniquely identify; therefore, it should set `UniqueKey="true"` as follows:

``` xml
<Entity Name="ContactAddress" Text="Contact Addresss" Collection="true" ExcludeAll="true" ExcludeData="false" AutoImplement="Database">
  <Property Name="Type" Type="RefDataNamespace.AddressType" UniqueKey="true" RefDataType="string" DataName="AddressTypeId" />
  <Property Name="Street1" Type="string" />
```

<br/>

## Service agent support

The generated service agent for `Patch` has a different method signature to enable to simplify its execution, as follows:

``` csharp
/// <summary>
/// Patches the <see cref="Person"/> object.
/// </summary>
/// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
/// <param name="value">The JSON patch value.</param>
/// <param name="id">The <see cref="Person"/> identifier.</param>
/// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
/// <returns>A <see cref="WebApiAgentResult"/>.</returns>
public Task<WebApiAgentResult<Person>> PatchAsync(WebApiPatchOption patchOption, JToken value, Guid id, WebApiRequestOptions requestOptions = null)
{
    return PersonServiceAgent.PatchAsync(patchOption, value, id, requestOptions);
}
```

<br/>

The sample [Demo](../samples/Demo) demonstrates the service agent being within the [`PersonTest`](../samples/Demo/Beef.Demo.Test/PersonTest.cs) - see method `H130_Patch_MergePatch`.