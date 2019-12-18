// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Loaders
{
    /// <summary>
    /// Represents an <b>Operation</b> configuration loader.
    /// </summary>
    public class OperationConfigLoader : ICodeGenConfigLoader
    {
        /// <summary>
        /// Gets the loader name.
        /// </summary>
        public string Name { get { return "Operation"; } }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> before the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadBeforeChildren(CodeGenConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            CodeGenConfig entity = CodeGenConfig.FindConfig(config, "Entity");

            if (config.GetAttributeValue<bool>("UniqueKey"))
            {
                List<CodeGenConfig> paramList = new List<CodeGenConfig>();
                var layerPassing = new string[] { "Create", "Update" }.Contains(config.GetAttributeValue<string>("OperationType")) ? "ToManagerSet" : "All";
                var isMandatory = new string[] { "Create", "Update" }.Contains(config.GetAttributeValue<string>("OperationType")) ? "false" : "true";

                var propConfigs = CodeGenConfig.FindConfigList(config, "Property") ?? new List<CodeGenConfig>();
                foreach (CodeGenConfig propConfig in propConfigs)
                {
                    if (!propConfig.GetAttributeValue<bool>("UniqueKey"))
                        continue;

                    CodeGenConfig p = new CodeGenConfig("Parameter", config);
                    p.AttributeAdd("Name", propConfig.GetAttributeValue<string>("Name"));
                    p.AttributeAdd("LayerPassing", layerPassing);
                    p.AttributeAdd("IsMandatory", isMandatory);
                    ParameterConfigLoader.UpdateConfigFromProperty(p, propConfig.GetAttributeValue<string>("Name"));
                    paramList.Add(p);
                }

                if (paramList.Count > 0)
                    config.Children.Add("Parameter", paramList);
            }

            config.AttributeAdd("OperationType", "Get");
            config.AttributeAdd("ReturnType", "void");

            if (config.Attributes.ContainsKey("ReturnType"))
                config.AttributeAdd("ReturnText", string.Format(System.Globalization.CultureInfo.InvariantCulture, "A resultant {{{{{0}}}}}.", config.Attributes["ReturnType"]));
            else
                config.AttributeAdd("ReturnText", string.Format(System.Globalization.CultureInfo.InvariantCulture, "A resultant {{{{{0}}}}}.", entity.Attributes["Name"]));
          
            config.AttributeUpdate("ReturnText", config.Attributes["ReturnText"]);

            if (config.Attributes.ContainsKey("OperationType") && config.Attributes["OperationType"] == "Custom")
                config.AttributeAdd("Text", CodeGenerator.ToSentenceCase(config.Attributes["Name"]));

            if (config.Attributes.ContainsKey("Text"))
                config.AttributeUpdate("Text", config.Attributes["Text"]);

            config.AttributeAdd("PrivateName", CodeGenerator.ToPrivateCase(config.Attributes["Name"]));
            config.AttributeAdd("ArgumentName", CodeGenerator.ToCamelCase(config.Attributes["Name"]));
            config.AttributeAdd("QualifiedName", entity.Attributes["Name"] + config.Attributes["Name"]);
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
