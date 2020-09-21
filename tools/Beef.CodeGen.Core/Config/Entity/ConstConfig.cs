// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Const</b> code-generation configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Const", Title = "The **Const** is used to define an constant value for an `Entity`.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class ConstConfig : ConfigBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// Gets or sets the unique constant name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique constant name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the C# code for the constant value.
        /// </summary>
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The C# code for the constant value.", IsMandatory = true, IsImportant = true,
            Description = "The code generation will ensure it is delimited correctly to output correctly formed C# code.")]
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the overridding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding text for use in comments.",
            Description = "By default the `Text` will be the `Name` reformatted as sentence casing. It will be formatted as: Represents a {text} constant value.'. To create a `<see cref=\"XXX\"/>` within use moustache shorthand (e.g. {{Xxx}}).")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => $"Represents a {Text} constant value.";

        /// <summary>
        /// Gets the value formatted for code output.
        /// </summary>
        public string? FormattedValue => CompareValue(Value, "int") ? Value : (CompareValue(Value, "Guid") ? $"new Guid(\"{Value}\")" : $"\"{Value}\"");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            DefaultWhereNull(Text, () => StringConversion.ToSentenceCase(Name));
        }
    }
}