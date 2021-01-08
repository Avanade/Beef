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

The code-generation tooling is composed of the following:

- [Configuration](#Configuration) - this is the configuration that is used to both control the generated artefacts and their respective content.
- [Templates](#Templates) - these are the [Handlebars](https://handlebarsjs.com/guide/) templates that define a specific artefact's content.
- [Scripts](#Scripts) - these orchestrate the templates (one or more) that should be used to generate the artefacts.

<br/>

### Configuration

The code-gen is driven by a configuration data source, in this case YAML, JSON or XML. This acts as a type of DSL ([Domain Specific Language](https://en.wikipedia.org/wiki/Domain-specific_language)) to define the key characteristics / attributes that will be used to generate the required artefacts.

The two supported configurations are:
- [Entity-driven](#Entity-driven-code-gen) - the configuration root definition is [`Entity.CodeGenConfig`](./Config/Entity/CodeGenConfig.cs). All the related types are found [here](./Config/Entity).
- [Database-driven](#Database-driven-code-gen) - the configuration root definition is [`Database.CodeGenConfig`](./Config/Database/CodeGenConfig.cs). All the related types are found [here](./Config/Database).

The above root, and corresponding related (child) configurations must inherit from [`ConfigBase`](./Config/ConfigBase.cs) to get their required capabilities. The advantage of using a .NET typed class for the configuration is that additional properties (computed at runtime) can be added to aid the code-generation process. The underlying `Prepare` method provides a consistent means to implement this logic at runtime.

<br/>

### Templates

Once the code-gen data source(s) have been defined, one or more templates will be required to drive the artefact output. These templates are defined using [Handlebars](https://handlebarsjs.com/guide/) and its syntax, or more specifically [Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net). Template files must be added as an embedded resource within the solution to enable runtime access.

Additionally, Handlebars has been [extended](./Generators/HandlebarsHelpers.cs) to add additional capabilities beyond what is available natively to enable the required generated output.

The _Beef_ standard templates can be found [here](./Templates).

<br/>

### Scripts

To orchestrate the code generation, in terms of the [Templates](#Templates) to be used, an XML-based script-like file is used. The _Beef_ standard scripts can be found [here](./Scripts). Script files must be added as an embedded resource within the project to enable runtime access.

The following are the `Script` element's attributes:

Attribute | Description
-|-
`ConfigType` | _Beef_ supports two configuration types; either [`Entity`](#Entity-driven-code-gen) or [`Database`](#Database-driven-code-gen). This informs the code generator which of the two is the intended configuration source.
`Inherits` | A script file can inherit the script configuration from one or more parent files by specifying the name or names (semicolon '`;`' separated). This simplifies the addition of additional artefact generation without having to repeat configuration.
`ConfigEditor` | This is the `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that provides an opportunity to edit (modify) the loaded configuration. The `Type` must implement [`IConfigEditor`](./Config/IConfigEditor.cs).

The following are the `Generate` element's attributes:

Attribute | Description
-|-
`GenType` | This is the `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that will perform the underlying configuration data selection, where the corresponding `Template` will be invoked per selected item. This `Type` must inherit from [`CodeGeneratorBase`](./Generators/CodeGeneratorBase.cs); the _Beef_ standard code generators can be found [here](./Generators).
`Template` | This is the unique name (case-sensitive) of the `Template` that should be used.
`FileName` | This is the name of the file (artefact) that will be generated; this also supports Handlebars syntax to enable runtime computation.
`OutDir` | This is the sub-directory (path name) where the file (artefact) will be generated; this also supports Handlebars syntax to enable runtime computation.
`GenOnce` | This boolean (true/false) indicates whether the file is to be only generated once; i.e. only created where it does not already exist.
`HelpText` | This is additional help text that is output to the console when the script generator is executed.

Any other attributes specified for the `Generate` element will be passed as a runtime parameters (name/value pairs); see [`IRootConfig.RuntimeParameters`](./Config/ConfigBase.cs).

An example of a Script XML file is as follows:

``` xml
<Script ConfigType="Entity" Inherits="EntityBusiness.xml">
  <Generate GenType="Beef.CodeGen.Generators.EntityWebApiControllerCodeGenerator" Template="EntityWebApiController_cs.hbs" FileName="{{Name}}Controller.cs" OutDir="{{Root.PathApi}}/Controllers/Generated" EntityScope="Common" HelpText="EntityWebApiControllerCodeGenerator: Api/Controllers" />
  ...
</Script>
```

<br/>

## Entity-driven code-gen

The entity-driven gen-many code generation is enabled by an **Entity** configuration file that is responsible for defining the characteristics used by the code-gen tooling. The hierarchy is as follows:

```
└── CodeGeneration
  └── Entity(s)
    └── Property(s)
    └── Const(s)
    └── Operation(s)
      └── Parameter(s)
```

Configuration details for each of the above are as follows:
- `CodeGeneration` - [YAML/JSON](../../docs/Entity-CodeGeneration-Config.md) or [XML](../../docs/Entity-CodeGeneration-Config-Xml.md)
- `Entity` - [YAML/JSON](../../docs/Entity-Entity-Config.md) or [XML](../../docs/Entity-Entity-Config-Xml.md)
- `Property` - [YAML/JSON](../../docs/Entity-Property-Config.md) or [XML](../../docs/Entity-Property-Config-Xml.md)
- `Const` - [YAML/JSON](../../docs/Entity-Const-Config.md) or [XML](../../docs/Entity-Const-Config-Xml.md)
- `Operation` - [YAML/JSON](../../docs/Entity-Operation-Config.md) or [XML](../../docs/Entity-Operation-Config-Xml.md)
- `Parameter` - [YAML/JSON](../../docs/Entity-Parameter-Config.md) or [XML](../../docs/Entity-Parameter-Config-Xml.md)

The Entity configuration supported filenames are, in the order in which they are searched by the code generator: `entity.beef.yaml`, `entity.beef.json`, `entity.beef.xml`, `{Company}.{AppName}.xml`.

The Entity configuration is defined by a schema, YAML/JSON-based [entity.beef.json](../../tools/Beef.CodeGen.Core/Schema/entity.beef.json) and XML-based [codegen.entity.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.entity.xsd). These schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

There are two additional configuration files that share the same schema:
- [Reference Data](./../../docs/Reference-Data.md) - used to define (configure) the _Beef_-specific Reference Data. Supported filenames are in the order in which they are searched by the code generator: `refdata.beef.yaml`, `refdata.beef.json`, `refdata.beef.xml`, `{Company}.{AppName}.RefData.xml`.
- Data Model - used to define (configure) basic data model .NET classes typically used to represent internal/backend contracts that do not require the full funcionality of a _Beef_ entity. Supported filenames are in the order in which they are searched by the code generator: `datamodel.beef.yaml`, `datamodel.beef.json`, `datamodel.beef.xml`, `{Company}.{AppName}.DataModel.xml`.

<br/>

## Database-driven code-gen

The database-driven code generation is enabled by a **Database** configuration file that is responsible for defining the characteristics used by the code-gen tooling. The hierarcy is as follows:

```
└── CodeGeneration
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
  └── Cdc(s)
    └── CdcJoin(s)
      └── CdcJoinOn(s)
```

Configuration details for each of the above are as follows:
- CodeGeneration - [YAML/JSON](../../docs/Database-CodeGeneration-Config.md) or [XML](../../docs/Database-CodeGeneration-Config-Xml.md)
- Query - [YAML/JSON](../../docs/Database-Query-Config.md) or [XML](../../docs/Database-Query-Config-Xml.md)
- QueryJoin - [YAML/JSON](../../docs/Database-QueryJoin-Config.md) or [XML](../../docs/Database-QueryJoin-Config-Xml.md)
- QueryJoinOn - [YAML/JSON](../../docs/Database-QueryJoinOn-Config.md) or [XML](../../docs/Database-QueryJoinOn-Config-Xml.md)
- QueryWhere - [YAML/JSON](../../docs/Database-QueryWhere-Config.md) or [XML](../../docs/Database-QueryWhere-Config-Xml.md)
- QueryOrder - [YAML/JSON](../../docs/Database-QueryOrder-Config.md) or [XML](../../docs/Database-QueryOrder-Config-Xml.md)
- Table - [YAML/JSON](../../docs/Database-Table-Config.md) or [XML](../../docs/Database-Table-Config-Xml.md)
- StoredProcedure - [YAML/JSON](../../docs/Database-StoredProcedure-Config.md) or [XML](../../docs/Database-StoredProcedure-Config-Xml.md)
- Parameter - [YAML/JSON](../../docs/Database-Parameter-Config.md) or [XML](../../docs/Database-Parameter-Config-Xml.md)
- Where - [YAML/JSON](../../docs/Database-Where-Config.md) or [XML](../../docs/Database-Where-Config-Xml.md)
- OrderBy - [YAML/JSON](../../docs/Database-OrderBy-Config.md) or [XML](../../docs/Database-OrderBy-Config-Xml.md)
- Execute - [YAML/JSON](../../docs/Database-Execute-Config.md) or [XML](../../docs/Database-Execute-Config-Xml.md)
- Cdc - [YAML/JSON](../../docs/Database-Cdc-Config.md) or [XML](../../docs/Database-Cdc-Config-Xml.md)
- CdcJoin - [YAML/JSON](../../docs/Database-CdcJoin-Config.md) or [XML](../../docs/Database-CdcJoin-Config-Xml.md)
- CdcJoinOn - [YAML/JSON](../../docs/Database-CdcJoinOn-Config.md) or [XML](../../docs/Database-CdcJoinOn-Config-Xml.md)

The Database configuration supported filenames are, in the order in which they are searched by the code generator: `database.beef.yaml`, `database.beef.json`, `database.beef.xml`, `{Company}.{AppName}.Database.xml`.

The Database configuration is defined by a schema, YAML/JSON-based [database.beef.json](../../tools/Beef.CodeGen.Core/Schema/database.beef.json) and XML-based [codegen.table.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.table.xsd). These schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

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
`Entity` | Performs code generation using the _entity_ configuration and [`EntityWebApiCoreAgent.xml`](./Scripts/EntityWebApiCoreAgent.xml) script.
`RefData` | Performs code generation using the _refdata_ configuration and [`RefDataCoreCrud.xml`](./Scripts/RefDataCoreCrud.xml) script.
`Database` | Performs code generation using the `Company.AppName.Database.xml` configuration and [`Database.xml`](./Scripts/Database.xml) script.
`DataModel` | Performs code generation using the `Company.AppName.DataModel.xml` configuration and [`DataModelOnly.xml`](./Scripts/DataModelOnly.xml) script.
`All` | Performs all of the above (where each is supported as per set up).

Additionally, there are a number of command line options that can be used.

Option | Description
-|-
`-cs` or `--connectionString` | Overrides the connection string for the database.
`-cf` or `--configFile` | Overrides the filename for the configuration.
`-s` or `--scriptFile` | Overrides the filename for the script orchestration.
`-enc` or `--expectNoChanges` | Expect no changes in the artefact output and error where changes are detected. This is intended for use with the likes of the build pipeline.
`-x2y` or `--xmlToYaml` | Convert the XML configuration into YAML equivalent (will not codegen).

<br/>

### Program.cs

The `Program.cs` for the new console application should be updated similar to the following. The `Company` and `AppName` values are specified, as well as optionally indicating whether the `Entity`, `RefData`, `Database` and/or `DataModel` commands are supported.

``` csharp
public class Program
{
    static int Main(string[] args)
    {
        return CodeGenConsoleWrapper
            .Create("Company", "AppName")           // Create the wrapper setting Company and AppName.
            .Supports(entity: true, refData: true)  // Set which of the Commands are supported.
            .Run(args);                             // Run the console.
    }
}
```

<br/>

To run the console application, simply specify the required command; e.g:
```
dotnet run entity      -- Default filename: Company.AppName.xml
dotnet run refdata     -- Default filename: Company.RefData.xml
dotnet run datamodel   -- Default filename: Company.AppName.DataModel.xml
dotnet run all         -- All of the above

-- Override the configuration filename.
dotnet run entity -cf configfilename.xml
```

</br>

## Personalization and/or overriding

As described above _Beef_ has a set of pre-defined (out-of-the-box) [Scripts](#Scripts) and [Templates](#Templates). These do not have to be used, or additional can be added, where an alternate code-generation outcome is required.

To avoid the need to clone the solution, and update, add the `Templates` and `Scripts` folders into the console application and embed the required resources. The underlying `Beef.CodeGen.Core` will probe the embedded resources and use the overridden version where provided, falling back on the _Beef_ version where not overridden. 

One or more of the following options exist to enable personalization.

Option | Description
-|-
[Config](#Config) | There is currently _no_ means to extend the underlying configuration .NET types directly. However, as all the configuration types inherit from [`ConfigBase`](./Config/ConfigBase.cs) the `ExtraProperties` hash table is populated with any additional configurations during the deserialization process. These values can then be referenced direcly within the Templates as required. To perform further changes to the configuration at runtime an [`IConfigEditor`](./Config/IConfigEditor.cs) can be added and then referenced from within the corresponding `Scripts` file; it will then be invoked prior to the code generation enabling further changes to occur. The `ConfigBase.CustomProperties` hash table is further provided to enable additional properties to be set and referenced in a consistent manner.
[Templates](#Templates) | Add new [Handlebars](https://handlebarsjs.com/guide/) file, as an embedded resource, to the `Templates` folder (add where not pre-existing) within the project. Where overriding use the same name as that provided out-of-the-box; otherwise, ensure the `Template` is referenced by the `Script`.
[Scripts](#Scripts) | Add new `Scripts` XML file, as an embedded resource, to the `Scripts` folder (add where not pre-existing) within the project. Use the `Inherits` attribute where still wanting to execute the out-of-the-box code-generation.

<br/>

### Example

The [`Beef.Demo.Codegen`](./../../samples/Demo/Beef.Demo.Codegen) provides an example (tests the capability) of the implementation.

Code | Description
-|-
[`TestConfigEditor.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Config/TestConfigEditor.cs) | This implements [`IConfigEditor`](./Config/IConfigEditor.cs) and demonstrates `ConfigBase.TryGetExtraProperty` and `ConfigBase.CustomProperties` usage.
[`TestCodeGenerator.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Generators/TestCodeGenerator.cs) | This inherits from [`CodeGeneratorBase<TRootConfig, TGenConfig>`](./Generators/CodeGeneratorBase.cs) overriding the `SelectGenConfig` to select the configuration that will be used by the associated Template.
[`Test_cs.hbs`](./../../samples/Demo/Beef.Demo.CodeGen/Templates/Test_cs.hbs) | This demonstrates how to reference both the `ExtraProperties` and `CustomProperties` using _Handlebars_ syntax. This file must be added as an embedded resource.
[`TestScript.xml`](./../../samples/Demo/Beef.Demo.CodeGen/Scripts/TestScript.xml) | This demonstrates the required configuration to wire-up the previous so that they are leveraged apprpropriately at runtime. This file must be added as an embedded resource.

Finally the [`Program.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Program.cs) will need to be updated similar as follows to use the new Scripts resource.

``` csharp
return CodeGenConsoleWrapper
    .Create("Beef", "Demo")
    .Supports(entity: true, refData: true, dataModel: true)
    .EntityScript("TestScript.xml")   // <- Overrides the Script name.
    .RunAsync(args);
```