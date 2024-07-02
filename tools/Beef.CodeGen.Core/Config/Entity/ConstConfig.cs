// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using OnRamp.Config;
using OnRamp.Utility;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Const</b> code-generation configuration.
    /// </summary>
    [CodeGenClass("Const", Title = "'Const' object (entity-driven)", 
        Description = "The `Const` object is used to define a .NET (C#) constant value for an `Entity`.", 
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml
consts: [
  { name: Female, value: F },
  { name: Male, value: M }
]
```")]
    [CodeGenCategory("Key", Title = "Provides the **key** configuration.")]
    public class ConstConfig : ConfigBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Const", Name);

        /// <summary>
        /// Gets or sets the unique constant name.
        /// </summary>
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The unique constant name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the C# code for the constant value.
        /// </summary>
        [JsonPropertyName("value")]
        [CodeGenProperty("Key", Title = "The .NET (C#) code for the constant value.", IsMandatory = true, IsImportant = true,
            Description = "The code generation will ensure the value is delimited properly to output correctly formed (delimited) .NET (C#) code.")]
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonPropertyName("text")]
        [CodeGenProperty("Key", Title = "The overriding text for use in comments.",
            Description = "By default the `Text` will be the `Name` reformatted as sentence casing. It will be formatted as: `Represents a {text} constant value.` To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. `{{Xxx}}`). To have the text used as-is prefix with a `+` plus-sign character.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => CodeGenConfig.GetSentenceText(CodeGenConfig.GetFormattedText(Text, text => $"Represents a {text} constant value."));

        /// <summary>
        /// Gets the value formatted for code output.
        /// </summary>
        public string? FormattedValue => CompareValue(Value, "int") ? Value : (CompareValue(Value, "Guid") ? $"new Guid(\"{Value}\")" : $"\"{Value}\"");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            Text = DefaultWhereNull(Text, () => StringConverter.ToSentenceCase(Name));
            return Task.CompletedTask;
        }
    }
}