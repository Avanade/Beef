# Service Interface

The *Service Interface* is the consumable API endpoint that hosts / encapsulates the underlying (reusable) Logic layer.

<br>

## Usage

This layer is code-generated and responsible for performing the ASP.NET Web API request / response handling, invoking the underlying business logic via a [`Manager`](Layer-Manager.md) class that is instantiated via the `Factory`. 

The [`WebApiActionBase`](../../Beef.AspNetCore.WebApi/WebApiActionBase.cs) provides the base functionality for the following classes to encapsulate the HTTP Method invocation:
- `WebApiGet` - encapsulates the HTTP **`GET`**.
- `WebApiPut` - encapsulates the HTTP **`PUT`**.
- `WebApiPost` - encapsulates the HTTP **`POST`**.
- `WebApiPatch` - encapsulates the HTTP **`PATCH`**.
- `WebApiDelete` - encapsulates the HTTP **`DELETE`**.

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the ouput. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}Controller`.

There are currently no opportunities for a developer to extend on the generated code; beyond amending the underlying code generation templates.

<br>

## Additive features

The following additive features are automatically performed:
- **`ETag`** - where the response entity implements `IETag` then the response `ETag` is set. Where the response entity implements `IEnumerable` then this is iterated and where each item implements `IETag` these and the query string are hashed to generate a largely-unique `Etag` value for the response.
- **`If-None-Match`** - where the `If-None-Match` is supplied in the request the `ETag(s)` provided will be checked against the above `ETag`; where there is a match an HTTP Status Code of 304 (not modified) will be returned. This will avoid the costs of content serialisation and associated network where there is no change. Note that the request to the underlying data source will still occur as no request/response caching occurs by default (with the exception of reference data).
- **Field selection** - fields can be selected (included), or excluded, from the response to reduce the payload to a specific data set as required. The field names are those that appear within the JSON response. Sub-entity (nested) fields can be referenced using dot-notation. This is supported on any HTTP Method invocation that returns a JSON response:
  - **Include**: usage of any of the following query strings `$fields`, `$includeFields`, or `include`; for example `$fields=id,code,text`.
  - **Exclude**: usage of any of the following query strings `$excludeFields` or `$exclude`; for example `$exclude=address.street,quantity`
- **Paging** - paging selection is specified within the query string and is converted to a `PagingArgs` for operations that support paging:
  - **Page** or **Skip** - specifying the required page (using `$page` or `$pageNumber`) or rows skipping (using `$skip`); for example `$page=10`.
  - **Take** - specifying the page size in rows using `$take`, `$top`, `$size` or `pageSize`; for example `$take=25`.
  - **Count** - specifying the required to get the total row count using `$count` or `$totalCount`; for example `$count=true`.