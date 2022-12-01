# Service Agent

The *Service Agent* is the proxy layer that encapsulates the invocation of the corresponding [Service Interface layer](./Layer-ServiceInterface.md). 

This layer is code-generated and responsible for performing the HTTP request / response handling.

<br>

## Capabilities

The [`TypedHttpClientBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Http/TypedHttpClientBaseT.cs) provides the core Service Agent functionality for invoking an API endpoint using HTTP:

- `GetAsync` - HTTP **`GET`** - generally a Read (CRUD) operation.
- `PutAsync` - HTTP **`PUT`** - generally a Create (CRUD) operation.
- `PostAsync` - HTTP **`POST`** - generally an Update (CRUD) replacement operation.
- `PatchAsync` - HTTP **`PATCH`** - generally an Update (CRUD) modification operation.
- `Delete` - HTTP **`DELETE`** - generally a Delete (CRUD) operation.

The following is generally performed for each:

Step | Description
-|-
`Request` | Create the [`HttpRequestMessage`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage).
`RequestUri` | Set the [`HttpRequestMessage.RequestUri`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage.requesturi) including the query string.
`ETag` | Where there is an [`HttpRequestOptions.ETag`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Http/HttpRequestOptions.cs) this will result in an `If-None-Match` (`GET`) or alternatively `If-Match` header.
`Serialize` | Serializes the entity (where specified) as JSON to the `HttpRequestMessage.Content` as applicable.
`Send` | Uses an underlying [`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) to send the `HttpRequestMessage` using the specified HTTP Method.
`Response` | Accepts the corresponding [`HttpResponseMessage`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage).
`StatusCode` | Where the `HttpResponseMessage.IsSuccessStatusCode` is `true`, will deserialize the JSON `HttpResponseMessage.Content` as the resulting entity; otherwise, will convert the `HttpResponseMessage.StatusCode` to the equivalent [`IExtendedException`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Abstractions/IExtendedException.cs) and throw.

<br/>

## Usage

There is a generated class per [`Entity`](./Entity-Entity-Config.md) named `{Entity}Agent`. There is also a corresonding interface named `I{Entity}Agent` generated so the likes of test mocking etc. can be employed. For example, if the entity is named `Person`, there will be corresponding `PersonAgent` and `IPersonAgent` classes.

There are currently **no** opportunities for a developer to extend on the generated code; beyond amending the underlying code generation templates. This is by design to limit the introduction of business or data logic into this layer.

<br/>

## Request manipulation

However, there will more often than not be the need to manipulte the request. For example, add the likes of the credentials for authentication as part of the request before send. There are multiple opportunities to perform this; select the best method based on the requirements:

- [`DelegatingHandler`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.delegatinghandler) - the _preferred approach_ that allows standardized request logic to be used for any `HttpClient.SendAsync`. This [article](https://www.stevejgordon.co.uk/httpclientfactory-aspnetcore-outgoing-request-middleware-pipeline-delegatinghandlers), and the two preceeding within, provide the details on how to achieve.
- [`OnBeforeRequest`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Http/TypedHttpClientBaseT.cs) - as the `{Entity}Agent` is implemented as a partial class this can be extended by overridding the default protected `OnBeforeRequest` method and the requistite logic included.
