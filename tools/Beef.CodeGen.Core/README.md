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

The code-generation is composed of the following:

- [Configuration](#Configuration) - this is the configuration that is used to both control the generated artefacts and their respective content.
- [Templates](#Templates) - these are the [Handlebars](https://handlebarsjs.com/guide/) templates that define a specific artefact's content.
- [Scripts](#Scripts) - these script (orchestrate) the templates (one or more) to 

<br/>

### Configuration

The code-gen is driven by a configuration data source, in this case YAML, JSON or XML. This acts as a type of DSL ([Domain Specific Language](https://en.wikipedia.org/wiki/Domain-specific_language)) to define the key characteristics / attributes that will be used to generate the required artefacts.

The two supported configurations are:
- [Entity-driven](#Entity-driven-code-gen) - the configuration root definition is [`Entity.CodeGenConfig`](./Config/Entity/CodeGenConfig.cs). All the related types are found [here](./Config/Entity).
- [Database-driven](#Database-driven-code-gen) - the configuration root definition is [`Database.CodeGenConfig`](./Config/Database/CodeGenConfig.cs). All the related types are found [here](./Config/Database).

The above root, and corresponding related (child), configurations must inherit from [`ConfigBase`](./Config/ConfigBase.cs) to get their required capabilities. The advantage of using a .NET typed class for the configuration is that additional properties (computed at runtime) can be added to aid the code-generation process. The underlying `Prepare` method provides a consistent means to implement this logic.

<br/>

### Templates

Once the code-gen data source(s) have been defined, one or more templates will be required to drive the artefact output. These templates are defined using [Handlebars](https://handlebarsjs.com/guide/) and its syntax, or more specifically [Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net). Template files must be added as an embedded resource within the solution to enable runtime access.

Additionally, _Handlebars_ has been [extended](./Generators/HandlebarsHelpers.cs) to add additional capabilities beyond what is available natively to enable the required generated output.

The _Beef_ standard templates can be found [here](./Templates).

<br/>

### Scripts

To orchestrate the code generation, in terms of the [Templates](#Templates) to be used, an XML-based script file is used. The _Beef_ standard scripts can be found [here](./Scripts). Script files must be added as an embedded resource within the project (within ) to enable runtime access.

The following are the `Script` element's attributes:

Attribute | Description
-|-
`ConfigType` | _Beef_ supports two configuration types; either [`Entity`](#Entity-driven-code-gen) or [`Database`](#Database-driven-code-gen). This informs the code generator which of the two is the intended configuration source.
`Inherits` | A script file can inherit the script configuration from one or more parent files by specifying the name or names (semicolon '`;`' separated). This simplifies the addition of additional artefact generation without having to repeat configuration.
`ConfigEditor` | This is the `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that provides an opportunity to edit (modify) the loaded configuration. The `Type` must implement [`IConfigEditor`](./Config/IConfigEditor.cs).

The following are the `Generate` element's attributes:

Attribute | Description
-|-
`GenType` | This is the `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that will perform the underlying configuration data selection, where the corresponding `Template` will be invoked per selected item. This `Type` must inherit from [`CodeGeneratorBase`](./Generators/CodeGeneratorBase.cs); the _Beef_ code generatos can be found [here](./Generators).
`Template` | This is the unique name (case-sensitive) of the `Template` to be used.
`FileName` | This is the name of the file (artefact) that will be generated; this also supports Handlebars syntax to enable runtime computation.
`OutDir` | This is the sub-directory (path name) where the artefact file (artefact) will be generated; this also supports Handlebars syntax to enable runtime computation.
`HelpText` | This is additional help text that is output to the console when the script generator is executed.

Any other attributes specified for the `Generate` element will be passed as a runtime parameters; see [`IRootConfig.RuntimeParameters`](./Config/ConfigBase.cs).

An example of a Script XML file is as follows:

``` xml
<Script ConfigType="Entity" Inherits="EntityBusiness.xml">
  <Generate GenType="Beef.CodeGen.Generators.EntityWebApiControllerCodeGenerator" Template="EntityWebApiController_cs.hbs" FileName="{{Name}}Controller.cs" OutDir="{{Root.PathApi}}/Controllers" EntityScope="Common" HelpText="EntityWebApiControllerCodeGenerator: Api/Controllers" />
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

The **Entity** configuration supported filenames are, in the order in which they are searched: `entity.beef.yaml`, `entity.beef.json`, `entity.beef.xml`, `{Company}.{AppName}.xml`.

The **Entity** configuration is defined by a schema, YAML/JSON-based [entity.beef.json](../../tools/Beef.CodeGen.Core/Schema/entity.beef.json) and XML-based [codegen.entity.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.entity.xsd). These schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

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

The **Database** configuration supported filenames are, in the order in which they are searched: `database.beef.yaml`, `database.beef.json`, `database.beef.xml`, `{Company}.{AppName}.Database.xml`.

The **Database** configuration is defined by a schema, YAML/JSON-based [database.beef.json](../../tools/Beef.CodeGen.Core/Schema/database.beef.json) and XML-based [codegen.table.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.table.xsd). These schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

Finally, this is not intended as an all purpose database schema generation capability. It is expected that the Tables pre-exist within the database. The database schema/table catalog information is queried from the database directly to aid the generation configuration to minimise the need to replicate column configurations within the configuration directly.

<br/>

## Console application

The `Beef.CodeGen.Core` can be executed as a console application directly; however, the experience has been optimised so that a new console application can reference and inherit the capabilities.

Additionally, the `Database` related code-generation can be (preferred method) enabled using [`Beef.Database.Core`](./../Beef.Database.Core/README.md); see documentation for internal details. This will internally execute `Beef.CodeGen.Core` to perform the code-generation task.

<br/>

### Commands

The following commands are automatically enabled for the console application (where set up):

- `Entity` - performs code generation using the `Company.AppName.xml` configuration and [`EntityWebApiCoreAgent.xml`](./Scripts/EntityWebApiCoreAgent.xml) script.
- `RefData` - performs code generation using the `Company.RefData.xml` configuration and [`RefDataCoreCrud.xml`](./Scripts/RefDataCoreCrud.xml) script.
- `Database` - performs code generation using the `Company.AppName.Database.xml` configuration and [`Database.xml`](./Scripts/Database.xml) script.
- `DataModel` - performs code generation using the `Company.AppName.DataModel.xml` configuration and [`DataModelOnly.xml`](./Scripts/DataModelOnly.xml) script.
- `All` - performs all of the above (where each is supported as per set up).

There are a number of properties that support changes to these templates above where they need to be overridden.

<br/>

### Program.cs

The `Program.cs` for the new console application should be updated similar to the following. The `Company` and `AppName` values are specified, as well as optionally indicating whether the `Entity`, `RefData`, `Database` and/or `DataModel` commands are supported.

``` csharp
public class Program
{
    static int Main(string[] args)
    {
        return CodeGenConsoleWrapper.Create("Company", "AppName").Supports(entity: true, refData: true).Run(args);
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

-- Override the configuration filename from the default.
dotnet run entity -x configfilename.xml
```

</br>

## Personalization and/or overriding

As described above _Beef_ has a set of pre-defined (out-of-the-box) [Scripts](#Scripts) and [Templates](#Templates). These do not have to be used, or could be maintained, to achieve an alternate code-generation outcome as required.

To avoid the need to clone the solution, and update, add the `Templates` and `Scripts` folders into the console application and embed the required resources. The underlying `Beef.CodeGen.Core` will probe the embedded resources and use the overridden version where provided, falling back on the _Beef_ version where not overridden. 

One or more of the following options exist to enable personalization.

Option | Description
-|-
[Config](#Config) | There is currently _no_ means to extend the underlying configuration .NET types directly. However, as all the configuration types inherit from [`ConfigBase`](./Config/ConfigBase.cs) the `ExtraProperties` hash table is updated with any additional configurations during the deserialization process. These values can then be referenced direcly within the Templates as required. To perform further changes to the configuration an [`IConfigEditor`](./Config/IConfigEditor.cs) can be added and then referenced from within the corresponding `Scripts` file to be used at runtime. The `ConfigBase.CustomProperties` hash table is further provided to enable additional properties to be set and referenced.
[Templates](#Templates) | Add new [Handlebars](https://handlebarsjs.com/guide/) file, as an embedded resource, to the `Templates` folder (add where not pre-existing) within the project. Where overriding use the same name as that provided out-of-the-box; otherwise, ensure the `Template` is referenced by the `Script`.
[Scripts](#Scripts) | Add new `Scripts` XML file, as an embedded resource, to the `Scripts` folder (add where not pre-existing) within the project. Use the `Inherits` attribute where still wanting to execute the out-of-the-box code-generation.

<br/>

### Example

The [`Beef.Demo.Codegen`](./../../samples/Demo/Beef.Demo.Codegen) provides an example (tests the capability) of the implementation.

Code | Description
-|-
[`TestConfigEditor.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Config/TestConfigEditor.cs) | This implements [`IConfigEditor`](./Config/IConfigEditor.cs) and demonstrates `ConfigBase.TryGetExtraProperty` and `ConfigBase.CustomProperties` usage.
[`TestCodeGenerator.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Generators/TestCodeGenerator.cs) | This inherits from [`CodeGeneratorBase<TRootConfig, TGenConfig>`](./Generators/CodeGeneratorBase.cs) overriding the `SelectGenConfig` to select the configuration that will be used by the associated Template.
[`Test_cs.hbs`](./../../samples/Demo/Beef.Demo.CodeGen/Templates/Test_cs.hbs) | This demonstrates how to reference both the `ExtraProperties` and `CustomProperties` using _Handlebars_ syntax. Must be added as an embedded resource.
[`TestScript.xml`](./../../samples/Demo/Beef.Demo.CodeGen/Scripts/TestScript.xml) | This demonstrates the required configuration to wire-up the previous so that they are leveraged apprpropriately at runtime. Must be added as an embedded resource.

Finally the [`Program.cs`](./../../samples/Demo/Beef.Demo.CodeGen/Program.cs) will need to be updated similar as follows to use the new Scripts resource.

``` csharp
return CodeGenConsoleWrapper
    .Create("Beef", "Demo")
    .Supports(entity: true, refData: true, dataModel: true)
    .EntityScript("TestScript.xml")   // <- Overrides the Script name.
    .RunAsync(args);
```