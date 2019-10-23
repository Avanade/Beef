# DataSvc (Service Orchestration)

The _DataSvc_ is primarily responsible for orchestrating the underlying data access; whilst often one-to-one there may be times that this class will be used to coordinate multiple data access components. It is responsible for the ensuring that the related `Entity` is fully constructed/updated/etc. as per the desired operation.

<br/>

## Execution caching

To improve potential performance, and reduce chattiness, within an in-process execution context the `DataSvc` introduces a level of caching (short-lived). The cache is managed within the [`ExecutionContext`](../src/Beef.Core/ExecutionContext.cs) via the following: `TryGetCacheValue`, `CacheSet`, `CacheGet`, `CacheRemove`, `CacheClear` and `CacheClearAll`.

The purpose of this cache is to minimise this chattiness to the underlying data source, to reduce this cost, where the time between calls (measured in milliseconds) is such that the data retrieved previously is considered sufficient/valid. This way within an execution context a developer can invoke the `XxxDataSvc` multiple times with only a single data source cost.

This logic of getting, setting and clearing the cache is included within the primary `Get`, `Create`, `Update` and `Delete` operations only.

<br>

## Event-driven

To support the goals of an [Event-driven architecture](https://en.wikipedia.org/wiki/Event-driven_architecture) an event publish can be included.

An [`Event`](../src/Beef.Core/Events/Event.cs) publish is invoked where the eventing infrastructure has been included (configured) during [code-generation](./tools/Beef.CodeGen.Core).

_Note:_ The `Event.PublishAsync` does not automatically publish (send) the event out-of-the-box. The `Event.Register` must be used to register a function(s) to be invoked, that will in turn perform the work of sending the [`EventData`](../src/Beef.Core/Events/EventData.cs) message. 

_Note:_ This is _always_ performed directly after the primary operation logic such that the _event_ is only published where successful. This is **not** transactional so if the event publish fails there is no automatic rollback capabilitity. The implementor will need to decide the corrective action for the failure.

<br/>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, or has extension opportunities to inject additional logic into the processing pipeline.

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the output. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}DataSvc`.

### Code-generated
 
An end-to-end code-generated processing pipeline generally consists of:

Step | Description
-|-
`DataSvcInvoker` | The logic is wrapped by a [`DataSvcInvoker`](../src/Beef.Core/Business/DataSvcInvoker.cs). This enables the [`BusinessInvokerArgs`](../src/Beef.Core/Business/BusinessInvokerBase.cs) options to be specified, including [`DataContextScopeOption`](../src/Beef.Core/DataContextScopeOption.cs), [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`Cache` | Trys the cache and returns result where found (as applicable).
`Data` | The [`{Entity}Data`](./Layer-DataSvc.md) layer is invoked to orchestrate the data processing; this is instantiated by the [`Factory`](../src/Beef.Core/Factory.cs) (enables a test mocking opportunity).
`EventPublish` | Constructs the `EventData` and invokes the `Event.Publish`.
`Cache` | Performs a cache set or remove (as applicable).
`OnAfter` | The `OnAfter` extension opportunity; where set this will be invoked. This enables logic to be invoked _after_ the primary `Operation` is performed.

The following demonstrates the usage (a snippet from the sample [`PersonDataSvc`](../samples/Demo/Beef.Demo.Business/DataSvc/Generated/PersonDataSvc.cs)):

``` csharp
// A Get operation.
public static Task<Person> GetAsync(Guid id)
{
    return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
    {
        var __key = new UniqueKey(id);
        if (ExecutionContext.Current.TryGetCacheValue<Person>(__key, out Person __val))
            return __val;

        var __result = await Factory.Create<IPersonData>().GetAsync(id);
        ExecutionContext.Current.CacheSet<Person>(__key, __result);
        if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id);
        return __result;
    });
} 


// An Update operation.
public static Task<Person> UpdateAsync(Person value)
{
    return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
    {
        var __result = await Factory.Create<IPersonData>().UpdateAsync(value);
        await Beef.Events.Event.PublishAsync(__result, "Demo.Person.{id}", "Update", new KeyValuePair<string, object>("id", __result.Id));
        ExecutionContext.Current.CacheSet<Person>(__result?.UniqueKey ?? UniqueKey.Empty, __result);
        if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result);
        return __result;
    }, new BusinessInvokerArgs { IncludeTransactionScope = true });
}
```

<br/>

### Custom

A custom (`OnImplementation`) processing pipeline generally consists of:

Step | Description
-|-
`DataSvcInvoker` | The logic is wrapped by a [`DataSvcInvoker`](../src/Beef.Core/Business/DataSvcInvoker.cs). This enables the [`BusinessInvokerArgs`](../src/Beef.Core/Business/BusinessInvokerBase.cs) options to be specified, including [`DataContextScopeOption`](../src/Beef.Core/DataContextScopeOption.cs), [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration.
`Cache` | Trys the cache and returns result where found (as applicable).
`OnImplementation` | Invocation of a named `XxxxxOnImplementaionAsync` method that must be implemented in a non-generated partial class.
`EventPublish` | Constructs the `EventData` and invokes the `Event.Publish`.
`Cache` | Performs a cache set or remove (as applicable).
`OnAfter` | The `OnAfter` extension opportunity; where set this will be invoked. This enables logic to be invoked _after_ the primary `Operation` is performed.

The following demonstrates the usage:

``` csharp
public static Task<Person> UpdateAsync(Person value)
{
    return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
    {
        var __result = await UpdateOnImplementationAsync(value);
        await Beef.Events.Event.PublishAsync(__result, "Demo.Person.{id}", "Update", new KeyValuePair<string, object>("id", __result.Id));
        ExecutionContext.Current.CacheSet<Person>(__result?.UniqueKey ?? UniqueKey.Empty, __result);
        if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result);
        return __result;
    }, new BusinessInvokerArgs { IncludeTransactionScope = true });
}
```
