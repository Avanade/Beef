# 'Const' element (entity-driven)

The **Const** element enables constant values to be defined for an entity.

An exampleis as follows:

```xml
<Const Name="Success" Text="Success" Value="1" />
```

<br>

## Attributes

The **Const** element supports a number of attributes that control the generated code output. These attributes has been broken into logical categories. The attributes with a bold name are those that are more typically used.

The following represents the **key** attributes: 

Attribute | Description
-|-
**`Name`** | The unique constant name.
`Text` | The text to be used within the generated summary comments ['Represents a {0} constant value'].
**`Value`** | The identifier value (as an `int`, `Guid` or `string` as defined by `Entity.ConstType`).