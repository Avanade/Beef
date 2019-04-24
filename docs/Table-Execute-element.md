# 'Execute' element (table-driven)

The **`Execute`** element will add the specified statement as-is to the generated SQL.

An example is as follows:

```xml
<Execute Statement="EXEC [Prac].[spContactCommsGetByContactId] @ContactId" />
```

<br>

## Attributes

The **`Execute`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes: 

Attribute | Description
-|-
**`Statement`** | The statement to execute. This should be a complete valid SQL statement. Multiple statements can be used to construct multi-line logic where required.
`Location` | Defines the location of the statement within the stored procedure. Options are either `Before` or `After` the primary logic as defined by the stored procedure type. Defaults to `After`. 