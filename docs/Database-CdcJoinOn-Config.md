# 'CdcJoinOn' object (database-driven)

The `CdcJoinOn` object defines the join on characteristics for a CDC join.

<br/>

## Example

A YAML configuration example is as follows:
``` yaml

```

<br/>

## Properties
The `CdcJoinOn` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The name of the join column (from the `Join` table). [Mandatory]
**`toColumn`** | The name of the join to column.<br/><br/>Defaults to `Name`; i.e. assumes same name.
`toStatement` | The SQL statement for the join on bypassing the corresponding `Column` specification.

