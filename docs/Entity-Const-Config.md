# 'Const' object (entity-driven)

The `Const` object is used to define a .NET (C#) constant value for an `Entity`.

<br/>

## Example

A YAML configuration example is as follows:
``` yaml
consts: [
  { name: Female, value: F },
  { name: Male, value: M }
]
```

<br/>

## Properties
The `Const` object supports a number of properties that control the generated code output. The following properties with a bold name are those that are more typically used (considered more important).

Property | Description
-|-
**`name`** | The unique constant name. [Mandatory]
**`value`** | The .NET (C#) code for the constant value. [Mandatory]<br/>&dagger; The code generation will ensure the value is delimited properly to output correctly formed (delimited) .NET (C#) code.
`text` | The overriding text for use in comments.<br/>&dagger; By default the `Text` will be the `Name` reformatted as sentence casing. It will be formatted as: `Represents a {text} constant value.` To create a `<see cref="XXX"/>` within use moustache shorthand (e.g. `{{Xxx}}`). To have the text used as-is prefix with a `+` plus-sign character.

