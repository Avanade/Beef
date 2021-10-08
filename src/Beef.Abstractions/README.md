# Beef.Abstractions

This is the primary Assembly that provides the _core fundamental_ capabilities of _Beef_. The key classes and/or underlying NameSpaces are desribed.

<br/>

## Beef (root) namespace

This is the base set of capabilities (classes) available within _Beef_; the following are of specific note:

<br/>

### ExecutionContext

The [`ExecutionContext`](./ExecutionContext.cs) is a foundational class that is integral to the underlying execution. It represents a thread-bound (request) execution context - enabling the availability of the likes of `Username` at anytime. The context is passed between executing threads for the owning request.

An implementor may choose to inherit from this class and add additional capabilities as required.

<br/>

### IBusinessException

There are a number of key exceptions that have a specific built in behaviour; these all implement [`IBusinessException`](./IBusinessException.cs).

Exception | Description | HTTP Status | [`ErrorType`](./ErrorType.cs) | SQL
-|-|-|-|-
[`AuthenticationException`](./AuthenticationException.cs) | Represents an **Authentication** exception. | 401 Unauthorized | 8 AuthenticationError | n/a
[`AuthorizationException`](./AuthorizationException.cs) | Represents an **Authorization** exception. | 403 Forbidden | 3 AuthorizationError | [`spThrowAuthorizationException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowAuthorizationException.sql)
[`BusinessException`](./BusinessException.cs) | Represents a **Business** exception whereby the message returned should be displayed directly to the consumer. | 400 BadRequest | 2 BusinessError | [`spThrowBusinessException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowBusinessException.sql)
[`ConcurrencyException`](./ConcurrencyException.cs) | Represents a data **Concurrency** exception; generally as a result of an errant [ETag](./Entities/IETag.cs). | 412 PreconditionFailed | 4 ConcurrencyError | [`spThrowConcurrencyException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowConcurrencyException.sql)
[`ConflictException`](./ConflictException.cs) | Represents a data **Conflict** exception; for example creating an entity that already exists. | 409 Conflict | 6 ConflictError | [`spThrowConflictException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowConflictException.sql)
[`DuplicateException`](./DuplicateException.cs) | Represents a **Duplicate** exception; for example updating a code on an entity where the value is already used. | 409 Conflict | 7 DuplicateError | [`spThrowDuplicateException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowDuplicateException.sql)
[`NotFoundException`](./NotFoundException.cs) | Represents a **NotFound** exception; for example updating a code on an entity where the value is already used. | 404 NotFound | 5 NotFoundError | [`spThrowNotFoundException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowNotFoundException.sql)
[`ValidationException`](./ValidationException.cs) | Represents a **Validation** exception with a corresponding `Messages` [collection](./Entities/MessageItemCollection.cs). | 400 BadRequest | 1 ValidationError | [`spThrowValidationException`](../../tools/Beef.Database.Core/Schema/dbo/Stored&#32;Procedures/spThrowValidationException.sql)
 
<br/>


### LText and TextProvider

The [`LText`](./LText.cs) represents a *localization text* key/identifier to be used by the [`TextProvider`](./TextProvider.cs) to access the underlying localized text representation. This is baked into _Beef_ whereever end user texts are intended to be output.

<br/>

### InvokerBase

The [`InvokerBase`](./InvokerBase.cs) represents the base class that enables an `Invoke` to be wrapped so that standard functionality can be added to all invocations. For example: Retry, Circuit Breaker, Exception Handling, Database Transactions, etc.

_Beef_ uses this extensively within the solution layering and other componenents such as data access to enable this capability to be easily injected into the execution pipeline. 

<br/>

## Beef.Entities namespace

Provides the key capabilities to enable the rich _business entity_ functionality central to _Beef_.

### EntityBasicBase

The [`EntityBasicBase`](./Entities/EntityBasicBase.cs) provides the basic entity capabilities:
- Standardised `SetValue` methods to enable capabilities such as [`StringTrim`](./Entities/StringTrim.cs), [`StringTransform`](./Entities/StringTransform.cs), [`DateTimeTransform`](./Entities/DateTimeTransform.cs).
- Property immutability support; i.e. value can not be changed once set.
- Entity readonly support (`MakeReadOnly` and `IsReadOnly`).
- Entity changed support (`IsChanged` and `AcceptChanges`).
- Implements [`INotifyPropertyChanged`](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged) for `PropertyChanged` event.

<br/>

### EntityBase

The [`EntityBase`](./Entities/EntityBase.cs) provides the rich entity capabilities (inherits from `EntityBasicBase`):
- Implements [`IEditableObject`](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.ieditableobject) for `BeginEdit`, `EndEdit` and `CancelEdit`.
- Implements [`IEquatable`](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1) for `Equals` and `GetHashCode`. 
- Implements [`ICleanUp`](./Entities/ICleanUp.cs) for `CleanUp` and `IsInitial`.
- Implements [`IUniqueKey`](./Entities/ICleanUp.cs) for `UniqueKey`.
- Implements [`IChangeTrackingLogging`](./Entities/IChangeTrackingLogging.cs) for `TrackChanges` and `ChangeTracking`.
- Implements [`ICopyFrom`](./Entities/ICopyFrom.cs) and [`ICloneable`](./Entities/ICloneable.cs) for `CopyFrom` and `Clone` respectively.

_Note_: The entity code-generation will ensure that the `EntityBase` and corresponding capabilities, specifically the `SetValue`, are implemented correctly. 

<br/>

### EntityBaseCollection

The [`EntityBaseCollection`](./Entities/EntityBaseCollection.cs) encapsulates an [`ObservableCollection<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1) to provide the base class for all entity collections enabling a consistent and rich experience.

<br/>

### EntityCollectionResult

The [`EntityCollectionResult`](./Entities/EntityCollectionResult.cs) provides a standardised mechanism to manage a collection `Result`, that may also contain corresponding [`Paging`](./Entities/PagingResult.cs) details where required.

<br/>

### MessageItem and MessageItemCollection

The [`MessageItem`](./Entities/MessageItem.cs) and [`MessageItemCollection`](./Entities/MessageItemCollection.cs) provide a means to manage messages ([`Type`](./Entities/MessageType.cs), `Text` and optional `Property`) used for the likes of validation error messages, etc.

<br/>

### ICleanUp and Cleaner

The [`ICleanUp`](./Entities/ICleanUp.cs) and [`Cleaner`](./Entities/Cleaner.cs) provide a means to clean up / reset the properties of an entity in a consistent manner, and / or return an entity to a consistent state.

<br/>

### IUniqueKey and UniqueKey

The [`IUniqueKey`](./Entities/IUniqueKey.cs) and [`UniqueKey`](./Entities/UniqueKey.cs) provide a means to define a unique key (composed of one or more properties) for an entity. This allows the `UniqueKey` for an entity to be accessed in a consistent manner that is leveraged by other capabilities within the _Beef_ framework.

<br/>

### Paging, IPagingResult and PagingResult

The [`PagingArgs`](./Entities/PagingArgs.cs), [`IPagingResult`](./Entities/IPagingResult.cs) and [`PagingResult`](./Entities/PagingResult.cs) represents the key capabilities to support paging requests and corresponding response in a consistent manner.

<br/>

## RefData namespace

Provides the key capabilities to enable the rich _reference data_ functionality central to _Beef_. This capability is further described [here](../../docs/Reference-Data.md).

<br/>

## WebApi namespace

Provides the [Web API Agent](../../docs/Layer-ServiceAgent.md) capabilities to standardize the invocation of APIs. The [`WebApiAgentBase`](./WebApi/WebApiAgentBase.cs) is essential to enable.