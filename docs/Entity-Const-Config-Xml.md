# 'Const' object (entity-driven) - XML

The `Const` object is used to define a .NET (C#) constant value for an `Entity`.

<br/>

## Properties
The `Const` object supports a number of properties that control the generated code output. The properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`Name`** | The unique constant name.
**`Value`** | The .NET (C#) code for the constant value. The code generation will ensure the value is delimited properly to output correctly formed (delimited) .NET (C#) code.
`Text` | The overriding text for use in comments. By default the `Text` will be the `Name` reformatted as sentence casing. It will be formatted as: `Represents a {text} constant value.` To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. `{{Xxx}}`).

<br/>

