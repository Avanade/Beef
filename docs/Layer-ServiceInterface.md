# Service Interface

The *Service Interface* layer is the consumable API endpoint that hosts / encapsulates the underlying (reusable) [Logic layer](./Layer-Manager.md).

This layer is code-generated and responsible for performing the [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/) request / response handling, invoking the underlying business logic via a [`Manager`](Layer-Manager.md) class that is passed as an argument using dependency injection.

<br>

## Capabilities

The [`WebApiActionBase`](../src/Beef.AspNetCore.WebApi/WebApiActionBase.cs) provides the base functionality for the following to encapsulate the specified HTTP Method invocation:

- `WebApiGet` - HTTP **`GET`**.
- `WebApiPut` - HTTP **`PUT`**.
- `WebApiPost` - HTTP **`POST`**.
- `WebApiPatch` - [HTTP **`PATCH`**](./Http-Patch.md).
- `WebApiDelete` - HTTP **`DELETE`**.

<br/>

### Status code

For each `Operation` a primary [`HttpStatusCode`](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode) for where the response is not `null`, and alternate for where the response if `null`, is specified. The following represents their defaults, these can be overridden within the [`Operation`](./Entity-Operation-element.md) code-generation element configuration:

OperationType | Description | Primary | Alternate
-|-|-|-
`Get` | A **`GET`** returning a single entity value. |	`HttpStatusCode.OK` | `HttpStatusCode.NotFound`
`GetColl` | A **`GET`** returning an entity collection (the default behaviour where no data is retrieved is to return an empty array using the _Primary_ status code). | `HttpStatusCode.OK` | `HttpStatusCode.NoContent`
`Create` | The **`PUT`** (creation) of an entity. | `HttpStatusCode.Created` | `null`
`Update` | The **`POST`** (update) of an entity. | `HttpStatusCode.OK` | `null`
`Patch` | The  **`PATCH`** (partial update) of an entity. | `HttpStatusCode.OK` | `null`
`Delete` | The **`DELETE`** of an entity. `HttpStatusCode.OK` | `null`
`Custom` | A **`POST`** (default). | `HttpStatusCode.OK` | `null`

<br/>

### IBusinessException

There are a number of key exceptions that have a specific built in behaviour; these all implement [`IBusinessException`](../src/Beef.Core/IBusinessException.cs). Where one of these exceptions has been thrown then the corresponding `HttpStatusCode` will be returned.

Exception | Description | HTTP Status | [`ErrorType`](../src/Beef.Core/ErrorType.cs)
-|-|-|-
[`AuthenticationException`](../src/Beef.Core/AuthenticationException.cs) | Represents an **Authentication** exception. | 401 Unauthorized | 8 AuthenticationError 
[`AuthorizationException`](../src/Beef.Core/AuthorizationException.cs) | Represents an **Authorization** exception. | 403 Forbidden | 3 AuthorizationError 
[`BusinessException`](../src/Beef.Core/BusinessException.cs) | Represents a **Business** exception whereby the message returned should be displayed directly to the consumer. | 400 BadRequest | 2 BusinessError 
[`ConcurrencyException`](../src/Beef.Core/ConcurrencyException.cs) | Represents a data **Concurrency** exception; generally as a result of an errant [ETag](../src/Beef.Core/Entities/IETag.cs). | 412 PreconditionFailed | 4 ConcurrencyError 
[`ConflictException`](../src/Beef.Core/ConflictException.cs) | Represents a data **Conflict** exception; for example creating an entity that already exists. | 409 Conflict | 6 ConflictError 
[`DuplicateException`](../src/Beef.Core/DuplicateException.cs) | Represents a **Duplicate** exception; for example updating a code on an entity where the value is already used. | 409 Conflict | 7 DuplicateError 
[`NotFoundException`](../src/Beef.Core/NotFoundException.cs) | Represents a **NotFound** exception; for example updating a code on an entity where the value is already used. | 404 NotFound | 5 NotFoundError 
[`ValidationException`](../src/Beef.Core/ValidationException.cs) | Represents a **Validation** exception with a corresponding `Messages` [collection](../src/Beef.Core/Entities/MessageItemCollection.cs). | 400 BadRequest | 1 ValidationError 

<br/>

## Additive features

The following additive features are automatically enabled by _Beef_:

Feature | Description
-|-
**`ETag`** | Where the response entity implements [`IETag`](../src/Beef.Core/Entities/IETag.cs) then the response `ETag` is set. Where the response entity implements `IEnumerable` then this is iterated and where each item implements `IETag` these and the query string are hashed to generate a largely-unique `Etag` value for the response.
**`If-None-Match`** | Where an `If-None-Match` is supplied in the request the `ETag(s)` provided will be checked against the above `ETag`; where there is a match an HTTP Status Code of `304` (`NotModified`) will be returned. This will avoid the costs of content serialisation and associated network where there is no change. Note that the request to the underlying data source will still occur as no request/response caching occurs by default (with the exception of [reference data](./Reference-Data.md)).
**Field selection** | Fields can be selected (included), or excluded, from the response to reduce the payload to a specific data set as required. The field names are those that appear within the JSON response. Sub-entity (nested) fields can be referenced using dot-notation. This is supported on any HTTP Method invocation that returns a JSON response: <br/>- **Include**: usage of any of the following query strings `$fields`, `$includeFields`, or `$include`; for example `$fields=id,code,text`.<br/>- **Exclude**: usage of any of the following query strings `$excludeFields` or `$exclude`; for example `$exclude=address.street,quantity`
**Paging** | Paging selection is specified within the query string and is converted to a [`PagingArgs`](../src/Beef.Core/Entities/PagingArgs.cs) for operations that have been configured to support paging: <br/> - **Page** or **Skip** - specifying the required page (using `$page` or `$pageNumber`) or rows to skip (using `$skip`); for example `$page=10` or `$skip=100`. <br/> - **Take** - specifying the page size in rows using `$take`, `$top`, `$size` or `pageSize`; for example `$take=25`. <br/> - **Count** - specifying the requirement to get the total row count using `$count` or `$totalCount`; for example `$count=true`.

<br/>

## Usage

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the output. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}Controller`.

There are currently **no** opportunities for a developer to extend on the generated code; beyond amending the underlying code generation templates. This is by design to limit the introduction of business or data logic into this layer.