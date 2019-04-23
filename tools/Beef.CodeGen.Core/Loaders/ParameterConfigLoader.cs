// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;

namespace Beef.CodeGen.Loaders
{
    /// <summary>
    /// Represents an <b>Parameter</b> configuration loader.
    /// </summary>
    public class ParameterConfigLoader : ICodeGenConfigLoader
    {
        /// <summary>
        /// Gets the loader name.
        /// </summary>
        public string Name { get { return "Parameter"; } }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> before the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadBeforeChildren(CodeGenConfig config)
        {
            if (config.Attributes.ContainsKey("Property"))
                UpdateConfigFromProperty(config, config.Attributes["Property"]);

            config.AttributeAdd("PrivateName", CodeGenerator.ToPrivateCase(config.Attributes["Name"]));
            config.AttributeAdd("ArgumentName", CodeGenerator.ToCamelCase(config.Attributes["Name"]));
            config.AttributeAdd("Type", "string");
            config.AttributeAdd("LayerPassing", "All");

            if (config.GetAttributeValue<string>("RefDataType") != null)
                config.AttributeAdd("Text", string.Format("{1} (see {{{{{0}}}}})", config.Attributes["Type"], CodeGenerator.ToSentenceCase(config.Attributes["Name"])));
            else if (CodeGenConfig.SystemTypes.Contains(config.Attributes["Type"]))
                config.AttributeAdd("Text", CodeGenerator.ToSentenceCase(config.Attributes["Name"]));
            else
                config.AttributeAdd("Text", string.Format("{1} (see {{{{{0}}}}})", config.Attributes["Type"], CodeGenerator.ToSentenceCase(config.Attributes["Name"])));

            config.AttributeUpdate("Text", config.Attributes["Text"]);
        }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> after the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadAfterChildren(CodeGenConfig config)
        {
        }

        /// <summary>
        /// Update the <see cref="CodeGenConfig"/> from a named <b>Property</b>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> to update.</param>
        /// <param name="propertyName">The <b>Property</b> name.</param>
        public static void UpdateConfigFromProperty(CodeGenConfig config, string propertyName)
        {
            List<CodeGenConfig> propConfig = CodeGenConfig.FindConfigList(config, "Property");
            if (propConfig == null)
                throw new CodeGenException(string.Format("Attribute value references Property '{0}' that does not exist for Entity.", propertyName));

            CodeGenConfig itemConfig = null;
            foreach (CodeGenConfig p in propConfig)
            {
                if (p.Attributes["Name"] == propertyName)
                    itemConfig = p;
            }

            if (itemConfig == null)
                throw new CodeGenException(string.Format("Attribute value references Property '{0}' that does not exist for Entity.", propertyName));

            config.AttributeAdd("ArgumentName", CodeGenerator.ToCamelCase(config.Attributes["Name"]));
            config.AttributeAdd("Text", itemConfig.Attributes["Text"]);
            config.AttributeAdd("Type", itemConfig.Attributes["Type"]);
            config.AttributeAdd("LayerPassing", "All");

            if (itemConfig.Attributes.ContainsKey("Nullable"))
                config.AttributeAdd("Nullable", itemConfig.Attributes["Nullable"]);

            if (itemConfig.Attributes.ContainsKey("RefDataType"))
                config.AttributeAdd("RefDataType", itemConfig.Attributes["RefDataType"]);

            if (itemConfig.Attributes.ContainsKey("DataConverter"))
                config.AttributeAdd("DataConverter", itemConfig.Attributes["DataConverter"]);

            if (itemConfig.Attributes.ContainsKey("IsDataConverterGeneric"))
                config.AttributeAdd("IsDataConverterGeneric", itemConfig.Attributes["IsDataConverterGeneric"]);

            if (itemConfig.Attributes.ContainsKey("UniqueKey"))
                config.AttributeAdd("UniqueKey", itemConfig.Attributes["UniqueKey"]);
        }
    }
}
