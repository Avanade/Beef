# Beef.Core

This is the secondary Assembly that provides the _core fundamental_ capabilities of _Beef_. The key classes and/or underlying NameSpaces are desribed.

<br/>

## Beef.Business namespace

This provides classes used specifically by the primary domain _business_ logic (see [`Solution Structure`](../../docs/solution-structure.md)).

<br/>

## Beef.Caching namespace

This provides for basic in-memory **Caching** capabilities (see [`CacheCoreBase`](./Caching/CacheCoreBase.cs)), and corresponding **Policy** to flush and/or refresh as required (managed by [`CachePolicyManager`](./Caching/Policy/CachePolicyManager.cs)).

The advantages of using memory caching is clearly performance; although, caution is required where the volume of data being cached is significant. Equally, where data must be expired at the same time across caches in the likes of a server farm. Alternates to consider are the likes of [Redis](https://redis.io/).

<br/>

## Events namespace

Provides the basic infrastructure support to support a basic _event-driven_ architecture, through [`EventPublisherBase`](./Events/EventPublisherBase.cs) and [`EventData`](./Events/EventData.cs).

<br/>

## Hosting namespace

Providea the [`TimerHostedServiceBase`](./Hosting/TimerHostedServiceBase.cs) to standardise the processing of long-running, batch-style, timer-based operations.

<br/>

## Json namespace

Additional capabilities to process JSON, such as [`JsonEntityMerge`](./Json/JsonEntityMerge.cs) and [`JsonPropertyFilter`](./Json/JsonPropertyFilter.cs).

<br/>

## Mapper namespace

Provides the base mapping type converters, and core capabilites for the database mapping. Otherwise, _Beef_ recommends using `AutoMapper`(https://automapper.org/), and the provided type converters are _AutoMapper_ compatible.

<br/>

## Net namespace

Additional `HTTP` capabilities, specifically [`HttpMultiPartRequestReader`](./Net/Http/HttpMultiPartRequestWriter.cs) and [`HttpMultiPartResponseReader`](./Net/Http/HttpMultiPartResponseReader.cs).

<br/>

## RefData namespace

Provides the underlying caching for the reference data. This capability is further described [here](../../docs/Reference-Data.md).

<br/>

## Validation namespace

Provides the key capabilities to enable the rich _validation_ functionality central to _Beef_. This capability is further described [here](../../docs/Beef-Validation.md).

<br/>

## WebApi namespace

Provides the _HTTP REST Agent_ capabilities to standardize the invocation of APIs as an optional data source. The [`HttpAgentBase`](./WebApi/HttpAgentBase.cs) is essential to enable.