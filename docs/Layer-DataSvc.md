# DataSvc (Service Orchestration)

The _DataSvc_ is primarily responsible for orchestrating the underlying data access; whilst often one-to-one there may be times that this class will be used to coordinate multiple data access components. This layer is responsible for ensuring that the related `Entity` is for the most part fully constructed/updated/etc. as per the desired operation.

<br/>

## Execution caching

To improve potential performance, and reduce chattiness, within an in-process execution context the `DataSvc` introduces a level of caching (short-lived); this can be turned off where not required. The cache is managed using the [`IRequestCache`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Caching/IRequestCache.cs).

The purpose of this cache is to minimise this chattiness to the underlying data source, to reduce this cost, where the time between calls (measured in nanoseconds/milliseconds) is such that the data retrieved previously is considered sufficient/valid. This way within an execution context (request) lifetime a developer can invoke the `XxxDataSvc` multiple times with only a single data source cost reducing the need to cache (and pass around) themselves.

This logic of getting, setting and clearing the cache is included within the primary `Get`, `Create`, `Update` and `Delete` operations only. Other operations will need to be reviewed and added accordingly (manually).

<br>

## Event-driven

To support the goals of an [Event-driven architecture](https://en.wikipedia.org/wiki/Event-driven_architecture) an event publish can be included.

An [`EventData`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/EventData.cs) publish is invoked where the eventing infrastructure has been included (configured) during [code-generation](./tools/Beef.CodeGen.Core). The [`IEventPublisher`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Events/IEventPublisher.cs) implementation is responsible for orchestraing the publishing and sending of the event message(s). 

_Note:_ This is _always_ performed directly after the primary operation logic such that the _event_ is only published where successful. This is may not be transactional (depends on implementation) so if the event publish fails there may be no automatic rollback capabilitity. The implementor will need to decide the corrective action for this type of failure; i.e. consider the transaction outbox pattern.

<br/>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, or has extension opportunities to inject additional logic into the processing pipeline.

The [`Operation`](./Entity-Operation-Config.md) element within the `entity.beef-5.yaml` configuration primarily drives the output

There is a generated class per [`Entity`](./Entity-Entity-Config.md) named `{Entity}DataSvc`. There is also a corresonding interface named `I{Entity}DataSvc` generated so the likes of test mocking etc. can be employed. For example, if the entity is named `Person`, there will be corresponding `PersonDataSvc` and `IPersonDataSvc` classes.

<br/>

## Railway-oriented programming

_CoreEx_ version `3.0.0` introduced [monadic](https://en.wikipedia.org/wiki/Monad_(functional_programming)) error-handling, often referred to as [Railway-oriented programming](https://swlaschin.gitbooks.io/fsharpforfunandprofit/content/posts/recipe-part2.html). This is enabled via the key types of `Result` and `Result<T>`; please review the corresponding [documentation](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Results/README.md) for more detail on purpose and usage. 

The [`Result`]() and [`Result<T>`]() have been integrated into the code-generated output and is leveraged within the underlying validation. This is intended to simplify success and failure tracking, avoiding the need, and performance cost, in throwing resulting exceptions. 

This is implemented by default; however, can be disabled by setting the `useResult` attribute to `false` within the code-generation configuration.

<br/>

### Code-generated
 
An end-to-end code-generated processing pipeline generally consists of:

Step | Description
-|-
`DataSvcInvoker` | The logic is wrapped by a [`DataSvcInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/DataSvcInvoker.cs). This enables the [`InvokerArgs`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/InvokerArgs.cs) options to be specified, including [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration. This invocation will only be output where required, or alternatively explicitly specified.
`Cache` | Trys the cache and returns result where found (as applicable).
`Data` | The [`I{Entity}Data`](./Layer-Data.md) layer is invoked to perform the data processing.
`EventPublish` | Constructs the `EventData` and invokes the `Event.Publish`.
`Cache` | Performs a cache set or remove (as applicable).
`OnAfter`&dagger; | The `OnAfter` extension opportunity; where set this will be invoked. This enables logic to be invoked _after_ the primary `Operation` is performed.

_&dagger; Note:_ To minimize the generated code the extension opportunities are only generated where selected. This is performed by setting the `dataSvcExtensions` attribute to `true` within the [`Entity`](./Entity-Entity-Config.md) code-generation configuration.

The following demonstrates the generated code (a snippet from the sample [`RobotDataSvc`](../samples/Demo/Beef.Demo.Business/DataSvc/Generated/RobotDataSvc.cs)) that does not include `DataSvcExtensions`:

``` csharp
// A Get operation.
public Task<Result<Robot?>> GetAsync(Guid id) => Result.Go().CacheGetOrAddAsync(_cache, id, () => _data.GetAsync(id));

// A Create operation.
public Task<Result<Robot>> CreateAsync(Robot value) => DataSvcInvoker.Current.InvokeAsync(this, _ =>
{
    return Result.GoAsync(_data.CreateAsync(value))
                 .Then(r => _events.PublishValueEvent(r, new Uri($"/robots/{r.Id}", UriKind.Relative), $"Demo.Robot", "Create"))
                 .Then(r => _cache.SetValue(r));
}, new InvokerArgs { EventPublisher = _events });
```

The non-`Result` based version would be similar to:

``` csharp
// A Get operation.
public Task<Robot?> GetAsync(Guid id) => _cache.GetOrAddAsync(id, () => _data.GetAsync(id));

// A Create operation.
public Task<Robot> CreateAsync(Robot value) => DataSvcInvoker.Current.InvokeAsync(this, async _ =>
{
    var __result = await _data.CreateAsync(value ?? throw new ArgumentNullException(nameof(value))).ConfigureAwait(false);
    _events.PublishValueEvent(__result, new Uri($"/robots/{__result.Id}", UriKind.Relative), $"Demo.Robot", "Create");
    return _cache.SetValue(__result);
}, new InvokerArgs { EventPublisher = _events });
```

The following demonstrates the generated code (a snippet from the sample [`PersonDataSvc`](../samples/Demo/Beef.Demo.Business/DataSvc/Generated/PersonDataSvc.cs)) that includes `DataSvcExtensions`:

``` csharp
// A Get operation.
public async Task<Person?> GetExAsync(Guid id)
{
    if (_cache.TryGetValue(id, out Person? __val))
        return __val;

    var __result = await _data.GetExAsync(id).ConfigureAwait(false);
    await Invoker.InvokeAsync(_getExOnAfterAsync?.Invoke(__result, id)).ConfigureAwait(false);
    return _cache.SetValue(__result);
}

// A create operation.
public Task<Person> CreateAsync(Person value) => DataSvcInvoker.Current.InvokeAsync(this, async _ =>
{
    var __result = await _data.CreateAsync(value ?? throw new ArgumentNullException(nameof(value))).ConfigureAwait(false);
    await Invoker.InvokeAsync(_createOnAfterAsync?.Invoke(__result)).ConfigureAwait(false);
    _events.PublishValueEvent(__result, new Uri($"/person/{__result.Id}", UriKind.Relative), $"Demo.Person", "Create");
    return _cache.SetValue(__result);
}, new InvokerArgs { IncludeTransactionScope = true, EventPublisher = _events });
```

<br/>

### Custom

A custom (`OnImplementation`) processing pipeline generally consists of:

Step | Description
-|-
`DataSvcInvoker` | The logic is wrapped by a [`DataSvcInvoker`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/DataSvcInvoker.cs). This enables the [`InvokerArgs`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Invokers/InvokerArgs.cs) options to be specified, including [`TransactionScopeOption`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transactionscopeoption) and `Exception` handler. These values are generally specified in the code-generation configuration. This invocation will only be output where required, or alternatively explicitly specified.
`OnImplementation` | Invocation of a named `XxxOnImplementaionAsync` method that must be implemented in a non-generated partial class.

The following demonstrates the generated code:

``` csharp
public Task<Result<int>> DataSvcCustomAsync() => DataSvcCustomOnImplementationAsync();
```

The non-`Result` based version would be similar to:

``` csharp
public Task<int> DataSvcCustomAsync() => DataSvcCustomOnImplementationAsync();
```
