# Beef.CodeGen.Core

This tooling console application assembly contains the _Beef_ code generation capabilities.

Code generation is proposed as a means to greatly accelerate application development where standard coding patterns can be determined and the opportunity to automate exists.

Code generation is intended to bring the following benefits:
- **Acceleration** of development;
- **Consistency** of approach;
- **Simplification** of implementation;
- **Reusability** of layering and framework;
- **Evolution** of approach over time;
- **Richness** of capabilities with limited effort.

There are generally two types of code generation:
- **Gen-many** - the ability to consistently generate a code artefact multiple times over its lifetime without unintended breaking side-effects. Considered non-maintainable by the likes of developers as contents may change at any time; however, can offer extensible hooks to enable custom code injection where applicable.
- **Gen-once** - the ability to generate a code artefact once to effectively start the development process. Considered maintainable, and should not be re-generated as this would override custom coded changes.

_Beef_ primarily leverages **Gen-many** as this offers the greatest long-term benefits.

<br/>

## Supported code-gen

Both [entity-driven](#Entity-driven-code-gen) and [database-driven](#Database-driven-code-gen) are supported; similar to the following image.

![CodeGen](../../docs/images/CodeGen.png)

<br/>

## Composition

The base code-generation tooling is enabled by [`OnRamp`](https://github.com/Avanade/OnRamp); the _OnRamp_ code-generation tooling is composed of the following:

1. [Configuration](#Configuration) - data used as the input for to drive the code-generation and the underlying _templates_.
2. [Templates](#Templates) - [Handlebars](https://handlebarsjs.com/guide/) templates that define a specific artefact's scripted content.
3. [Scripts](#Scripts) - orchestrates one or more _templates_ that are used to generate artefacts for a given _configuration_ input.

<br/>

### Configuration

The code-gen is driven by a configuration data source, in this case YAML or JSON.

The two supported configurations are:
- [Entity-driven](#Entity-driven-code-gen) - the configuration root definition is [`Entity.CodeGenConfig`](./Config/Entity/CodeGenConfig.cs). All the related types are found [here](./Config/Entity).
- [Database-driven](#Database-driven-code-gen) - the configuration root definition is [`Database.CodeGenConfig`](./Config/Database/CodeGenConfig.cs). All the related types are found [here](./Config/Database).

<br/>

### Templates

The _Beef_ templates (as per [`OnRamp`](https://github.com/Avanade/OnRamp)) are defined using [Handlebars](https://handlebarsjs.com/guide/) and its syntax, or more specifically [Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net). 

The template files have been added as embedded resources within the project to enable runtime access. The _Beef_ standard templates can be found [here](./Templates).

<br/>

### Scripts

To orchestrate the code generation, in terms of the [Templates](#Templates) to be used, a YAML-based script-like file is used. The _Beef_ standard scripts can be found [here](./Scripts). The Script files have been added as embedded resources within the project to enable runtime access.

<br/>

## Entity-driven code-gen

The entity-driven gen-many code generation is enabled by an **Entity** configuration file that is responsible for defining the characteristics used by the code-gen tooling. The hierarchy is as follows:

```
CodeGeneration
└── Entity(s)
  └── Property(s)
  └── Const(s)
  └── Operation(s)
    └── Parameter(s)
```

Configuration details for each of the above are as follows:
- `CodeGeneration` - [YAML/JSON](../../docs/Entity-CodeGeneration-Config.md)
- `Entity` - [YAML/JSON](../../docs/Entity-Entity-Config.md)
- `Property` - [YAML/JSON](../../docs/Entity-Property-Config.md)
- `Const` - [YAML/JSON](../../docs/Entity-Const-Config.md)
- `Operation` - [YAML/JSON](../../docs/Entity-Operation-Config.md)
- `Parameter` - [YAML/JSON](../../docs/Entity-Parameter-Config.md)

The Entity configuration supported filenames are, in the order in which they are searched by the code generator: `entity.beef-5.yaml` and `entity.beef-5.json`.

The Entity configuration is defined by a schema, YAML/JSON-based [entity.beef.json](../../tools/Beef.CodeGen.Core/Schema/entity.beef.json). This schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

There are two additional configuration files that share the same schema:
- [Reference Data](./../../docs/Reference-Data.md) - used to define (configure) the _Beef_-specific Reference Data. Supported filenames are in the order in which they are searched by the code generator: `refdata.beef-5.yaml` and `refdata.beef-5.json`.
- Data Model - used to define (configure) basic data model .NET classes typically used to represent internal/backend contracts that do not require the full funcionality of a _Beef_ entity. Supported filenames are in the order in which they are searched by the code generator: `datamodel.beef-5.yaml` and `datamodel.beef-5.json`.

<br/>

## Database-driven code-gen

The database-driven code generation is enabled by a **Database** configuration file that is responsible for defining the characteristics used by the code-gen tooling. The hierarcy is as follows:

```
CodeGeneration
└── Query(s)
  └── QueryJoin(s)
    └── QueryJoinOn(s)
  └── QueryWhere(s)
  └── QueryOrder(s)
└── Table(s)
  └── StoredProcedure(s)
    └── Parameter(s)
    └── Where(s)
    └── OrderBy(s)
    └── Execute(s)
  └── Relationship(s)
```

Configuration details for each of the above are as follows:
- CodeGeneration - [YAML/JSON](../../docs/Database-CodeGeneration-Config.md)
- Query - [YAML/JSON](../../docs/Database-Query-Config.md)
- QueryJoin - [YAML/JSON](../../docs/Database-QueryJoin-Config.md)
- QueryJoinOn - [YAML/JSON](../../docs/Database-QueryJoinOn-Config.md)
- QueryWhere - [YAML/JSON](../../docs/Database-QueryWhere-Config.md)
- QueryOrder - [YAML/JSON](../../docs/Database-QueryOrder-Config.md)
- Table - [YAML/JSON](../../docs/Database-Table-Config.md)
- StoredProcedure - [YAML/JSON](../../docs/Database-StoredProcedure-Config.md)
- Parameter - [YAML/JSON](../../docs/Database-Parameter-Config.md)
- Where - [YAML/JSON](../../docs/Database-Where-Config.md)
- OrderBy - [YAML/JSON](../../docs/Database-OrderBy-Config.md)
- Execute - [YAML/JSON](../../docs/Database-Execute-Config.md)
- Relationship (EF) - [YAML/JSON](../../docs/Database-Relationship-Config.md)


The Database configuration supported filenames are, in the order in which they are searched by the code generator: `database.beef-5.yaml` and `database.beef-5.json`.

The Database configuration is defined by a schema, YAML/JSON-based [database.beef-5.json](../../tools/Beef.CodeGen.Core/Schema/database.beef-5.json). The schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

Finally, this is not intended as an all purpose database schema generation capability. It is expected that the tables pre-exist within the database. The database schema/table catalog information is queried from the database directly during code generation, to be additive to the configuration, to minimise the need to replicate (duplicate) column configuration and require on-going synchronization.

<br/>

## Console application

The `Beef.CodeGen.Core` can be executed as a console application directly; however, the experience has been optimized so that a new console application can reference and inherit the capabilities.

Additionally, the `Database` related code-generation can be (preferred method) enabled using [`Beef.Database.Core`](./../Beef.Database.Core/README.md). This will internally execute `Beef.CodeGen.Core` to perform the code-generation task as required.

<br/>

### Commands

The following commands are available for the console application (the enablement of each can be overridden within the program logic).

Command | Description
-|-
`Entity` | Performs code generation using the _entity_ configuration and [`EntityWebApiCoreAgent.yaml`](./Scripts/EntityWebApiCoreAgent.yaml) script.
`RefData` | Performs code generation using the _refdata_ configuration and [`RefDataCoreCrud.yaml`](./Scripts/RefDataCoreCrud.yaml) script.
`Database` | Performs code generation using the _database_ configuration and [`Database.yaml`](../Beef.Database.SqlServer/Scripts/Database.yaml) script.
`DataModel` | Performs code generation using the _data model_ configuration and [`DataModelOnly.yaml`](./Scripts/DataModelOnly.yaml) script.
`All` | Performs all of the above (where each is supported as per set up).

Additionally, there are a number of command line options that can be used.

```
Beef.CodeGen.Core Code Generation Tool.

Usage: Beef.CodeGen.Core [options] <command>

Arguments:
  command                   Execution command type.
                            Allowed values are: Entity, Database, RefData, DataModel, All, Clean, Count.

Options:
  -?|-h|--help              Show help information.
  -s|--script               Script orchestration file/resource name.
  -c|--config               Configuration data file name.
  -o|--output               Output directory path.
  -a|--assembly             Assembly containing embedded resources (multiple can be specified in probing order).
  -p|--param                Parameter expressed as a 'Name=Value' pair (multiple can be specified).
  -cs|--connection-string   Database connection string.
  -cv|--connection-varname  Database connection string environment variable name.
  -enc|--expect-no-changes  Indicates to expect _no_ changes in the artefact output (e.g. error within build pipeline).
  -sim|--simulation         Indicates whether the code-generation is a simulation (i.e. does not create/update any artefacts).

Extended commands and argument(s):
  clean   Cleans (removes) all related directories named 'Generated'.
          - Use --param exclude=name[,name] to exclude named directory(s) from the clean.

  count   Counts and reports the number of files and lines (All and Generated) within all related directories.
          - Use --param exclude=name[,name] to exclude named directory(s) from the count.
```

<br/>

### Program.cs

The `Program.cs` for the new console application should be updated similar to the following. The `Company` and `AppName` values are specified, as well as optionally indicating whether the `Entity`, `RefData`, `DataModel` and/or `Database` commands are supported.

``` csharp
public class Program
{
    static Task<int> Main(string[] args) => CodeGenConsole
        .Create("Company", "AppName")           // Create the Console setting Company and AppName.
        .Supports(entity: true, refData: true)  // Set which of the Commands are supported.
        .RunAsync(args);                        // Run the console.
}
```

<br/>

To run the console application, simply specify the required command; e.g:
```
dotnet run entity      -- Default filename: entity.beef-5.yaml
dotnet run refdata     -- Default filename: refdata.beef-5.yaml
dotnet run datamodel   -- Default filename: datamodel.beef-5.yaml
dotnet run all         -- All of the above (that are supported)

-- Override the configuration filename.
dotnet run entity --configFile configfilename.xml
```

</br>

## Personalization and/or overriding

As described above _Beef_ has a set of pre-defined (out-of-the-box) [Scripts](#Scripts) and [Templates](#Templates). These do not have to be used, or additional can be added, where an alternate code-generation outcome is required.

To avoid the need to clone the solution and update, add the `Templates` and `Scripts` folders into the new console application and embed the required resources. The underlying `Beef.CodeGen.Core` will probe the embedded resources and use the overridden version where provided, falling back on the _Beef_ version where not overridden. 

One or more of the following options exist to enable personalization.

Option | Description
-|-
[Config](#Config) | There is currently _no_ means to extend the underlying configuration .NET types directly. However, as all the configuration types inherit from [`ConfigBase`](https://github.com/Avanade/OnRamp/blob/main/src/OnRamp/Config/ConfigBaseT.cs) the `ExtraProperties` hash table is populated with any additional configurations during the deserialization process. These values can then be referenced direcly within the Templates as required. To perform further changes to the configuration at runtime an [`IConfigEditor`](https://github.com/Avanade/OnRamp/blob/main/src/OnRamp/Config/IConfigEditor.cs) can be added and then referenced from within the corresponding `Scripts` file; it will then be invoked prior to the code generation enabling further changes to occur. The `ConfigBase.CustomProperties` hash table is further provided to enable additional properties to be set and referenced in a consistent manner.
[Templates](#Templates) | Add new [Handlebars](https://handlebarsjs.com/guide/) file, as an embedded resource, to the `Templates` folder (add where not pre-existing) within the project. Where overriding use the same name as that provided out-of-the-box; otherwise, ensure the `Template` is referenced by the `Script`.
[Scripts](#Scripts) | Add new `Scripts` YAML file, as an embedded resource, to the `Scripts` folder (add where not pre-existing) within the project. Use the `Inherits` attribute where still wanting to execute the out-of-the-box code-generation.

<br/>

### Example

The [`Beef.Demo.Codegen`](./../../samples/Demo/Beef.Demo.Codegen) provides an example (tests the capability) of the implementation.

Code | Description
-|-
[`TestConfigEditor.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Config/TestConfigEditor.cs) | This implements [`IConfigEditor`](https://github.com/Avanade/OnRamp/blob/main/src/OnRamp/Config/IConfigEditor.cs) and demonstrates `ConfigBase.TryGetExtraProperty` and `ConfigBase.CustomProperties` usage.
[`TestCodeGenerator.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Generators/TestCodeGenerator.cs) | This inherits from [`CodeGeneratorBase<TRootConfig, TGenConfig>`](https://github.com/Avanade/OnRamp/blob/main/src/OnRamp/Generators/CodeGeneratorBaseT2.cs) overriding the `SelectGenConfig` to select the configuration that will be used by the associated Template.
[`Test_cs.hbs`](./../../samples/Demo/Beef.Demo.CodeGen/Templates/Test_cs.hbs) | This demonstrates how to reference both the `ExtraProperties` and `CustomProperties` using _Handlebars_ syntax. This file must be added as an embedded resource.
[`TestScript.yaml`](./../../samples/Demo/Beef.Demo.CodeGen/Scripts/TestScript.yaml) | This demonstrates the required configuration to wire-up the previous so that they are leveraged apprpropriately at runtime. This file must be added as an embedded resource.

Finally the [`Program.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Program.cs) will need to be updated similar as follows to use the new Scripts resource.

``` csharp
return CodeGenConsoleWrapper
    .Create("Beef", "Demo")
    .Supports(entity: true, refData: true, dataModel: true)
    .EntityScript("TestScript.yaml")   // <- Overrides the Script name.
    .RunAsync(args);
```
