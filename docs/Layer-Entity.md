# Entity (DTO)

The *Entity* ([DTO](https://en.wikipedia.org/wiki/Data_transfer_object)) is primarily responsible for defining the domain-based model (and contract where accessible externally over-the-wire). They are also used to define the [reference data](./Reference-Data.md) model.

The aim here is decouple this definition, where applicable, from the underlying data source. This is likely to be defined differently, with an alternate naming convention, alternate shape/structure, etc. This also enables the data source to evolve independently of this model, as well as possibly hide additional implementaion details.

_Beef_ looks to define two versions of the entity, the `Common` (external) and `Business` (internal) representations. This allows additional features to be leveraged internally that need not be exposed externally. 

<br/>

## Capabilities

The key _Beef_ entity capabilities are enabled primarily by the [`CoreEx.Entities`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Entities) and [`CoreEx.Entities.Extended`](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx/Entities.Extended) namespaces.

<br/>

### Business (internal)

A [code-generated](../tools/Beef.CodeGen.Core/Readme.md) `Business` (internal) entity will get the following capabilities:

- Inherit from [CoreEx.Entities.Extended.EntityBase](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/Extended/EntityBase.cs) or [CoreEx.RefData.Extended.ReferenceDataBaseEx](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/Extended/ReferenceDataBaseEx.cs)
- Support for [System.ComponentModel.INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged).
- Support for [System.ComponentModel.IChangeTracking](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.ichangetracking).
- Support for [CoreEx.Entities.IReadOnly](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IReadOnly.cs).
- Support for [CoreEx.Entities.ICopyFrom](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/ICopyFrom.cs).
- Support for [CoreEx.Entities.ICleanUp](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/ICleanUp.cs).
- Support for [CoreEx.Entities.IInitial](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IInitial.cs).
- Support for [Clone](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/Extended/ExtendedExtensions.cs) extension method.
- Collection inheriting from [CoreEx.Entities.Extended.EntityBaseCollection](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/Extended/EntityBaseCollection.cs) or [CoreEx.RefData.ReferenceDataCollectionBase](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/RefData/ReferenceDataCollectionBase.cs)
- Collection result inheriting from [CoreEx.Entities.CollectionResult](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/CollectionResult.cs).

Additional interfaces are automatically implemented where corresponding properties, etc. are specified:
- Support for [CoreEx.Entities.IIdentifier](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IIdentifierT.cs) or [CoreEx.Entities.IPrimaryKey](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IPrimaryKey.cs) (composite).
- Support for [CoreEx.Entities.IETag](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IETag.cs).
- Support for [CoreEx.Entities.Extended.IChangeLogEx](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/Extended/IChangeLogEx.cs).
- Support for [CoreEx.Entities.IPartitionKey](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IPartitionKey.cs).

The sample [`PerformanceReview`](../samples/My.Hr/My.Hr.Business/Entities/Generated/PerformanceReview.cs) entity demonstrates the richness of the generated output.

<br/>

### Common (external)

A [code-generated](../tools/Beef.CodeGen.Core/Readme.md) `Common` (external) entity will get the following interfaces automatically implemented where corresponding properties, etc. are specified:

- Support for [CoreEx.Entities.IIdentifier](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IIdentifierT.cs) or [CoreEx.Entities.IPrimaryKey](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IPrimaryKey.cs) (composite).
- Support for [CoreEx.Entities.IETag](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IETag.cs).
- Support for [CoreEx.Entities.IChangeLog](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IChangeLog.cs).
- Support for [CoreEx.Entities.IPartitionKey](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/Entities/IPartitionKey.cs).

The sample [`PerformanceReview`](../samples/My.Hr/My.Hr.Common/Entities/Generated/PerformanceReview.cs) entity demonstrates the simpler alternative of the generated output.

<br/>

## Usage

The [`Entity`](./Entity-Entity-Config.md) and its corresponding [`Property`](./Entity-Property-Config.md) elements within the `entity.beef-5.yaml` configuration primarily drives the output. There is a generated class per `Entity` with the same name.