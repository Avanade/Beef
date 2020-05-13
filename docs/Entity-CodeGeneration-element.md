﻿# 'CodeGeneration' element (entity-driven)

The **`CodeGeneration`** element defines global attributes that are used to drive the underlying entity-driven code generation. 

An example is as follows:

```xml
<CodeGeneration xmlns="http://schemas.beef.com/codegen/2015/01/entity" RefDataNamespace="Beef.Demo.Common.Entities">
```

<br>

## Attributes

The **`CodeGeneration`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes:

Attribute | Description
-|-
**`RefDataNamespace`** | The namespace for the reference data entities (adds using statement).
**`ValidatorLayer`** | Defines the layer namespace where the Validator's are defined. Options are: `Common` (defined within the common namespace/assembly), or `Business` (defined within the business namespace/assembly). The default is `Business`.
**`WebApiAuthorize`** | Indicates whether the Web API controller should use the `Authorize` or `AllowAnonynous` (default) attribute. This can be overridden within the **`Entity`** and **`Operation`** elements.
`EventPublish` | Indicates whether to add logic to publish an event on the successful completion of the `DataSvc` layer invocation for a Create, Update or Delete. Defaults to `true`. Used to enable the sending of messages to the likes of EventHub, EventGrid, Service Broker, SignalR, etc.
`EventActionFormat` | Defines the format for the Action when an Event is published. Options are: `UpperCase`, `PastTense`, and `PastTenseUpperCase`. Defaults to `null` (no formatting).
`EntityUsing` | The namespace for the non reference data entities (adds using statements). Options are: `Common` to add `.Common.Entities` (default), `Business` to add `.Business.Entities`, `All` to add both, and `None` to exclude.
`RefDataBusNamespace` | The namespace to be used for the reference data entities where the EntityScope is `Business` (adds using statement). Only used for the business namespace/assembly.
`MapperDefaultRefDataConverter` | Specifies the default Reference Data property `Converter` used by the generated `Mapper(s)` where not specifically defined. Options are: `ReferenceDataInt32IdConverter` (default),  `ReferenceDataNullableInt32IdConverter`, `ReferenceDataCodeConverter`, `ReferenceDataGuidIdConverter` or `ReferenceDataNullableGuidIdConverter`. 
`JsonSerializer` | Specifies the default JSON Serializer to use for JSON property attribution. Options are `None` or `Newtonsoft`. Defaults to `Newtonsoft`. Can be overridden (specified) per `Entity`.

<br>

### Data attributes

The following represents the **Data** attributes:

Attribute | Description
-|-
DatabaseName | Provides the default database instance name (where `AutoImplement` is `'Database'`); defaults to `'Database'`.
EntityFrameworkName | Provides the default Entity Framework instance name (where `AutoImplement` is `'EntityFramework'`); defaults to `'EfDb'`.
CosmosName | Provides the default Cosmos instance name (where `AutoImplement` is `'Cosmos'`); defaults to `'CosmosDb'`.
ODataName | Provides the default OData instance name (where `AutoImplement` is `'OData'`); defaults to `'OData'`.

<br/>

### Reference data attributes

The following represents the attributes when specifically generating **reference data** entities:

Attribute | Description
-|-
**`RefDataWebApiRoute`** | The top level reference data Web API route required for named pre-fetching.
`RefDataCache` | Defines the cache used for the ReferenceData providers. Options are: `ReferenceDataCache` (single-tenant cache), or `ReferenceDataMultiTenantCache` (multi-tenant cache). The default is `ReferenceDataCache`.
`RefDataText` | Indicates whether a corresponding *text* property is added when generating a reference data propety. This is generally only used where serializing within the `Controller` and the `ExecutionContext.IsRefDataTextSerializationEnabled` is set to `true` (automatically performed where url contains '$text=true').

<br/>

### Namespace attributes

The following represents optional **namespace** attributes:

Attribute | Description
-|-
`DataUsingNamespace` | Adds an additional Namespace using statement to the Data code.
`DatabaseUsingNamespace` | Adds an additional Namespace using statement to the Data code where using AutoImplement of `Database`.
`EntityFrameworkUsingNamespace` | Adds an additional Namespace using statement to the Data code where using AutoImplement of `EntityFramework`.
`ODataUsingNamespace` | Adds an additional Namespace using statement to the Data code where using AutoImplement of `OData`.
`CosmosUsingNamespace` | Adds an additional Namespace using statement to the Data code where using AutoImplement of `Cosmos`.
`UsingNamespace1` | Adds an additional Namespace using statement to the Entity code.
`UsingNamespace2` | Adds an additional Namespace using statement to the Entity code.
`UsingNamespace3` | Adds an additional Namespace using statement to the Entity code.
`AppendToNamespace` | The name of the entity namespace appended to end of the standard `company.appname.Common.Entities` output (defaults to `null`; being nothing to append).

<br/>

### gRPC attributes

The following represents optional **[gRPC](../src/Beef.Grpc/README.md)** attributes:

Attribute | Description
-|-
`Grpc` | Indicates whether _gRPC_ support is required; must be set to `true` for any of the subordinate _gRPC_ capabilities to be code-generated. Will require each [`Entity`](./Entity-Entity-element.md) and corresponding [`Property`](./Entity-Property-element.md) and [`Operation`](./Entity-Operation-element.md) to be opted-in specifically.
