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
      └── OrderBy
      └── Execute
```

The **Table.xml** is defined by a schema [codegen.table.xsd](../../tools/Beef.CodeGen.Core/Schema/codegen.table.xsd). This schema should be used within the likes of Visual Studio when editing to enable real-time validation and basic intellisense capabilities. 

This is not intended as an all purpose database schema generation capability. It is expected that the Tables and/or Views pre-exist within the database. This database schema/catalog information is queried from the database directly to aid the generation configuration to minimise the need to replicate column configurations within the **Table.xml**.

<br>

## What about T4?

There are multiple capabilities to perform code generation, such as the likes of [T4](https://docs.microsoft.com/en-au/visualstudio/modeling/code-generation-and-t4-text-templates) (arguably, it is a more fully featured code generation capability).

The *Beef* code generation largely pre-dates T4, is highly-flexible achieving the desired code generation outcomes, and as such there has been no compelling reason to replatform to date - a high-cost, with a limited return.