# Beef.CodeGen.Abstractions

Provides extended code-generation capabilities that encapsulates [Handlebars](https://handlebarsjs.com/guide/) as the underlying code generator, or more specifically [Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net).

This is intended to provide the base for code-generation tooling enabling a rich and orchestrated code-generation experience in addition to the templating and generation enabled using native _Handlebars_. 

## Overview

Code generation can be used to greatly accelerate application development where standard coding patterns can be determined and the opportunity to automate exists.

Code generation can bring many or all of the following benefits:
- **Acceleration** of development;
- **Consistency** of approach;
- **Simplification** of implementation;
- **Reusability** of logic;
- **Evolution** of approach over time;
- **Richness** of capabilities with limited effort.

There are generally two types of code generation:
- **Gen-many** - the ability to consistently generate a code artefact multiple times over its lifetime without unintended breaking side-effects. Considered non-maintainable by the likes of developers as contents may change at any time; however, can offer extensible hooks to enable custom code injection where applicable. This approach offers the greatest long-term benefits.
- **Gen-once** - the ability to generate a code artefact once to effectively start the development process. Considered maintainable, and should not be re-generated as this would override custom coded changes.

The code-generation capabilities within support both of the above two types.

<br/>

## Composition

The code-generation tooling is composed of the following:

1. [Configuration](#Configuration) - data used as the input for to drive the code-generation and the underlying _templates_.
2. [Templates](#Templates) - [Handlebars](https://handlebarsjs.com/guide/) templates that define a specific artefact's scripted content.
3. [Scripts](#Scripts) - orchestrates one or more templates that are used to generate artefacts for a gvien _configuration_ input.

<br/>

## Configuration

The code-gen is driven by a configuration data source, in this case a YAML or JSON file. This acts as a type of DSL ([Domain Specific Language](https://en.wikipedia.org/wiki/Domain-specific_language)) to define the key characteristics / attributes that will be used to generate the required artefacts.

A configuration consists of the following, which each ultimately inherit from [`ConfigBase`](./Config/ConfigBase.cs) for its base capabilities:

- **Root node** - this must inherit from [`ConfigRootBase`](./Config/ConfigRootBase.cs) which enables additional runtime parameters enabled by [`IRootConfig`](./Config/IRootConfig.cs).
- **Child nodes** - zero or more child nodes (hierarchical) that inherit from [`ConfigBase<TRoot, TParent>`](./Config/ConfigBaseT.cs) that specify the `Root` and `Parent` hierarchy.

The advantage of using a .NET typed class for the configuration is that additional properties (computed at runtime) can be added to aid the code-generation process. The underlying `Prepare` method provides a consistent means to implement this logic at runtime.

The following attributes are generally required when defining the .NET Types, as they enable validation, and corresponding schema/documentation generation:

Attribute | Description
-|-
[`CodeGenClassAttribute`](./Config/CodeGenClassAttribute.cs) | Defines the schema/documentation details for the .NET class.
[`CodeGenCategoryAttribute`](./Config/CodeGenCategoryAttribute.cs) | Defines one or more documentation categories for a .NET class.
[`CodeGenPropertyAttribute`](./Config/CodeGenPropertyAttribute.cs) | Defines validation (`IsMandatory` and `Options`) and documentation for a property (non-collection).
[`CodeGenPropertyCollectionAttribute`](./Config/CodeGenPropertyCollectionAttribute.cs) | Defines validation (`IsMandatory`) and documentation for a collection property.

The configuration must also use the [Newtonsoft Json.NET serializer attributes](https://www.newtonsoft.com/json/help/html/SerializationAttributes.htm) as [Json.NET](https://www.newtonsoft.com/json/help) is used internally to perform all JSON deserialization.

An example is as follows:

``` csharp
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[CodeGenClass("Entity", Title = "'Entity' object.", Description = "The `Entity` object.", Markdown = "This is a _sample_ markdown.", ExampleMarkdown = "This is an `example` markdown.")]
[CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
[CodeGenCategory("Collection", Title = "Provides related child (hierarchical) configuration.")]
public class EntityConfig : ConfigRootBase<EntityConfig>
{
    [JsonProperty("name")]
    [CodeGenProperty("Key", Title = "The entity name.", IsMandatory = true)]
    public string? Name { get; set; }

    [JsonProperty("properties")]
    [CodeGenPropertyCollection("Collection", Title = "The `Property` collection.", IsImportant = true)]
    public List<PropertyConfig>? Properties { get; set; }

    protected override void Prepare()
    {
        Properties = PrepareCollection(Properties);
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[CodeGenClass("Property", Title = "'Property' object.", Description = "The `Property` object.")]
[CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
public class PropertyConfig : ConfigBase<EntityConfig, EntityConfig>
{
    public override string QualifiedKeyName => BuildQualifiedKeyName("Property", Name);

    [JsonProperty("name")]
    [CodeGenProperty("Key", Title = "The property name.", IsMandatory = true)]
    public string? Name { get; set; }

    [JsonProperty("type")]
    [CodeGenProperty("Key", Title = "The property type.", Description = "This is a more detailed description for the property type.", IsImportant = true, Options = new string[] { "string", "int", "decimal" })]
    public string? Type { get; set; }

    [JsonProperty("isNullable")]
    [CodeGenProperty("Key", Title = "Indicates whether the property is nullable.")]
    public bool? IsNullable { get; set; }

    protected override void Prepare()
    {
        Type = DefaultWhereNull(Type, () => "string");
    }
}
```

A corresponding configuration YAML example is as follows:

``` yaml
name: Person
properties:
- { name: Name }
- { name: Age, type: int }
- { name: Salary, type: decimal, isNullable: true }
```

<br/>

## Templates

Once the code-gen configuration data source(s) have been defined, one or more _templates_ will be required to drive the artefact output. These templates are defined using [Handlebars](https://handlebarsjs.com/guide/) syntax. Template files can either be added as an embedded resource within a folder named `Templates` (primary), or referenced directly on the file system (secondary), to enable runtime access.

Additionally, Handlebars has been [extended](./Utility/HandlebarsHelpers.cs) to add additional capabilities beyond what is available natively to further enable the required generated output.

<br/>

## Scripts

To orchestrate the code generation, in terms of the [Templates](#Templates) to be used, a YAML-based script-like file is used. Script files can either be added as an embedded resource within a folder named `Scripts` (primary), or referenced directly on the file system (secondary), to enable runtime access.

The following are the [`CodeGenScripts`](./Scripts/CodeGenScripts.cs) properties:

Property | Description
-|-
`configType` | The expected .NET [Configuration](#Configuration) root node `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)). This ensures that the code generator will only be executed with the specified configuration source.
`inherits` | A script file can inherit the script configuration from one or more parent script files specified by a script name array. This is intended to simplify/standardize the addition of additional artefact generation without the need to repeat.
`editorType` | The .NET `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that provides an opportunity to modity the loaded configuration. The `Type` must implement [`IConfigEditor`](./Config/IConfigEditor.cs). This enables runtime changes to configuration where access to the underlying source code for the configuration is unavailable.
`generators` | A collection of none of more scripted generators.

The following are the `generators` [`CodeGenScript`](./Scripts/CodeGenScript.cs) item properties:

Attribute | Description
-|-
`type` | The .NET `Type` name (as used by [`Type.GetType`](https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype#System_Type_GetType_System_String_)) that will perform the underlying configuration data selection, where the corresponding `Template` will be invoked per selected item. This `Type` must inherit from either [`CodeGeneratorBase<TRootConfig>`](./Generators/CodeGeneratorBaseT.cs) or [`CodeGeneratorBase<TRootConfig, TGenConfig>`](./Generators/CodeGeneratorBaseT2.cs). The inherited `SelectGenConfig` method should be overridden where applicable to perform the actual selection.
`template` | The name of the [Handlebars template](https://handlebarsjs.com/guide/) that should be used.
`file` | The name of the file (artefact) that will be generated; this also supports _Handlebars_ syntax to enable runtime computation.
`directory` | This is the sub-directory (path) where the file (artefact) will be generated; this also supports _Handlebars_ syntax to enable runtime computation.
`genOnce` | This boolean (true/false) indicates whether the file is to be only generated once; i.e. only created where it does not already exist (optional).
`text` | The text written to the log to enable additional context (optional).

Any other YAML properties specified will be automatically passed in as a runtime parameters (name/value pairs); see [`IRootConfig.RuntimeParameters`](./Config/IRootConfig.cs).

An example of a Script YAML file is as follows:

``` yaml
configType: Beef.CodeGen.Abstractions.Test.Config.EntityConfig, Beef.CodeGen.Abstractions.Test
generators:
- { type: 'Beef.CodeGen.Abstractions.Test.Generators.EntityGenerator, Beef.CodeGen.Abstractions.Test', template: EntityExample.hbs, directory: "{{lookup RuntimeParameters 'Directory'}}", file: '{{Name}}.txt', Company: Xxx, AppName: Yyy }
- { type: 'Beef.CodeGen.Abstractions.Test.Generators.PropertyGenerator, Beef.CodeGen.Abstractions.Test', template: PropertyExample.hbs, directory: "{{lookup Root.RuntimeParameters 'Directory'}}", file: '{{Name}}.txt' }
```

An [`CodeGeneratorBase<TRootConfig, TGenConfig>`](./Generators/CodeGeneratorBaseT2.cs) example is as follows:

``` csharp
// A generator that is targeted at the Root (EntityConfig); pre-selects automatically.
public class EntityGenerator : CodeGeneratorBase<EntityConfig> { }

// A generator that targets a Child; requires selection from the Root (EntityConfig).
public class PropertyGenerator : CodeGeneratorBase<EntityConfig, PropertyConfig>
{
    protected override IEnumerable<PropertyConfig> SelectGenConfig(EntityConfig config) => config.Properties!;
}
```

<br/>

## Code-generation

To enable code-generation the [`CodeGenerator`](./CodeGenerator.cs) is used. The constructor for this class takes a [`CodeGeneratorArgs`](./CodeGeneratorArgs.cs) that specifies the key input, such as the [script](#Scripts) file name. The additional argument properties enable additional capabilities within. The `CodeGenerator` constructor will ensure that the underlying script configuration is valid before continuing.

To perform the code-generation the `Generate` method is invoked passing the [configuration](#Configuration) file input. The configuration is initially parsed and validated, then each of the _generators_ described within the corresponding _script_ file are instantiated, the configuration data selected and iterated, then passed into the [`HandlebarsCodeGenerator`](./Utility/HandlebarsCodeGenerator.cs) to perform the code-generation proper against the scripted _template_. A [`CodeGenStatistics`](./CodeGenStatistics.cs) is returned providing basic statistics from the code-generation execution.

The `CodeGenerator` can be inherited to further customize; with the `OnBeforeScript`, `OnCodeGenerated`, and `OnAfterScript` methods available for override.

An example is as follows:

``` csharp
var cg = new CodeGenerator(new CodeGeneratorArgs("Script.yaml") { Assemblies = new Assembly[] { typeof(Program).Assembly } });
var stats = cg.Generate("Configuration.yaml");
```

<br/>

## Utility

Some additional utility capabilites have been provided to support:

- [`JsonSchemaGenerator`](./Utility/JsonSchemaGenerator.cs) - provides the capability to generate a [JSON Schema](https://json-schema.org/) from the [configuration](#Configuration). This can then be published to the likes of the [JSON Schema Store](https://www.schemastore.org/) so that it can be used in Visual Studio and Visual Studio Code to provide editor intellisense and basic validation.
- [`MarkdownDocumentationGenerator`](./Utility/MarkdownDocumentationGenerator.cs) - provides the capability to generate [markdown](https://en.wikipedia.org/wiki/Markdown) documentation files from the [configuration](#Configuration). These can then be published with the owning source code repository or wiki to provide corresponding documentation.