# 'Execute' object (database-driven)

The _Execute_ object enables additional TSQL statements to be embedded within the stored procedure.

<br/>

## Properties
The `Execute` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`Statement`** | The additional TSQL statement. [Mandatory]
**`Location`** | The location of the statement in relation to the underlying primary stored procedure statement. Valid options are: `Before`, `After`.<br/>&dagger; Defaults to `After`.

