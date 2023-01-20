# 'Relationship' object (database-driven)

The `Relationship` object enables the definition of an entity framework (EF) model relationship.

<br/>

## Property categories
The `Relationship` object supports a number of properties that control the generated code output. These properties are separated into a series of logical categories.

Category | Description
-|-
[`Key`](#Key) | Provides the _key_ configuration.
[`EF`](#EF) | Provides the _.NET Entity Framework (EF)_ specific configuration.
[`DotNet`](#DotNet) | Provides the _.NET_ configuration.

The properties with a bold name are those that are more typically used (considered more important).

<br/>

## Key
Provides the _key_ configuration.

Property | Description
-|-
**`name`** | The name of the primary table of the query. [Mandatory]
`schema` | The schema name of the primary table of the view.<br/>&dagger; Defaults to `CodeGeneration.Schema`.
**`type`** | The relationship type between the parent and child (self). Valid options are: `OneToMany`, `ManyToOne`.<br/>&dagger; Defaults to `OneToMany`.
**`foreignKeyColumns`** | The list of `Column` names from the related table that reference the parent. [Mandatory]
`principalKeyColumns` | The list of `Column` names from the principal table that reference the child.<br/>&dagger;  Typically this is only used where referencing property(s) other than the primary key as the principal property(s).

<br/>

## EF
Provides the _.NET Entity Framework (EF)_ specific configuration.

Property | Description
-|-
`onDelete` | The operation applied to dependent entities in the relationship when the principal is deleted or the relationship is severed. Valid options are: `NoAction`, `Cascade`, `ClientCascade`, `ClientNoAction`, `ClientSetNull`, `Restrict`, `SetNull`.<br/>&dagger; Defaults to `NoAction`. See https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.deletebehavior for more information.
`autoInclude` | Indicates whether to automatically include navigation to the property.<br/>&dagger; Defaults to `false`.

<br/>

## DotNet
Provides the _.NET_ configuration.

Property | Description
-|-
`propertyName` | The corresponding property name within the entity framework (EF) model.<br/>&dagger; Defaults to `Name` using the `CodeGeneration.AutoDotNetRename` option.
`efModelName` | The corresponding entity framework (EF) model name (.NET Type).<br/>&dagger; Defaults to `Name` using the `CodeGeneration.AutoDotNetRename` option.

