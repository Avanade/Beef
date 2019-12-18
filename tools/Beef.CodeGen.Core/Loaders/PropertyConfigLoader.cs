// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;

namespace Beef.CodeGen.Loaders
{
    /// <summary>
    /// Represents an <b>Property</b> configuration loader.
    /// </summary>
    public class PropertyConfigLoader : ICodeGenConfigLoader
    {
        /// <summary>
        /// Gets the loader name.
        /// </summary>
        public string Name { get { return "Property"; } }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> before the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadBeforeChildren(CodeGenConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.AttributeAdd("Type", "string");

            if (config.GetAttributeValue<string>("RefDataType") != null)
                config.AttributeAdd("Text", string.Format(System.Globalization.CultureInfo.InvariantCulture, "{1} (see {{{{{0}}}}})", config.Attributes["Type"], CodeGenerator.ToSentenceCase(config.Attributes["Name"])));
            else if (CodeGenConfig.SystemTypes.Contains(config.Attributes["Type"]))
                config.AttributeAdd("Text", CodeGenerator.ToSentenceCase(config.Attributes["Name"]));
            else
                config.AttributeAdd("Text", string.Format(System.Globalization.CultureInfo.InvariantCulture, "{1} (see {{{{{0}}}}})", config.Attributes["Type"], CodeGenerator.ToSentenceCase(config.Attributes["Name"])));

            config.AttributeUpdate("Text", config.Attributes["Text"]);

            config.AttributeAdd("StringTrim", "End");
            config.AttributeAdd("StringTransform", "EmptyToNull");
            config.AttributeAdd("DateTimeTransform", "DateTimeLocal");
            config.AttributeAdd("PrivateName", CodeGenerator.ToPrivateCase(config.Attributes["Name"]));
            config.AttributeAdd("ArgumentName", CodeGenerator.ToCamelCase(config.Attributes["Name"]));
            config.AttributeAdd("DisplayName", GenerateDisplayName(config));
        }

        /// <summary>
        /// Generates the display name (checks for Id and handles specifically).
        /// </summary>
        private string GenerateDisplayName(CodeGenConfig config)
        {
            var dn = CodeGenerator.ToSentenceCase(config.Attributes["Name"]);
            var parts = dn.Split(' ');
            if (parts.Length == 1)
                return (parts[0] == "Id") ? "Identifier" : dn;

            if (parts.Last() != "Id")
                return dn;

            var parts2 = new string[parts.Length - 1];
            Array.Copy(parts, parts2, parts.Length - 1);
            return string.Join(" ", parts2);
        }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> after the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadAfterChildren(CodeGenConfig config)
        {
        }
    }
}
