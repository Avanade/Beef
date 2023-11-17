# Step 10 - Service Bus Subscribe

At a high-level this represents the subscribing of events/messages from Azure Service Bus to meet the stated requirements of when an Employee is _terminated_ that the Employee's User Account will be automatically _deactivated_ within OKTA.

This article contains an end-to-end walkthrough to build the requisite Azure Function.

<br/>

## Security domain

A new _Security_ domain _should_ be implemented within a new .NET Solution, independent of the _Hr_ domain. It is conceivable that this new domain could contain its own APIs and data repository, etc. Maybe even leverage _Beef_ ;-)

However, for the purposes of this sample, a new domain will not be implemented. The requisite functionality will be developed within the existing `MyEf.Hr` solution, within a new `MyEf.Hr.Security` namespace for basic separation and simplicity. (_Disclaimer:_ obviously, this is not the recommended approach where implementing proper).

<br/>

## Azure function

It is intended that the _Security_ domain subscribing capabilities are also hosted in Azure, as such the architectural decision has been made to develop leveraging an [Azure Function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview).

This will enable the subscriber function to essentially run in the background, and we will also be able to leverage the [Azure Service Bus trigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger) to invoke the requisite logic per event/message received.

It is assumed for the purposes of this sample that the developer has at least some basic knowledge of Azure Functions, and has likely developed one in the past.

<br/>

## Generic subscribing

The [`CoreEx.Events`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Events) capabilities enables messaging subsystem agnostic subscribing functionality; of interest are the following.

Class | Description
-|-
[`EventSubscriberBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventSubscriberBase.cs) | Provides the messaging platform host agnostic base functionality, such as the [`IErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IErrorHandling.cs) configuration, being the corresponding [`ErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/ErrorHandling.cs) action per error type. Also encapsulates the underlying message to `EventData` deserialization and associated error handling.
[`EventSubscriberOrchestrator`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/EventSubscriberOrchestrator.cs) | Enables none or more subscribers ([`IEventSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IEventSubscriber.cs)) to be added, which are dynamically invoked at runtime when their [`EventSubscriberAttribute`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/EventSubscriberAttribute.cs) configuration matches the received `EventData` message content. For more information see _CoreEx_ [orchestrated subscribing](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Events#orchestrated-subscribing).

<br/>

## Service Bus subscribing

The [`CoreEx.Azure.ServiceBus`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx.Azure/ServiceBus) capabilities will be leveraged to manage the underlying processing once triggered. This has the advantage of enabling standardized, consistent, and tested, functionality to further industrailize event/messaging subscription services.

There are two Azure Service Bus subscribing capabilities that both inherit from the aforementioned `EventSubscriberBase`. These each also leverage the [`ServiceBusSubscriberInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSubscriberInvoker.cs) that ensures consistency of logging, exception handling, associated [`ServiceBusMessageActions`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.webjobs.servicebus.servicebusmessageactions) management to perform the corresponding `CompleteMessageAsync` or `DeadLetterMessageAsync`, and finally message bubbling to enable retries where the error is considered transient in nature.

Class | Description
-|-
[`ServiceBusSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusSubscriber.cs) | Provides the standard [`ServiceBusReceivedMessage`](https://learn.microsoft.com/en-us/python/api/azure-servicebus/azure.servicebus.servicebusreceivedmessage) subscribe (receive) execution encapsulation to run the underlying function logic in a consistent manner. This is generally used where the `EventData` content is consistent; i.e. a single message type.
[`ServiceBusOrchestratedSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusOrchestratedSubscriber.cs) | Provides the `EventSubscriberOrchestrator`-managed [`ServiceBusReceivedMessage`](https://learn.microsoft.com/en-us/python/api/azure-servicebus/azure.servicebus.servicebusreceivedmessage) subscribe (receive) execution encapsulation to run the underlying function logic in a consistent manner. For the most part, this is the preferred, most flexible option.

This sample will leverage the `ServiceBusOrchestratedSubscriber` to subscribe to the _terminated_ events; whilst ignoring all other events. This has the advantage, that other events can be subscribed to over time by leveraging the same underlying Azure Function, whilst ensuring processing in order (where applicable).

<br/>

### Queues vs. Topics and subscriptions

For the most part, where multiple domains exists, then [topics and subscriptions](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-queues-topics-subscriptions) should be leveraged; as each consuming domain can process _their_ own subscription (or subscriptions) independently of others. Additionally, the subscriptions capability enables [filters](https://learn.microsoft.com/en-us/azure/service-bus-messaging/topic-filters) thats can be used to select specific messages in advance of invoking the function code.

This sample has used a basic queue for simplicity.

<br/>

## Create Azure Function project

From Visual Studio, add a new Project named `MyEf.Hr.Security.Subscriptions` (within the existing `MyEf.Hr` solution) leveraging the _Azure Functions_ project template. On the additional information page of the wizard, enter the following, then _Create_ for the new `SecuritySubscriberFunction` function.

Property | Value
-|-
Functions worker | `.NET 6.0 (Long Term Support)`
Connection string setting name | `ServiceBusConnectionString`
Queue name | `%ServiceBusQueueName%`

Then complete the following house cleaning tasks within the newly created project.

<br/>

### Update project dependencies

Update project dependencies as follows.

1. Update the `Microsoft.Azure.WebJobs.Extensions.ServiceBus` NuGet package dependency to latest `5.x.x` version.
2. Add the `Microsoft.Azure.Functions.Extensions` NuGet package as a dependency to enable Dependency Injection (DI). 
3. Add the `CoreEx.Azure` and `CoreEx.Validation` NuGet packages as dependencies.
4. Add `MyHr.Ef.Common` as a project reference dependency (within a real implemenation the `*.Common` assemblies should be published as internal packages for reuse across domains; that is largely their purpose).

<br/>

### Host settings

Open the `host.json` file and replace with the contents from [`host.json`](../MyEf.Hr.Security.Subscriptions/host.json). 

The [configuration](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus) within will ensure that _only_ a _single_ message can be processed at a time, and that there is also _no_ automatic completion of messages.

<br/>

### Local settings

Open the `local.settings.json` file and replace `Values` JSON with the following.

``` json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ServiceBusConnectionString": "get-your-secret-and-paste-here",
    "ServiceBusQueueName": "event-stream"
  }
}
```

<br/>

### Project JSON

Open the `MyEf.Hr.Security.Subscriptions` project XML file and add the following within the `<PropertyGroup>` XML element. This is needed to resolve an issue related to the usage of `System.Memory.Data v6.0.0`.

``` xml
<!-- Following needed as per: https://github.com/Azure/azure-functions-core-tools/issues/2872 -->
<_FunctionsSkipCleanOutput>true</_FunctionsSkipCleanOutput>
<Nullable>enable</Nullable>
```

<br/>

### Global usings

Create a new `GlobalUsings.cs` file, then copy in the contents from [`GlobalUsings`](../MyEf.Hr.Security.Subscriptions/GlobalUsings.cs) replacing existing.

<br/>

### Dependency injection

[Dependency Injection](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection) needs to be added to the Project as this is not enabled by default. To enable, create a new `Startup.cs` file, then copy in the contents from [`Startup`](../MyEf.Hr.Security.Subscriptions/Startup.cs) replacing existing. For the most part the required `CoreEx` services are being registered.

The Service Bus subscribing requires the following additional services registered.

Service | Description
-|-
[`EventSubscriberOrchestrator`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/EventSubscriberOrchestrator.cs) | The `AddEventSubscriberOrchestrator()` is used to register the `EventSubscriberOrchestrator` as a singleton service. The delegate enables the opportunity to further configure options.<br/><br/> Setting `NotSubscribedHandling = ErrorHandling.CompleteAsSilent` indicates that any messages not subscribed should be completed silently without any corresponding logging. Essentially, skipping any unsubscribed messages. <br/><br/> The `AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>()` adds all the `IEventSubscriber` types defined within the specified assembly. The are also additional methods to add specific subscribers manually where applicable.
[`ServiceBusOrchestratedSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Azure/ServiceBus/ServiceBusOrchestratedSubscriber.cs) | The `AddEventSubscriberOrchestrator()` is used to register the `ServiceBusOrchestratedSubscriber` as a scoped service. The delegate enables the opportunity to further configure options.<br/><br/> Similar to above, the likes of `EventDataDeserializationErrorHandling` and other [`IErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IErrorHandling.cs) properties can be overridden to define the default error handling behaviours. 
[`OktaHttpClient`](#OKTA-Http-Client) | The `AddTypedHttpClient()` is a standard .NET method to register a [typed-`HttpClient`](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#typed-clients), in this case an `OktaHttpClient`. A name is also specified so it can be uniquely referenced; this is useful for the likes of [unit testing](https://github.com/Avanade/unittestex#http-client-mocking).


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

This represents the Azure Service Bus trigger subscription entry point; i.e. what is the registered function logic to be executed by the Azure Function runtime fabric. This requires the use of Dependency Injection to access the registered `ServiceBusOrchestratedSubscriber` to orchestrate the underlying subscribers. 

The function method signature must include the [ServiceBusTrigger](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.webjobs.servicebustriggerattribute) attribute to specify the queue or topic/subscription related properties, sessions support, as well as the Service Bus connection string name. Finally, for the `ServiceBusOrchestratedSubscriber` to function correctly the `ServiceBusReceivedMessage` and `ServiceBusMessageActions` parameters must be specified and passed into the `ServiceBusOrchestratedSubscriber.ReceiveAsync`. Note: do _not_ under any circumstances use `AutoCompleteMessages` as completion is managed internally.

Rename the `Function1.cs` to `SecuritySubscriberFunction.cs` where this did not occur correctly. Copy and replace the contents from the following.

``` csharp
namespace MyEf.Hr.Security.Subscriptions;

public class SecuritySubscriberFunction
{
    private readonly ServiceBusOrchestratedSubscriber _subscriber;

    public SecuritySubscriberFunction(ServiceBusOrchestratedSubscriber subscriber) => _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

    [Singleton(Mode = SingletonMode.Function)]
    [FunctionName(nameof(SecuritySubscriberFunction))]
    public Task RunAsync([ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions, CancellationToken cancellationToken)
        => _subscriber.ReceiveAsync(message, messageActions, cancellationToken);
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

Create a new corresponding `appsettings.json` file, then copy in the following contents. Update the file properties; set _Build Action_ to _Content_, and _Copy to Output Directory_ to _Copy if newer_. The '*' within denotes that the configuration settings are accessed internally by _CoreEx_ at runtime and therefore do not need to be specifically defined as `SecuritySettings` properties.

Setting | Description
-|-
`OktaHttpClient:BaseUri` | The base [`Uri`](https://learn.microsoft.com/en-us/dotnet/api/system.uri) for the external OKTA API.
`OktaHttpClient:HttpRetryCount`* | Specifies the number of times the HTTP request should be retried when a transient error occurs.
`OktaHttpClient:HttpTimeoutSeconds`* | Specifies the maximum number of seconds for the HTTP request to complete before timing out. 
`ServiceBusOrchestratedSubscriber.AbandonOnTransient`* | Indicates that the message should be explicitly abandoned where transient error occurs.
`ServiceBusOrchestratedSubscriber.RetryDelay`* | The timespan to delay (multiplied by delivery count) after each transient error is encountered; continues to lock message.

``` json
{
  "OktaHttpClient": {
    "BaseUri": "https://dev-1234.okta.com",
    "HttpRetryCount": 2,
    "HttpTimeoutSeconds": 120
  },
  "ServiceBusOrchestratedSubscriber": {
    "AbandonOnTransient": true,
    "RetryDelay": "00:00:30"
  }
}
```

<br/>

## OKTA Http Client

The external OKTA API must be accessed using an [`HttpClient`](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient). Create a new `OktaHttpClient.cs` file, then copy in the following contents. This inherits from the _CoreEx_ [`TypedHttpClientBase<TSelf>`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Http/TypedHttpClientBaseT.cs) that encapsulates an underlying `HttpClient` and adds extended fluent-style method-chaining capabilities and supporting `SendAsync` logic. Also, of note is the usage of `Result` which will enable simplified chaining and error handling.

The advantage of a typed client such as this, is that it can encapsulate all of the appropriate behavior, making it easier to understand and test. The constructor is using the injected `SecuritySettings` to set the underlying `HttpClient.BaseAddress`. Additionally, other `DefaultOptions` can be set to ensure consistent behaviour of the underlying request/response; these can also be overridden per request where applicable.

For the purposes of _deactivating a user_ the following OKTA capabilities are exposed:

Method | Description
-|-
`GetIdentifier` | The underlying OKTA identifier is not persisted as part of the `Employee` data, only the related `Email`. This method will perform a search on the email and return the corresponding identifier where found. Also checks that the user is in a status that allows deactivation. See OKTA [search user API documentation](https://developer.okta.com/docs/reference/api/users/#list-users-with-search).
`DeactivateUser` | Deactivates the user by using the passed OKTA identifier. See OKTA [deactivate user API documentation](https://developer.okta.com/docs/reference/api/users/#deactivate-user)


``` csharp
namespace MyEf.Hr.Security.Subscriptions;

public class OktaHttpClient : TypedHttpClientBase<OktaHttpClient>
{
    public OktaHttpClient(HttpClient client, SecuritySettings settings, IJsonSerializer? jsonSerializer = null, CoreEx.ExecutionContext? executionContext = null, ILogger<OktaHttpClient>? logger = null) 
        : base(client, jsonSerializer, executionContext, settings, logger)
    {
        Client.BaseAddress = new Uri(settings.OktaHttpClientBaseUri);
        DefaultOptions.WithRetry().EnsureSuccess().ThrowKnownException();
    }

    /// <summary>
    /// Gets the identifier for the email (see <see href="https://developer.okta.com/docs/reference/api/users/#list-users-with-search"/>).
    /// </summary>
    public async Task<Result<OktaUser>> GetUserAsync(Guid id, string email) 
        => Result.GoFrom(await GetAsync<List<OktaUser>>($"/api/v1/users?search=profile.email eq \"{email}\"").ConfigureAwait(false))
            .ThenAs(coll => coll.Count switch 
            {
                0 => Result.NotFoundError($"Employee {id} with email {email} not found within OKTA."),
                1 => Result.Ok(coll[0]),
                _ => Result.NotFoundError($"Employee {id} with email {email} has multiple entries within OKTA.")
            });

    /// <summary>
    /// Deactivates the specified user (<see href="https://developer.okta.com/docs/reference/api/users/#deactivate-user"/>)
    /// </summary>
    public async Task<Result> DeactivateUserAsync(string id) => Result.GoFrom(await PostAsync($"/api/v1/users/{id}/lifecycle/deactivate?sendEmail=true").ConfigureAwait(false));

    /// <summary>
    /// The basic OKTA user properties (see <see href="https://developer.okta.com/docs/reference/api/users/#user-object"/>)
    /// </summary>
    public class OktaUser
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public bool IsDeactivatable => new string[] { "STAGED", "PROVISIONED", "ACTIVE", "RECOVERY", "LOCKED_OUT", "PASSWORD_EXPIRED", "SUSPENDED" }.Contains(Status?.ToUpperInvariant());
    }
}
```

<br/>

## Employee terminated subscriber

As discussed earlier a subscriber must implement [`IEventSubscriber`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IEventSubscriber.cs). To further enable the [`SubscriberBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/SubscriberBase.cs) and [`SubscriberBase<TValue>`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/SubscriberBaseT.cs) abstract base classes enable the requisite functionality. These provide overridable [`IErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/IErrorHandling.cs) configuration, being the corresponding [`ErrorHandling`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/Subscribing/ErrorHandling.cs) action per error type so that specific error handling can be defined per subscriber where needed explicitly.

Where using the `SubscriberBase<TValue>` a `ValueValidator` can be specified to automatically validate the value before the underlying logic is invoked. There is a `ValueIsRequired` property to control whether the value is required; defaults to `true`.

The underlying logic is implemented by overridding the applicable `ReceiveAsync` method. The `ServiceBusReceivedMessage` will have already been converted/deserialized to the appropriate `EventData` or `EventData<TValue>` depending. 

To subscribe to the Employee terminated event raised from the `MyEf.Hr` domain the following metdata will be used to uniquely match. These values will be declared using the [`EventSubscriberAttribute`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/subscribing/EventSubscriberAttribute.cs) for the subscriber. 

Property | Value
-|-
`EventData.Subject` | `MyEf.Hr.Employee`
`EventData.Action` | `Terminated`

The required logic is as follows:
- The `EventData.Value` must be validated to ensure `Id`, `Email` and `Termination` data is present within the event/message data payload. A corresponding [`IValidator<T>`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Validation/IValidatorT.cs) is required to enable.
- Where any security-related error is returned from OKTA treat as transient and attempt an event/message retry (where possible).
- Where the data is not found then complete (versus dead letter) the event/message and write a warning to the log; the assumption being there is nothing that can be done where a corresponding user does not exist in OKTA.
- The business functionality, is to find the user by their email within OKTA, and where found, deactivate (in OKTA).

To implement, create a new `Subscribers` folder witin the `MyEf.Hr.Security.Subscriptions` project. Create a new `EmployeeTerminatedSubcriber.cs` file, then copy in the following contents.

``` csharp
namespace MyEf.Hr.Security.Subscriptions.Subscribers;

[EventSubscriber("MyEf.Hr.Employee", "Terminated")]
public class EmployeeTerminatedSubcriber : SubscriberBase<Employee>
{
    private static readonly Validator<Employee> _employeeValidator = Validator.Create<Employee>()
        .HasProperty(x => x.Id, p => p.Mandatory())
        .HasProperty(x => x.Email, p => p.Mandatory().Email())
        .HasProperty(x => x.Termination, p => p.Mandatory());

    private readonly OktaHttpClient _okta;
    private readonly ILogger _logger;

    public EmployeeTerminatedSubcriber(OktaHttpClient okta, ILogger<EmployeeTerminatedSubcriber> logger)
    {
        _okta = okta ?? throw new ArgumentNullException(nameof(okta));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ValueValidator = _employeeValidator;
    }

    public override ErrorHandling SecurityHandling => ErrorHandling.TransientRetry;

    public override ErrorHandling NotFoundHandling => ErrorHandling.CompleteWithWarning;

    public override Task<Result> ReceiveAsync(EventData<Employee> @event, EventSubscriberArgs args, CancellationToken cancellationToken) 
        => Result.GoAsync(_okta.GetUserAsync(@event.Value.Id, @event.Value.Email!))
            .When(user => !user.IsDeactivatable, user => _logger.LogWarning("Employee {EmployeeId} with email {Email} has User status of {UserStatus} and is therefore unable to be deactivated.", @event.Value.Id, @event.Value.Email, user.Status))
            .WhenAsAsync(user => user.IsDeactivatable, user => _okta.DeactivateUserAsync(user.Id!));
}
```

<br/>

## Unit testing

There is generally no specific provision for the unit testing of Azure Functions, and related Azure Service Bus trigger, as it requires a dependent messaging subsystem, being Azure Service Bus. Also, at time of writing, there is not a standard means of hosting the Azure Function in process to verify in a unit testing context; especially where the likes of Dependency Injection (DI) is being leveraged.

However, given the complexity of logic that typically resides within there should be _strong_ desire to verfiy via unit tests. To enable Avanade has created _UnitTestEx_ which supports the unit testing of [Service Bus-trigger Azure Functions](https://github.com/Avanade/unittestex#service-bus-trigger-azure-function). This capability assumes that Dependency Injection (DI) is being leveraged, and will create the underlying host and enable a `ServiceBusReceivedMessage` to be sent simulating the Azure Function runtime capability.

As the implemented subscriber is invoking OKTA, _UnitTestEx_ also easily enables the [mocking of HTTP requests/responses](https://github.com/Avanade/unittestex#http-client-mocking) to verify both success and failure scenarios.

<br/>

### Create unit test project

From Visual Studio, add a new Project named `MyEf.Hr.Security.Test` (within the existing `MyEf.Hr` solution) leveraging the _NUnit Test Project_ project template.

Make the following house cleaning changes to the new project:

1. Add the `UnitTestEx.NUnit` NuGet package as a dependency.
2. Add `MyHr.Ef.Security.Subscriptions` as a project reference dependency.
3. Rename `Usings.cs` to `GlobalUsings.cs` and replace with content from [`GlobalUsings`](../MyEf.Hr.Security.Test/GlobalUsings.cs).

<br/>

### SecuritySubscriberFunction test

This test is to verfiy the `SecuritySubscriberFunction` capabilities that are non underlying subscriber specific. The following will be tested.

- Verify that an invalid event/message is dead lettered.
- Verify that an non-subscriber event/message is completed silently.

Create a new `SecuritySubscriberFunctionTest.cs` file, then replace with content from [`SecuritySubscriberFunctionTest`](../MyEf.Hr.Security.Test/SecuritySubscriberFunctionTest.cs).

Review and execute the tests and ensure they all pass as expected.

<br/>

### Define canned response

This will be used by the upcoming subscriber test as an OKTA response, leverages an embedded resource as it is easier to maintain as a JSON file, than within the c# test code directly.

Create a new folder named `Responses`, and add a new `EmployeeTerminatedSubscriberTest_Success.json` file, then replace with content from [`EmployeeTerminatedSubscriberTest_Success`](../MyEf.Hr.Security.Test/Responses/EmployeeTerminatedSubscriberTest_Success.json). Go to the file properties and set _Build Action_ to _Embedded Resource_. 

<br/>

### EmployeeTerminatedSubscriber test

This test is to verify the `EmployeeTerminatedSubscriber` capabilities, including the mocking of the underlying HTTP invocations to verify both success and failure scenarios.

To implement, create a new `Subscribers` folder witin the `MyEf.Hr.Security.Test` project. Create a new `EmployeeTerminatedSubscriberTest.cs` file, then replace with content from [`EmployeeTerminatedSubscriberTest`](../MyEf.Hr.Security.Test/Subscribers/EmployeeTerminatedSubscriberTest.cs).

Review and execute the tests and ensure they all pass as expected.

</br>

## Localized testing

To achieve a basic test within a developers machine then the Function should be started using the emulator. Where corresponding events/messages exist within the Azure Service Bus they should start to be received and processed.

_Note:_ that without an _actual_ OKTA development account the OKTA endpoints cannot be consumed directly; i.e. they will always fail. For now, this is good enough to prove potential connectivity.

Where running locally ensure that the Azure Storage Emulator [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) is installed and running; otherwise, errors will occur as the `SingletonAttribute` added to the function requires access to storage to enable.

As Cloud Events are used as the serialization format, any events added manually will need to be in this format, example as follows:

``` json
{
  "specversion": "1.0",
  "id": "49d1b3f2-44f3-4ff2-9d75-847662a233e3",
  "time": "2023-04-21T17:16:57.4014016Z",
  "type": "myef.hr.common.entities.employee",
  "source": "test",
  "subject": "myef.hr.employee",
  "action": "terminated",
  "correlationid": "49d1b3f2-44f3-4ff2-9d75-847662a233e3",
  "datacontenttype": "application/json",
  "data": {
    "id": "00000001-0000-0000-0000-000000000000",
    "email": "bob@email.com",
    "termination": {}
  }
}
```

<br/>

## End-to-end integration testing

Now that all the moving parts have been developed and configured an end-to-end integration test can be performed. This is initiated by invoking the _HR_ domain-based APIs that result in an Azure Service Bus publish and subcribe.

<br/>

## Verify

The new _Security_ domain that performs a [Service Bus Subscribe](./Service-Bus-Subscribe.md) of the _Termination_ related events and proxies [Okta]() (as our identity solution) automatically _Deactivating_ the Employee's account is complete.

*Verification Steps TBD*

## Next Step

Next we will [wrap up](./../README.md#Conclusion) the sample - we are done!    
