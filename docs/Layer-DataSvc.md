# DataSvc (Service Orchestration)

The _DataSvc_ is primarily responsible for orchestrating the underlying data access; whilst often one-to-one there may be times that this class will be used to coordinate multiple data access components. It is responsible for the ensuring that the related `Entity` is fully constructed/updated/etc. as per the desired operation.

<br/>

## Execution caching

To improve potential performance, and reduce chattiness, within an in-process execution context the `DataSvc` introduces a level of caching (short-lived). The cache is managed using the [`IRequestCache`](../src/Beef.Core/Caching/IRequestCache.cs) via the following: `TryGetValue`, `SetValue`, `Reove`, `Clear` and `CacheClearAll`.

The purpose of this cache is to minimise this chattiness to the underlying data source, to reduce this cost, where the time between calls (measured in milliseconds) is such that the data retrieved previously is considered sufficient/valid. This way within an execution context (request) lifetime a developer can invoke the `XxxDataSvc` multiple times with only a single data source cost.

This logic of getting, setting and clearing the cache is included within the primary `Get`, `Create`, `Update` and `Delete` operations only.

<br>

## Event-driven

To support the goals of an [Event-driven architecture](https://en.wikipedia.org/wiki/Event-driven_architecture) an event publish can be included.

An [`EventData`](../src/Beef.Core/Events/EventData.cs) publish is invoked where the eventing infrastructure has been included (configured) during [code-generation](./tools/Beef.CodeGen.Core). The [`IEventPublisher`](../src/Beef.Core/Events/IEventPublisher.cs) implementation is responsible for publishing (sending) the event message. 

_Note:_ This is _always_ performed directly after the primary operation logic such that the _event_ is only published where successful. This is **not** transactional so if the event publish fails there is no automatic rollback capabilitity. The implementor will need to decide the corrective action for the failure.

<br/>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, or has extension opportunities to inject additional logic into the processing pipeline.

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the output. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}DataSvc`.

There is also a corresonding interface named `I{Entity}DataSvc` generated so the likes of test mocking etc. can be employed.

### Code-generated
 
An end-to-end code-generated processing pipeline generally consists of:

Step | Description
-|-
`DataSvcInvoker` | The logic is wrapped by a [`DataSvcInvoker`](../src/Beef.Core/Business/DataSvcInvoker.cs). This enables the [`BusinessInvokerArgs`](../src/Beef.Core/Business/BusinessInvokerBase.cs) options to be specified, including [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`Cache` | Trys the cache and returns result where found (as applicable).
`Data` | The [`{Entity}Data`](./Layer-DataSvc.md) layer is invoked to perform the data processing.
`EventPublish` | Constructs the `EventData` and invokes the `Event.Publish`.
`Cache` | Performs a cache set or remove (as applicable).
`OnAfter`* | The `OnAfter` extension opportunity; where set this will be invoked. This enables logic to be invoked _after_ the primary `Operation` is performed.

_\* Note:_ To minimize the generated code the extension opportunities are only generated where selected. This is performed by using the [`EntityElement`](./Entity-Entity-element.md) and setting the `DataSvcExtensions` attribute to `true` (defaults to `false`).

The following demonstrates the generated code (a snippet from the sample [`ContactDataSvc`](../samples/Demo/Beef.Demo.Business/DataSvc/Generated/ContactDataSvc.cs)) that does not include `DataSvcExtensions`:

``` csharp
// A Get operation.
public Task<Contact?> GetAsync(Guid id)
{
    return DataSvcInvoker.Current.InvokeAsync(typeof(ContactDataSvc), async () => 
    {
        var __key = new UniqueKey(id);
        if (_cache.TryGetValue(__key, out Contact __val))
            return __val;

        var __result = await _data.GetAsync(id).ConfigureAwait(false);
        _cache.SetValue(__key, __result!);
        return __result;
    });
}

// An Update operation.
public Task<Contact> UpdateAsync(Contact value)
{
    return DataSvcInvoker.Current.InvokeAsync(typeof(ContactDataSvc), async () => 
    {
        var __result = await _data.UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
        await _evtPub.PublishValueAsync(__result, $"Demo.Contact.{__result.Id}", "Update").ConfigureAwaitfalse);
        _cache.SetValue(__result.UniqueKey, __result);
        return __result;
    });
}
```

The following demonstrates the generated code (a snippet from the sample [`PersonDataSvc`](../samples/Demo/Beef.Demo.Business/DataSvc/Generated/PersonDataSvc.cs)) that includes `DataSvcExtensions`:

``` csharp
// A Get operation.
public Task<Person?> GetAsync(Guid id)
{
    return DataSvcInvoker.Current.InvokeAsync(typeof(PersonDataSvc), async () => 
    {
        var __key = new UniqueKey(id);
        if (_cache.TryGetValue(__key, out Person __val))
            return __val;

        var __result = await _data.GetAsync(id).ConfigureAwait(false);
        _cache.SetValue(__key, __result!);
        if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id).ConfigureAwait(false);
        return __result;
    });
}


// An Update operation.
public Task<Person> UpdateAsync(Person value)
{
    return DataSvcInvoker.Current.InvokeAsync(typeof(PersonDataSvc), async () => 
    {
        var __result = await _data.UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
        await _evtPub.PublishValueAsync(__result, $"Demo.Person.{__result.Id}", "Update").ConfigureAwait(false);
        _cache.SetValue(__result.UniqueKey, __result);
        if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result).ConfigureAwait(false);
        return __result;
    }, new BusinessInvokerArgs { IncludeTransactionScope = true });
}
```

<br/>

### Custom

A custom (`OnImplementation`) processing pipeline generally consists of:

Step | Description
-|-
`DataSvcInvoker` | The logic is wrapped by a [`DataSvcInvoker`](../src/Beef.Core/Business/DataSvcInvoker.cs). This enables the [`BusinessInvokerArgs`](../src/Beef.Core/Business/BusinessInvokerBase.cs) options to be specified, including [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`OnImplementation` | Invocation of a named `XxxxxOnImplementaionAsync` method that must be implemented in a non-generated partial class.

The following demonstrates the generated code:

``` csharp
public Task<int> DataSvcCustomAsync()
{
    return DataSvcInvoker.Current.InvokeAsync(typeof(PersonDataSvc), async () => 
    {
        var __result = await DataSvcCustomOnImplementationAsync().ConfigureAwait(false);
        return __result;
    });
}
```
