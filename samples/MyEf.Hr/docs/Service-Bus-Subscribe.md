# Step 9 - Service Bus Subscribe

At a high-level this represents the subscribing of events/messages from Azure Service Bus to meet the stated requirements of when an Employee is _terminated_ the Employee's User Account will be automatically _deactivated_ within OKTA.

This article contains an end-to-end walkthrough to build the requisite Azure Function.

<br/>

## Security domain

A new _Security_ domain _should_ be implemented within a whole new Solution, independent of the _Hr_ domain. It is conceivable that this new domain could contain its own APIs and data repository, etc.

However, for the purposes of this sample, a new domain will not be implemented. The requisite functionality will be developed within the existing `MyEf.Hr` solution, with a new `MyEf.Hr.Security` namespace for basic separation and simplicity. (_Disclaimer:_ obviously, this is not the recommended approach where implementing for real).

<br/>

## Azure function

It is intended that the _Security_ domain is also hosted in Azure, as such the architectural decision has been made to develop the subscribing capabilities leveraging an [Azure Function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview).

This will enable the subscriber function to essentially run in the background, and we will also able to leverage the [Azure Service Bus trigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger) to invoke the requisite logic per event/message received.

It is assumed for the purposes of this sample that the developer has at least some basic knowledge of Azure Functions, and has likely developed one in the past.

<br/>

## Generic subscribing

The [`CoreEx.Events`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Events) capabilities enables event/messaging subscribing functionality; of interest are the following.

Class | Description
-|-
[`EventSubscriberBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventSubscriberBase.cs) | Provides the messaging platform host agnostic base functionality, such as the [`IErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IErrorHandling.cs) configuration, being the corresponding [`ErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/ErrorHandling.cs) action per error type. Also encapsulates the underlying message to `EventData` deserialization and associated error handling.
[`EventSubscriberOrchestrator`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/EventSubscriberOrchestrator.cs) | Enables none or more subscribers ([`IEventSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IEventSubscriber.cs)) to be added, which are dynamically invoked at runtime when their [`EventSubscriberAttribute`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/subscribing/EventSubscriberAttribute.cs) configuration matches the received `EventData` content. For more information see [orchestrated subscribing](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Events#orchestrated-subscribing).

<br/>

## Service Bus subscribing

The [`CoreEx.Azure.ServiceBus`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx.Azure/ServiceBus) capabilities will be leveraged to manage the underlying processing once triggered. This has the advantage of enabling standardized, and tested, functionality to further industrailize event/messaging subscription services.

There are two Azure Service Bus subscribing capabilities that both inherit the `EventSubscriberBase`. These each also leverage the [`ServiceBusSubscriberInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSubscriberInvoker.cs) that ensures consistency of logging, exception handling, associated [`ServiceBusMessageActions`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.webjobs.servicebus.servicebusmessageactions) management to perform the corresponding `CompleteMessageAsync` or `DeadLetterMessageAsync`, and finally message bubbling to enable retries where the error is considered transient in nature.

Class | Description
-|-
[`ServiceBusSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSubscriber.cs) | Provides the standard [`ServiceBusReceivedMessage`](https://learn.microsoft.com/en-us/python/api/azure-servicebus/azure.servicebus.servicebusreceivedmessage) subscribe (receive) execution encapsulation to run the underlying function logic in a consistent manner. This is generally used where the `EventData` content is consistent; i.e. a single message type.
[`ServiceBusOrchestratedSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusOrchestratedSubscriber.cs) | Provides the `EventSubscriberOrchestrator`-managed [`ServiceBusReceivedMessage`](https://learn.microsoft.com/en-us/python/api/azure-servicebus/azure.servicebus.servicebusreceivedmessage) subscribe (receive) execution encapsulation to run the underlying function logic in a consistent manner. For the most part, this is the preferred, most flexible option.

This sample will leverage the `ServiceBusOrchestratedSubscriber` to subscribe to the _terminated_ events; whilst ignoring all other events. This has the advantage, that other events can be subscribed to over time leveraging the same underlying Azure Function, whilst processing in order (where applicable).

<br/>

## Create Azure Function project

From Visual Studio, add a new Project named `MyEf.Hr.Security.Subscriptions` leveraging the _Azure Functions_ project template. On the additional information page of the wizard, enter the following, then _Create_.

Property | Value
-|-
Functions worker | `.NET 6.0 (Long Term Support)`
Function | `SecuritySubscriberFunction`
Connection string setting name | `ServiceBusConnectionString`
Queue name | `%ServiceBusQueueName%`

Then complete the following house cleaning tasks within the newly created project.

<br/>

### Update project dependencies

Update project dependencies as follows.

1. Add the `CoreEx.Azure` and `CoreEx.Validation` NuGet packages as dependencies.
2. Add a project reference to `MyHr.Ef.Common` as a dependency (within a real implemenation the `*.Common` assemblies should be published as internal packages for reuse).

<br/>

### Local settings

Open the `local.settings.json` and replace `Values` JSON with the following.

``` json
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ServiceBusConnectionString": "get-your-secret-and-paste-here",
    "ServiceBusQueueName": "event-stream"
```

<br/>

### Project JSON

Open the `MyEf.Hr.Security.Subscriptions` project XML file and add the following within the `<PropertyGroup>` XML element. This is needed to resolve an issue related to the usage of `System.Memory.Data v6.0.0`.

``` xml
<!-- Following needed as per: https://github.com/Azure/azure-functions-core-tools/issues/2872 -->
<_FunctionsSkipCleanOutput>true</_FunctionsSkipCleanOutput>
```

<br/>

### Global usings

Create a new `GlobalUsings.cs` file, then copy in the contents from [`GlobalUsings`](../MyEf.Hr.Security.Subscriptions/GlobalUsings.cs) replacing existing.

<br/>

### Dependency injection

[Dependency Injection](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection) needs to be added to the Project as this is not enabled by default. To enable, create a new `Startup.cs` file, then copy in the contents from [`Startup`](../MyEf.Hr.Security.Subscriptions/Startup.cs) replacing existing. For the most part the required `CoreEx` services are being registered.

The Service Bus Publishing requires the following services registered.

Service | Description
-|-
[`EventSubscriberOrchestrator`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/EventSubscriberOrchestrator.cs) | The `AddEventSubscriberOrchestrator()` is used to register the `EventSubscriberOrchestrator` as a singleton service. The delegate enables the opportunity to further configure options.<br/><br/> Setting `NotSubscribedHandling = ErrorHandling.CompleteAsSilent` indicates that any messages not subscribed should be completed silently without any corresponding logging. <br/><br/> The `AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>()` adds all the `IEventSubscriber` types defined within the specified assembly.
[`ServiceBusOrchestratedSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusOrchestratedSubscriber.cs) | The `AddEventSubscriberOrchestrator()` is used to register the `ServiceBusOrchestratedSubscriber` as a scoped service. The delegate enables the opportunity to further configure options.<br/><br/> Similar to above, the likes of `EventDataDeserializationErrorHandling` and other [`IErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IErrorHandling.cs) properties can be overridden to define the default error handling behaviours. 
[`OktaHttpClient`](#OKTA-Http-Client) | The `AddTypedHttpClient` is a standard .NET means to register a typed-[`HttpClient`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient), in this case an `OktaHttpClient`. A name is also specified so it can be uniquely referenced; this is useful for the likes of unit testing.


The aforementioned services registration code of interest is as follows.

``` csharp
.AddEventSubscriberOrchestrator((_, o) =>
{
    o.NotSubscribedHandling = ErrorHandling.CompleteAsSilent;
    o.AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>());
})
.AddAzureServiceBusOrchestratedSubscriber((_, o) =>
{
    o.EventDataDeserializationErrorHandling = ErrorHandling.ThrowSubscriberException;
})
.AddTypedHttpClient<OktaHttpClient>("OktaApi");
```

<br/>

## Security subscriber function

This represents the Azure Service Bus trigger subscription entry point; i.e. what is the registered function logic to be executed by the Azure Function runtime fabric. This requires the use of Dependency Injection to access the registered `ServiceBusOrchestratedSubscriber` to orchestrate the underlying subscriber (which is still to be developed). 

The function method signature must include the [ServiceBusTrigger](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.webjobs.servicebustriggerattribute) attribute to specify the queue or topic related properties, sessions support, as well as the Service Bus connection string name. Finally, for the `ServiceBusOrchestratedSubscriber` to function correctly the `ServiceBusReceivedMessage` and `ServiceBusMessageActions` parameters must be specified and passed into the `ServiceBusOrchestratedSubscriber.ReceiveAsync`.

Rename the `Function1.cs` to `SecuritySubscriberFunction.cs` where this did not occur correctly. Copy and replace the contents from the following.

``` csharp
namespace MyEf.Hr.Security.Subscriptions;

public class SecuritySubscriberFunction
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber;

    public SecuritySubscriberFunction(ServiceBusOrchestratedSubscriber subscriber) => _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

    [Singleton(Mode = SingletonMode.Function)]
    [FunctionName(nameof(SecuritySubscriberFunction))]
    [ExponentialBackoffRetry(3, "00:02:00", "00:30:00")]
    public Task RunAsync([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
        => _subscriber.ReceiveAsync(message, messageActions);
}
```

<br/>

## Configuration settings

Create a new `SecuritySettings.cs` file, then copy in the following contents. This is similar to the `HrSettings` created earlier; however, the properties are specific to this domain. The key property is the `OktaHttpClientBaseUri` to support the OKTA API endpoint specification.

``` csharp
namespace MyEf.Hr.Security.Subscriptions;

/// <summary>
/// Provides the <see cref="IConfiguration"/> settings.
/// </summary>
public class SecuritySettings : SettingsBase
{
    /// <summary>
    /// Gets the setting prefixes in order of precedence.
    /// </summary>
    public static string[] Prefixes { get; } = { "Security/", "Common/" };

    /// <summary>
    /// Initializes a new instance of the <see cref="SecuritySettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public SecuritySettings(IConfiguration configuration) : base(configuration, Prefixes) => ValidationArgs.DefaultUseJsonNames = true;

    /// <summary>
    /// Gets the OKTA API base URI.
    /// </summary>
    public string OktaHttpClientBaseUri => GetRequiredValue<string>("OktaHttpClient__BaseUri");
}
```

<br/>

Create a new `appsettings.json` file, then copy in the following contents. The '*' within denotes that the configuration settings are accessed internally by _CoreEx_ at runtime and therefore do not need to be specifically defined as `SecuritySettings` properties.

Setting | Description
-|-
`OktaHttpClient:BaseUri` | The base [`Uri`](https://learn.microsoft.com/en-us/dotnet/api/system.uri) for the external OKTA API.
`OktaHttpClient:HttpRetryCount` | Specifies the number of times the HTTP request should be retried when a transient error occurs.
`OktaHttpClient:HttpTimeoutSeconds` | Specifies the maximum number of seconds for the HTTP request to complete before timing out. 

``` json
{
  "OktaHttpClient": {
    "BaseUri": "https://dev-1234.okta.com",
    "HttpRetryCount": 2,
    "HttpTimeoutSeconds": 120
  }
}
```

<br/>

## OKTA Http Client

The external OKTA API must be accessed using an [`HttpClient`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient). Create a new `OktaHttpClient` file, then copy in the following contents. This inherits from the _CoreEx_ [`TypedHttpClientBase<TSelf>`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Http/TypedHttpClientBaseT.cs) that encapsulates an underlying `HttpClient` and adds extended fluent-style method-chaining capabilities and supporting SendAsync logic.

The advantage of a typed client such as this, is that it can encapsulate all of the appropriate behavior, making it easier to understand and test. The constructor is using the injected `SecuritySettings` to set the underlying `HttpClient.BaseAddress`. Additionally, other `DefaultOptions` can be set to ensure consistent behaviour of the underlying request/response; these can be overridden per request where applicable.



``` csharp
namespace MyEf.Hr.Security.Subscriptions;

public class OktaHttpClient : TypedHttpClientBase<OktaHttpClient>
{
    public OktaHttpClient(HttpClient client, SecuritySettings settings, IJsonSerializer? jsonSerializer = null, CoreEx.ExecutionContext? executionContext = null, ILogger<OktaHttpClient>? logger = null) 
        : base(client, jsonSerializer, executionContext, settings, logger)
    {
        Client.BaseAddress = new Uri(settings.OktaHttpClientBaseUri);
        DefaultOptions.WithRetry().EnsureOK().EnsureSuccess().ThrowKnownException();
    }

    /// <summary>
    /// Gets the identifier for the email (see <see href="https://developer.okta.com/docs/reference/api/users/#list-users-with-search"/>).
    /// </summary>
    public async Task<string?> GetIdentifier(string email)
    {
        var response = await EnsureOK().GetAsync<List<OktaUser>>($"/api/v1/users?search=profile.email eq \"{email}\"").ConfigureAwait(false);
        var user = response.Value.SingleOrDefault();

        return user?.Status?.ToUpperInvariant() switch
        {
            "STAGED" or "PROVISIONED" or "ACTIVE" or "RECOVERY" or "LOCKED_OUT" or "PASSWORD_EXPIRED" or "SUSPENDED" => user?.Id,
            _ => null
        };
    }

    /// <summary>
    /// Deactivates the specified user (<see href="https://developer.okta.com/docs/reference/api/users/#deactivate-user"/>)
    /// </summary>
    public async Task DeactivateUser(string identifier)
    {
        var response = await EnsureOK().EnsureNoContent().PostAsync($"/api/v1/users/{identifier}/lifecycle/deactivate?sendEmail=true").ConfigureAwait(false);
        response.ThrowOnError();
    }

    /// <summary>
    /// The basic OKTA user properties (see <see href="https://developer.okta.com/docs/reference/api/users/#user-object"/>)
    /// </summary>
    private class OktaUser
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
    }
}
```
