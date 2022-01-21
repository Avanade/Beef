# 'QueryOrder' object (database-driven)

The `QueryOrder` object that defines the query order.

<br/>

## Properties
The `QueryOrder` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`Name`** | The name of the `Column` to order by. [Mandatory]<br/>&dagger; See also `Schema` and `Table` as these all relate.
`Schema` | The name of order by table schema. See also `Name` and `Column` as these all relate.<br/>&dagger; Defaults to `Query.Schema`.
`Table` | The name of the order by table.<br/>&dagger; Defaults to `Table.Name`; i.e. primary table. See also `Schema` and `Column` as these all relate.
**`Order`** | The corresponding sort order. Valid options are: `Ascending`, `Descending`.<br/>&dagger; Defaults to `Ascending`.

