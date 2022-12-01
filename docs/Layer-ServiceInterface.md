# Service Interface

The *Service Interface* layer is the consumable API endpoint that hosts / encapsulates the underlying (reusable) [Manager layer](./Layer-Manager.md).

This layer is code-generated and responsible for performing the [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/) request / response handling, invoking the underlying business logic via a [`Manager`](Layer-Manager.md) class that is passed as an argument using dependency injection.

<br>

## Capabilities

The [`WebApi`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/WebApis/WebApi.cs) provides the core functionality for the following to encapsulate the specified HTTP Method invocation.

- `GetAsync` - HTTP **`GET`** - generally a Read (CRUD) operation.
- `PutAsync` - HTTP **`PUT`** - generally a Create (CRUD) operation.
- `PostAsync` - HTTP **`POST`** - generally an Update (CRUD) replacement operation.
- `PatchAsync` - HTTP **`PATCH`** - generally an Update (CRUD) modification operation.
- `Delete` - HTTP **`DELETE`** - generally a Delete (CRUD) operation.

The HTTP Method can be overridden within the [`Operation`](./Entity-Operation-Config.md) code-generation element configuration.

<br/>

### Status code

For each `Operation` a primary [`HttpStatusCode`](https://docs.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode) for where the response is not `null`, and alternate for where the response is `null`, is generally supported. The following represents their defaults, these can be overridden within the [`Operation`](./Entity-Operation-Config.md) code-generation element configuration:

OperationType | Description | Primary | Alternate
-|-|-|-
`Get` | A **`GET`** returning a single entity value. |	`HttpStatusCode.OK` | `HttpStatusCode.NotFound`
`GetColl` | A **`GET`** returning an entity collection (the default behaviour where no data is retrieved is to return an empty array using the _Primary_ status code). | `HttpStatusCode.OK` | `HttpStatusCode.NoContent`
`Create` | The **`PUT`** (creation) of an entity. | `HttpStatusCode.Created` | `null`
`Update` | The **`POST`** (replacement) of an entity. | `HttpStatusCode.OK` | `null`
`Patch` | The  **`PATCH`** (modification) of an entity. | `HttpStatusCode.OK` | `null`
`Delete` | The **`DELETE`** of an entity. `HttpStatusCode.OK` | `null`
`Custom` | A **`POST`** (default). | `HttpStatusCode.OK` | `null`

<br/>

### IExtendedException

There are a number of key exceptions that have a specific built in behaviour; these all implement [`IExtendedException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Abstractions/IExtendedException.cs). Where one of these exceptions has been thrown then the corresponding `HttpStatusCode` will be returned automatically.

Exception | Description | HTTP Status | [`ErrorType`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Abstractions/ErrorType.cs)
-|-|-|-
[`AuthenticationException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/AuthenticationException.cs) | Represents an **Authentication** exception. | 401 Unauthorized | 8 AuthenticationError 
[`AuthorizationException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/AuthorizationException.cs) | Represents an **Authorization** exception. | 403 Forbidden | 3 AuthorizationError 
[`BusinessException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/BusinessException.cs) | Represents a **Business** exception whereby the message returned should be displayed directly to the consumer. | 400 BadRequest | 2 BusinessError 
[`ConcurrencyException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ConcurrencyException.cs) | Represents a data **Concurrency** exception; generally as a result of an errant [ETag](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IETag.cs). | 412 PreconditionFailed | 4 ConcurrencyError 
[`ConflictException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ConflictException.cs) | Represents a data **Conflict** exception; for example creating an entity that already exists. | 409 Conflict | 6 ConflictError 
[`DuplicateException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/DuplicateException.cs) | Represents a **Duplicate** exception; for example updating a code on an entity where the value is already used. | 409 Conflict | 7 DuplicateError 
[`NotFoundException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/NotFoundException.cs) | Represents a **NotFound** exception; for example getting an entity that does not exist. | 404 NotFound | 5 NotFoundError 
[`TransientException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/TransientException.cs) | Represents a **Transient** exception; failed but is a candidate for a retry. | 503 ServiceUnavailable | 9 TransientError 
[`ValidationException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ValidationException.cs) | Represents a **Validation** exception with a corresponding `Messages` [collection](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/MessageItemCollection.cs). | 400 BadRequest | 1 ValidationError 

<br/>

## Additive features

The following additive features are automatically enabled by _CoreEx_ [`WebApis`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/WebApis):

Feature | Description
-|-
`ETag` | Where the response entity implements [`IETag`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IETag.cs) then the response `ETag` is set. Where the response entity implements `IEnumerable` then this is iterated and where each item implements `IETag` these and the query string are hashed to generate a largely-unique `Etag` value for the response.
`If-None-Match` | Where an `If-None-Match` is supplied in the request the `ETag(s)` provided will be checked against the above `ETag`; where there is a match an HTTP Status Code of `304` (`NotModified`) will be returned. This will avoid the costs of content serialisation and associated network where there is no change. Note that the request to the underlying data source will still occur as no request/response caching occurs by default (with the exception of [reference data](./Reference-Data.md)).
[Field selection](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Text/Json/JsonFilterer.cs) | Fields can be selected (included), or excluded, from the response to reduce the payload to a specific data set as required. The field names are those that appear within the JSON response. Sub-entity (nested) fields can be referenced using dot-notation. This is supported on any HTTP Method invocation that returns a JSON response: <br/>- **Include**: usage of any of the following query strings `$fields`, `$includeFields`, or `$include`; for example `$fields=id,code,text`.<br/>- **Exclude**: usage of any of the following query strings `$excludeFields` or `$exclude`; for example `$exclude=address.street,quantity`
Paging | Paging selection is specified within the query string and is converted to a [`PagingArgs`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/PagingArgs.cs) for operations that have been configured to support paging: <br/> - **Page** or **Skip** - specifying the required page (using `$page` or `$pageNumber`) or rows to skip (using `$skip`); for example `$page=10` or `$skip=100`. <br/> - **Take** - specifying the page size in rows using `$take`, `$top`, `$size` or `pageSize`; for example `$take=25`. <br/> - **Count** - specifying the requirement to get the total row count using `$count` or `$totalCount`; for example `$count=true`.

<br/>

## Usage

The [`Operation`](./Entity-Operation-Config.md) element within the `entity.beef-5.yaml` configuration primarily drives the output. There is a generated class per [`Entity`](./Entity-Entity-Config.md) named `{Entity}Controller`. For example, if the entity is named `Person`, there will be corresponding `PersonController`

There are currently **no** opportunities for a developer to extend on the generated code; beyond amending the underlying code generation templates. This is by design to limit the introduction of business or data logic into this layer.