# 'Const' object (entity-driven)

The `Const` object is used to define a .NET (C#) constant value for an `Entity`.

A YAML configuration example is as follows:
``` yaml
consts: [
  { name: Female, value: F },
  { name: Male, value: M }
]
```

<br/>

## Properties
The `Const` object supports a number of properties that control the generated code output. The properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The unique constant name.
**`value`** | The .NET (C#) code for the constant value. The code generation will ensure the value is delimited properly to output correctly formed (delimited) .NET (C#) code.
`text` | The overriding text for use in comments. By default the `Text` will be the `Name` reformatted as sentence casing. It will be formatted as: `Represents a {text} constant value.` To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. `{{Xxx}}`).

<br/>

