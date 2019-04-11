# `OrderBy` element (table-driven)

The **`OrderBy`** element will add the specified order by statement to the generated SQL query (stored procedure type `GetAll` only).

An example is as follows:

```xml
<OrderBy Name="LastName" />
<OrderBy Name="Birthday" Order="Desc" />
```

<br>

## Attributes

The **`OrderBy`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes: 

Attribute | Description
-|-
**`Name`** | The column name. This is mandatory.
`Order` | The sort order. Options are: `Asc` (ascending) or `Desc` (descending). Defaults to `Asc`.