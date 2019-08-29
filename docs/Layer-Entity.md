# Entity (DTO)

The *Entity* ([DTO](https://en.wikipedia.org/wiki/Data_transfer_object)) is primarily responsible for defining the domain-based data model (and contract where accessible externally over-the-wire). They are also used to define the [reference data](./Reference-Data.md) model.

The aim here is decouple this definition, where applicable, from the underlying data source. This is likely to be defined differently, with an alternate naming conversion, alternate shape/structure, etc. This also enables the data source to evolve independently of this model, as well as possibly hide additional implementaion details.

<br/>

## Capabilities

The key _Beef_ entity capabilities are enabled primarily by the [`Beef.Entities`](../src/Beef.Core/README.md#beefentities-namespace) namespace.

A [code-generated](../tools/Beef.CodeGen.Core/Readme.md) entity will get the following capabilities by default:
- Inherit from [EntityBase](../src/Beef.Core/Entities/EntityBase.cs) or [ReferenceDataBase](../src/Beef.Core/RefData/ReferenceDataBase.cs)
- Support for [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged).
- Support for [IEditableObject](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.ieditableobject).
- Support for [ICloneable](../src/Beef.Core/Entities/ICloneable.cs).
- Support for [ICopyFrom](../src/Beef.Core/Entities/ICopyFrom.cs).
- Support for [ICleanUp](../src/Beef.Core/Entities/ICleanUp.cs).
- Support for [IUniqueKey](../src/Beef.Core/Entities/IUniqueKey.cs).
- Corresponding collection inheriting from [EntityBaseCollection](../src/Beef.Core/Entities/EntityBaseCollection.cs) or [ReferenceDataCollectionBase](../src/Beef.Core/RefData/ReferenceDataCollectionBase.cs)

Other optional interfaces can also be implemented:
- Support for [IIdentifier](../src/Beef.Core/Entities/IIdentifier.cs)
- Support for [IETag](../src/Beef.Core/Entities/IETag.cs).
- Support for [IChangeLog](../src/Beef.Core/Entities/IChangeLog.cs).

<br/>

## Usage

The [`Entity`](./Entity-Entity-element.md) and its corresponding [`Property`](./Entity-Property-element.md) elements within the `entity.xml` configuration primarily drives the output. There is a generated class per [`Entity`](./Entity-Entity-element.md) with the same name.

The sample [`Person`](../samples/Demo/Beef.Demo.Common/Entities/Generated/Person.cs) demonstrates the richness of the generated output.