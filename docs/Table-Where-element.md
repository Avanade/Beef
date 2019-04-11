# `Where` element (table-driven)

The **`Where`** element will add the specified where statement to the generated SQL.

An example is as follows:

```xml
<Where Statement="IsActive EQ 0" />
```

<br>

## Attributes

The **`Where`** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes: 

Attribute | Description
-|-
**`Statement`** | The statement to execute. It is assumed that the left hand side of the condition is the column name; as this will be prefixed by the alias within the generated code. This is mandatory.