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

<br>

## Code-gen data source

The code-gen is driven by a data source, in this case XML. This acts as a type of DSL ([Domain Specific Language](https://en.wikipedia.org/wiki/Domain-specific_language)) to define the key characteristics / attributes that will be used to generate the required artefacts.

<br>

## Code-gen templates

Once the code-gen data source(s) have been defined, one or more templates will be required to drive the artefact output. These [templates](../../docs/Template-structure.md) are defined using XML - these have some basic language / logic capabilities to enable rich code generation output.

The _Beef_ standard templates can be found [here](./Templates).

<br>

## Scripts and loaders

To orchestrate the code generation, in terms of the templates to be used, an XML-based script file is used. The _Beef_ standard scripts can be found [here](./Scripts).

Additionally, the script can define the _loader_ `Type` which is a class that implements [`ICodeGenConfigLoader`](../../src/Beef.Core/CodeGen/ICodeGenConfigLoader.cs). The purpose of a loader is to manipulate (update) the configuration XML before processing via the templates. The _Beef_ standard loaders can be found [here](./Loaders).

## Supported code-gen

The following code generation is supported:

![CodeGen](../../docs/images/CodeGen.png)

<br>

### Entity-driven code-gen

The entity-driven gen-many code generation is enabled by an **Entity.xml** file that is responsible for defining the characteristics for an [Entity](../../docs/Entity-Entity-element.md), its [Properties](../../docs/Entity-Property-element.md), [Constants](../../docs/Entity-Const-element.md) and [Operations](../../docs/Entity-Operation-element.md) (and underlying [Parameters](../../docs/Entity-Parameter-element.md)). The entity definitions are wrapped by a root [CodeGeneration](../../docs/Entity-CodeGeneration-element.md) element.

The hierarcy is as follows:

```
└── CodeGeneration
  └── Entity(s)
    └── Property(s)
    └── Const(s)
    └── Operation(s)
      └── Parameter(s)
```

The **Entity.xml** is defined by a schema [codegen.entity.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.entity.xsd). This schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities.

<br>

### Database table-driven code-gen

The database table-driven code generation is enabled by a **Table.xml** file that is responsible for defining the characteristics for the generation of stored procedures. A [Table](../../docs/Table-Table-element.md) has [StoredProcedures](../../docs/Table-StoredProcedure-element.md) (and underlying [Parameters](../../docs/Table-Parameter-element.md), [Where](../../docs/Table-Where-element.md) and [OrderBy](../../docs/Table-OrderBy-element.md) clauses, and [Execute](../../docs/Table-Execute-element.md) statements). The tablle definitions are wrapped by a root [CodeGeneration](../../docs/Table-CodeGeneration-element.md) element.

 Standard stored procedures can be generated from a **Table** schema, as well as the definition of a custom defined **Stored Procedure**.

The hierarcy is as follows:

```
└── CodeGeneration
  └── Table(s)
    └── StoredProcedure(s)
      └── Parameter(s)
      └── Where(s)
      └── OrderBy(s)
      └── Execute(s)
```

The **Table.xml** is defined by a schema [codegen.table.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.table.xsd). This schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities. 

This is not intended as an all purpose database schema generation capability. It is expected that the Tables and/or Views pre-exist within the database. This database schema/catalog information is queried from the database directly to aid the generation configuration to minimise the need to replicate column configurations within the **Table.xml**.

<br>

## Console application

The `Beef.CodeGen.Core` can be executed as a console application directly; however, the experience has been optimised so that a new console application can reference and inherit the capabilities. 

Then simply add the `Templates` and `Scripts` folders and embed the required resources. See the sample [`Beef.Demo.Database`](../../samples/Demo/Beef.Demo.CodeGen) as an example.

<br/>

### Commands

The following commands are automatically enabled for the console application (where set up):

- `Entity` - performs code generation using the `Company.AppName.xml` configuration and [`EntityWebApiCoreAgent.xml`](./Scripts/EntityWebApiCoreAgent.xml) script.
- `RefData` - performs code generation using the `Company.RefData.xml` configuration and [`RefDataCoreCrud.xml`](./Scripts/RefDataCoreCrud.xml) script.
- `Database` - performs code generation using the `Company.AppName.Database.xml` configuration and [`Database.xml`](./Scripts/Database.xml) script.
- `All` - performs all of the above (where each is supported as per set up).

There are a number of properties that support changes to these template above where they need to be overridden.

<br/>

### Program.cs

The `Program.cs` for the new console application should be updated similar to the following. The `Company` and `AppName` values are specified, as well as optionally indicating whether the `entity` and/or `refdata` commands are supported.

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
dotnet run entity    -- Default filename: Company.AppName.xml
dotnet run refdata   -- Default filename: Company.RefData.xml
dotnet run all       -- All of the above

-- Override the configuration filename from the default.
dotnet run entity -x configfilename.xml
```

</br>

### Personalization and/or overridding

As described above _Beef_ has a set of defined (out-of-the-box) templates and scripts - these do not have to be used, or could be maintained, to achieve an alternate outcome as required.

To avoid the need to clone the solution, and update, add the `Templates` and `Scripts` folders and embed the required resources. The underlying `Beef.CodeGen.Core` will probe the embedded resources to and use the overridden version where provided, falling back on the _Beef_ version where not found. 

<br/>

## What about T4?

There are multiple capabilities to perform code generation, such as the likes of [T4](https://docs.microsoft.com/en-au/visualstudio/modeling/code-generation-and-t4-text-templates) (arguably, it is a more fully featured code generation capability).

The *Beef* code generation largely pre-dates T4, is highly-flexible achieving the desired code generation outcomes, and as such there has been no compelling reason to replatform to date - a high-cost, with a limited return.