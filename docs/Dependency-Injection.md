# Dependency Injection

_Beef_, and the underlying [_CoreEx_](https://github.com/Avanade/CoreEx) capabilities, need to be set up correctly using [Dependency Injection (DI)](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) at startup to function correctly. Given _Beef_ exists primarily to industralize the development of APIs, it is logical to expect this setup to occur during [App startup in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup). For example, see _My.Hr_ sample [`Startup.cs`](../samples/My.Hr/My.Hr.Api/Startup.cs).

<br/>

## Core services

There are a number of core services that need to be configured for _CoreEx_ to function.

``` csharp
// Add the core services.
services.AddSettings<HrSettings>()
        .AddExecutionContext()
        .AddReferenceDataOrchestrator()
        .AddJsonSerializer()
        .AddWebApi()
        .AddReferenceDataContentWebApi()
        .AddJsonMergePatch()
        .AddRequestCache()
        .AddValidationTextProvider()
        .AddValidators<EmployeeManager>();
```

These are as follows:

Service | Description
-|-
`AddSettings` | Adds the [`SettingsBase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Configuration/SettingsBase.cs) _singleton_ service; alternatively, where not specifically implemented use `AddDefaultSettings`.
`AddExecutionContext` | Adds the _scoped_ service to create an [`ExecutionContext`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ExecutionContext.cs); the creation can be overridden by specifying the underlying `executionContextFactory` parameter.
`AddReferenceDataOrchestrator` | Adds the [`ReferenceDataOrchestrator`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/ReferenceDataOrchestrator.cs) _singleton_ service to manage the centralized [Reference Data](./Reference-Data.md) orchestration and caching. See also [CoreEx.RefData](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/README.md).
`AddJsonSerializer` | Adds the _scoped_ service that implements [`IJsonSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/IJsonSerializer.cs), being either the [`CoreEx.Text.Json.JsonSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Text/Json/JsonSerializer.cs) or [`CoreEx.Newtonsoft.Json.JsonSerializer`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Newtonsoft/Json/JsonSerializer.cs). Also, registers the corresponding [`IReferenceDataContentJsonSerializer `](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/IReferenceDataContentJsonSerializer.cs) which is an alternate `IJsonSerializer` designed to serialize (emit) the contents of an [`IReferenceData`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/IReferenceData.cs).
`AddWebApi` | Adds the [`WebApi`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/WebApis/WebApi.cs) _scoped_ service used by the likes of the ASP.NET [Controllers](../samples/My.Hr/My.Hr.Api/Controllers/Generated/EmployeeController.cs) to orchestrate the underlying request operation logic in a standardized/consistent manner. See also [CoreEx.WebApis](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/WebApis/Readme.md).
`AddReferenceDataContentWebApi` | Adds the [ReferenceDataContentWebApi](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/WebApis/ReferenceDataContentWebApi.cs) _scoped_ service that uses the specialized [`IReferenceDataContentJsonSerializer `](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/IReferenceDataContentJsonSerializer.cs).
`AddJsonMergePatch` | Adds the [`IJsonMergePatch`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/Merge/IJsonMergePatch.cs) _singleton_ service using the [`JsonMergePatch`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Json/Merge/JsonMergePatch.cs) that is resposible for enabling the JSON Merge Patch (`application/merge-patch+json`) functionality.
`AddRequestCache` | Adds the [`IRequestCache`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Caching/IRequestCache.cs) _scoped_ service using the [`RequestCache`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Caching/RequestCache.cs) for short-lived caching used by the likes of the [data-service-layer](./Layer-DataSvc.md) to reduce [data-layer](./Layer-Data.md) chattiness.
`AddValidationTextProvider` | Adds the [`ITextProvider`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Localization/ITextProvider.cs) _singleton_ service using the [`ValidationTextProvider`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Validation/ValidationTextProvider.cs) . See also [CoreEx.Validation](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Validation/README.md).
`AddValidators` | Adds all the [`IValidatorEx`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Validation/IValidatorExT.cs) types from the specified `Assembly` as _scoped_ services using reflection. Individual validators can be added singularly using `AddValidator`.

<br/>

## Database services

As a minumum the `AddDatabase` is needed to add the [`IDatabase`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database/IDatabase.cs) _scoped_ service. This is required to enable the specified [`Database`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.Database/Database.cs) instance to be registered. 

Where using [CoreEx.EntityFramework](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.EntityFrameworkCore/README.md) then the corresponding `AddDbContext` (standard Microsoft Entity Framework requirement) is required. As is the `AddEfDb`, that adds the [`IEfDb`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.EntityFrameworkCore/IEfDb.cs) _scoped_ service that enables the underlying extended EF capabilities.

Example as follows.

``` csharp
// Add the beef database services (scoped per request/connection).
services.AddDatabase(sp => new HrDb(() => new SqlConnection(sp.GetRequiredService<HrSettings>().DatabaseConnectionString), p.GetRequiredService<ILogger<HrDb>>()));

// Add the beef entity framework services (scoped per request/connection).
services.AddDbContext<HrEfDbContext>();
services.AddEfDb<HrEfDb>();
```

<br/>

## Event publishing

The [CoreEx.Events](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/README.md) provides an agnostic, flexible, pluggable, approach to the [publishing](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventPublisher.cs) ([formatting](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventDataFormatter.cs), [serializing](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSerializer.cs) and [sending](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventSender.cs)) of [`EventData`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventData.cs) objects.

There are the likes of the [`NullEventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/NullEventPublisher.cs) and [`NullEventSender`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/NullEventSender.cs) that can be used for initial development that simplly swallow/discard the events on send.

Example as follows. The event outbox capabilities where required are described further [here](../samples/My.Hr/docs/Employee-DB.md#event-outbox) within the [My.Hr sample](../samples/My.Hr/README.md).

``` csharp
// Add event publishing services.
services.AddNullEventPublisher();

// Add transactional event outbox services.
services.AddScoped<IEventSender>(sp =>
{
    var eoe = new EventOutboxEnqueue(sp.GetRequiredService<IDatabase>(), p.GetRequiredService<ILogger<EventOutboxEnqueue>>());
    //eoe.SetPrimaryEventSender(/* the primary sender instance; i.e. service bus */); // This is ptional.
    return eoe;
});
```

<br/>

## Mapping

The [CoreEx.Mapping.Mapper](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Mapping/Mapper.cs) provides an alternative to AutoMapper (which can still be used). Designed and implemented as a simple (explicit) [`IMapper`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Mapping/IMapper.cs) capability that enables the key `Map`, `Flatten` and `Expand` mapping capabilities. This is no reflection/compiling magic, just specified C# mapping code which executes very fast (and is easily debuggable).

The `AddMappers` adds all the [`IMapper<TSource, TDestination>`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Mapping/IMapperT.cs) types from the specified `Assembly` into a _singleton_ `Mapper` service using reflection.

See `CoreEx.AutoMapper` extensions to implement `IMapper` via the [`AutoMapperWrapper`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx.AutoMapper/AutoMapperWrapper.cs). The `IMapper` is intended to decouple _CoreEx_ from any specific implementation.