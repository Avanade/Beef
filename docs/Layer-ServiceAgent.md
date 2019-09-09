# Service Agent

The *Service Agent* is the proxy layer that encapsulates the invocation of the corresponding [Service Interface layer](./Layer-ServiceInterface.md). 

This layer is code-generated and responsible for performing the HTTP request / response handling. it generates the following artefacts :

<br>

## Capabilities

The [`WebApiServiceAgentBase`](../src/Beef.Core/WebApi/WebApiServiceAgentBase.cs) provides the core Service Agent functionality for invoking an API endpoint:

- `GetAsync` - Sends an HTTP **`GET`**.
- `PutAsync` - Sends an HTTP **`PUT`**.
- `PostAsync` - Sends an HTTP **`POST`**.
- `PatchAsync` - Sends an [HTTP **`PATCH`**](./Http-Patch.md).
- `DeleteAsync` - Sends an HTTP **`DELETE`**.

The following is generally performed for each:

Step | Description
-|-
`Request` | Create the [`HttpRequestMessage`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage).
`RequestUri` | Set the [`HttpRequestMessage.RequestUri`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage.requesturi) including the query string.
`ETag` | Where there is an [`WebApiRequestOptions.ETag`](../src/Beef.Core/WebApi/WebApiRequestOptions.cs) this will result in an `If-None-Match` (`GET`) or alternatively `If-Match` header.
`Serialize` | Serializes the entity as JSON `HttpRequestMessage.Content` as applicable.
`Send` | Uses an [`HttpClient`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient) to send the `HttpRequestMessage` using the appropriate HTTP Method.
`Response` | Accepts the corresponding [`HttpResponseMessage`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage).
`StatusCode` | Where the `HttpResponseMessage.IsSuccessStatusCode` is `true`, will deserialize the JSON `HttpResponseMessage.Content` as the resulting entity; otherwise, will convert the `HttpResponseMessage.StatusCode` to the equivalent [`IBusinessException`](../src/Beef.Core/IBusinessException.cs) and throw.

<br/>

## Usage

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the output. There are generated classes per [`Entity`](./Entity-Entity-element.md) named `{Entity}Agent` and `{EntityServiceAgent}`.

The `{Entity}Agent` is simply a proxy class that invokes the `{Entity}ServiceAgent` class that is instantiated by the [`Factory`](../src/Beef.Core/Factory.cs) (enables a test mocking opportunity). The `{Entity}Agent` is the component that should be invoked directly by the developer.

There are currently **no** opportunities for a developer to extend on the generated code; beyond amending the underlying code generation templates. This is by design to limit the introduction of business or data logic into this layer.