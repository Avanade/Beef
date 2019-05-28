# `Beef.Core`

This is the foundational assembly that provides the core capabilities of _Beef_.

<br/>

## Core

This is the base set of capabilities available within _Beef_; the following are of specific note:

<br/>

### ExecutionContext

This [`ExecutionContext`](./ExecutionContext.cs) a foundational class that is integral to the underlying execution. It represents a thread-bound (request) execution context - enabling the availability of the likes of `Username` at anytime. The context is passed between executing threads to ensure concurrency.

<br/>

### IBusinessException

There are a number of key exceptions that have a specific built in behaviour; these all implement [`IBusinessException`](./IBusinessException.cs).

Exception | Description | HTTP Status | [`ErrorType`](./ErrorType.cs) | SQL
-|-|-|-|-
[`AuthorizationException`](./AuthorizationException.cs) | Represents an **Authorization** exception. | 403 Forbidden | 3 AuthorizationError | [`spThrowAuthorizationException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowAuthorizationException.sql)
[`BusinessException`](./BusinessException.cs) | Represents a **Business** exception whereby the message returned should be displayed directly to the consumer. | 400 BadRequest | 2 BusinessError | [`spThrowBusinessException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowBusinessException.sql)
[`ConcurrencyException`](./ConcurrencyException.cs) | Represents a data **Concurrency** exception; generally as a result of an errant [ETag](./Entities/IETag.cs). | 412 PreconditionFailed | 4 ConcurrencyError | [`spThrowConcurrencyException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowConcurrencyException.sql)
[`ConflictException`](./ConflictException.cs) | Represents a data **Conflict** exception; for example creating an entity that already exists. | 409 Conflict | 6 ConflictError | [`spThrowConflictException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowConflictException.sql)
[`DuplicateException`](./DuplicateException.cs) | Represents a **Duplicate** exception; for example updating a code on an entity where the value is already used. | 409 Conflict | 7 DuplicateError | [`spThrowDuplicateException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowDuplicateException.sql)
[`NotFoundException`](./NotFoundException.cs) | Represents a **NotFound** exception; for example updating a code on an entity where the value is already used. | 404 NotFound | 5 NotFoundError | [`spThrowNotFoundException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowNotFoundException.sql)
[`ValidationException`](./ValidationException.cs) | Represents a **Validation** exception with a corresponding `Messages` [collection](./Entities/MessageItemCollection.cs). | 400 BadRequest | 1 ValidationError | [`spThrowValidationException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowValidationException.sql)
 
<br/>

### DataContextScope

### LText

### Factory

## Business

## Caching


<br/>

-----

It is composed of the following key features:

Namespace | Description
-|-
~ | Core or common capabilities, such as `ExecutionContext`, `IBusinessException`, `Factory`, and `DataContextScope`.
`Business` | _Business_ tier components; specifically the invokers (see `BusinessInvokerBase`). 
`Caching` | In-memory cache capabilities with associated policies to periodically flush.
`CodeGen` | Core _Code Generator_ capabilities used by all tooling.
`Diagnostics` | Basic diagnostics such as the shared `Logger` and `PerformanceTimer`.
`Entities` | Provides the key capabilities to enable the rich _business entity_ functionality central to _Beef_.
`Events` | Provides basic infrastucture to support a basic _event-driven_ architecture, through `Event` and `EventData`. 
`Executors` | Execution, and corresponding trigger orchestration, to standardise the processing of long-running, batch-style, operations.
`FlatFile` | Provides a rich framework for reading and writing fixed, and delimited, flat files.
`Json` | Additional capabilities to process JSON, such as `JsonEntityMerge` and `JsonPropertyFilter`.
`Mapper` | Provides the base, and entity-to-entity, class and property mapping central to _Beef_.
`Net` | Additional `HTTP` capabilities.
`RefData` | Provides the key capabilities to enable the rich _business reference data_ functionality central to _Beef_.
`Reflection` | Additional reflection capabilities leveraged primarily by the _Beef_ framework.
`Strings` | Embedded string resources used by _Beef_.
`Validation` | Provides a rich, fluent-style, validation framework.
`WebApi` | Provides additional capabilities to standardize the consumption of Web APIs.