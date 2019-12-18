// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Loaders
{
    /// <summary>
    /// Represents an <b>Entity</b> configuration loader.
    /// </summary>
    public class EntityConfigLoader : ICodeGenConfigLoader, ICodeGenConfigGetLoaders
    {
        /// <summary>
        /// Gets the corresponding loaders.
        /// </summary>
        /// <returns>An <see cref="ICodeGenConfigLoader"/> array.</returns>
        public ICodeGenConfigLoader[] GetLoaders()
        {
            return new ICodeGenConfigLoader[]
            {
                new EntityConfigLoader(),
                new OperationConfigLoader(),
                new ParameterConfigLoader(),
                new PropertyConfigLoader()
            };
        }

        /// <summary>
        /// Gets the loader name.
        /// </summary>
        public string Name { get { return "Entity"; } }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> before the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadBeforeChildren(CodeGenConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.AttributeAdd("Text", CodeGenerator.ToSentenceCase(config.Attributes["Name"]));
            config.AttributeAdd("PrivateName", CodeGenerator.ToPrivateCase(config.Attributes["Name"]));
            config.AttributeAdd("ArgumentName", CodeGenerator.ToCamelCase(config.Attributes["Name"]));

            config.AttributeUpdate("Text", config.Attributes["Text"]);
            if (config.Attributes.ContainsKey("RefDataType"))
                config.AttributeAdd("ConstType", config.Attributes["RefDataType"]);

            config.AttributeAdd("Namespace", config.Root.GetAttributeValue<string>("Namespace"));
            config.AttributeAdd("FileName", config.Attributes["Name"]);
            config.AttributeAdd("EntityScope", "Common");
        }

        /// <summary>
        /// Loads the <see cref="CodeGenConfig"/> after the corresponding <see cref="CodeGenConfig.Children"/>.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> being loaded.</param>
        public void LoadAfterChildren(CodeGenConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var autoGet = config.GetAttributeValue<bool>("Get");
            var autoGetAll = config.GetAttributeValue<bool>("GetAll");
            var autoCreate = config.GetAttributeValue<bool>("Create");
            var autoUpdate = config.GetAttributeValue<bool>("Update");
            var autoPatch = config.GetAttributeValue<bool>("Patch");
            var autoDelete = config.GetAttributeValue<bool>("Delete");

            // Check quick Get/Create/Update/Delete configurations.
            var opsConfig = CodeGenConfig.FindConfigList(config, "Operation");
            if (opsConfig != null)
            {
                foreach (CodeGenConfig opConfig in opsConfig)
                {
                    switch (opConfig.GetAttributeValue<string>("Name"))
                    {
                        case "Get": autoGet = false; break;
                        case "GetAll": autoGetAll = false; break;
                        case "Create": autoCreate = false; break;
                        case "Update": autoUpdate = false; break;
                        case "Patch": autoPatch = false; break;
                        case "Delete": autoDelete = false; break;
                    }
                }
            }

            // Stop if no quick configs.
            if (!autoGet && !autoGetAll && !autoCreate && !autoUpdate && !autoDelete)
                return;

            // Where no operations already defined, need to add in the placeholder.
            if (opsConfig == null)
            {
                opsConfig = new List<CodeGenConfig>();
                config.Children.Add("Operation", opsConfig);
            }

            // Where ReferenceData is being generated; add an 'Id' property to enable the code gen to function.
            var refDataType = config.GetAttributeValue<string>("RefDataType");
            if (refDataType != null)
            {
                var propsConfig = CodeGenConfig.FindConfigList(config, "Property");
                if (propsConfig == null)
                {
                    propsConfig = new List<CodeGenConfig>();
                    config.Children.Add("Property", propsConfig);
                }

                ApplyRefDataProperties(config, propsConfig, refDataType);
            }

            // Determine the WebApiRout based on the unique key.
            var webApiRoute = string.Join("/", CodeGenConfig.FindConfigList(config, "Property").Where(x => x.GetAttributeValue<bool>("UniqueKey")).Select(x => "{" + x.GetAttributeValue<string>("ArgumentName") + "}"));
            if (string.IsNullOrEmpty(webApiRoute))
                return;

            // Add each operation.
            if (autoDelete)
            {
                CodeGenConfig o = new CodeGenConfig("Operation", config);
                o.AttributeAdd("Name", "Delete");
                o.AttributeAdd("OperationType", "Delete");
                o.AttributeAdd("UniqueKey", "true");
                o.AttributeAdd("WebApiRoute", webApiRoute);
                opsConfig.Insert(0, ApplyOperationLoader(o));
            }

            if (autoPatch)
            {
                CodeGenConfig o = new CodeGenConfig("Operation", config);
                o.AttributeAdd("Name", "Patch");
                o.AttributeAdd("OperationType", "Patch");
                o.AttributeAdd("UniqueKey", "true");
                o.AttributeAdd("WebApiRoute", webApiRoute);
                opsConfig.Insert(0, ApplyOperationLoader(o));
            }

            if (autoUpdate)
            {
                CodeGenConfig o = new CodeGenConfig("Operation", config);
                o.AttributeAdd("Name", "Update");
                o.AttributeAdd("OperationType", "Update");
                o.AttributeAdd("UniqueKey", "true");
                o.AttributeAdd("WebApiRoute", webApiRoute);

                if (refDataType != null)
                    o.AttributeAdd("ValidatorFluent", $"Entity(new ReferenceDataValidator<{config.GetAttributeValue<string>("Name")}>())");

                opsConfig.Insert(0, ApplyOperationLoader(o));
            }

            if (autoCreate)
            {
                CodeGenConfig o = new CodeGenConfig("Operation", config);
                o.AttributeAdd("Name", "Create");
                o.AttributeAdd("OperationType", "Create");
                o.AttributeAdd("UniqueKey", "false");
                o.AttributeAdd("WebApiRoute", "");

                if (refDataType != null)
                    o.AttributeAdd("ValidatorFluent", $"Entity(new ReferenceDataValidator<{config.GetAttributeValue<string>("Name")}>())");

                opsConfig.Insert(0, ApplyOperationLoader(o));
            }

            if (autoGet)
            {
                CodeGenConfig o = new CodeGenConfig("Operation", config);
                o.AttributeAdd("Name", "Get");
                o.AttributeAdd("OperationType", "Get");
                o.AttributeAdd("UniqueKey", "true");
                o.AttributeAdd("WebApiRoute", webApiRoute);
                opsConfig.Insert(0, ApplyOperationLoader(o));
            }

            if (autoGetAll)
            {
                CodeGenConfig o = new CodeGenConfig("Operation", config);
                o.AttributeAdd("Name", "GetAll");
                o.AttributeAdd("OperationType", "GetColl");
                o.AttributeAdd("WebApiRoute", "");
                opsConfig.Insert(0, ApplyOperationLoader(o));
            }
        }

        /// <summary>
        /// Applies the Operation Loader.
        /// </summary>
        private CodeGenConfig ApplyOperationLoader(CodeGenConfig config)
        {
            var opl = new OperationConfigLoader();
            opl.LoadBeforeChildren(config);
            opl.LoadAfterChildren(config);
            return config;
        }

        /// <summary>
        /// Applies standard and configured reference data properties.
        /// </summary>
        private void ApplyRefDataProperties(CodeGenConfig config, List<CodeGenConfig> propsConfig, string refDataType)
        {
            var i = 0;

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "Id"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "Id");
                p.AttributeAdd("ArgumentName", "id");
                p.AttributeAdd("Text", "{{" + config.GetAttributeValue<string>("Name") + "}} identifier");
                p.AttributeAdd("Type", refDataType);
                p.AttributeAdd("UniqueKey", "true");
                p.AttributeAdd("DataAutoGenerated", "true");
                p.AttributeAdd("DataName", config.GetAttributeValue<string>("Name") + "Id");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "Code"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "Code");
                p.AttributeAdd("Type", "string");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "Text"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "Text");
                p.AttributeAdd("Type", "string");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "IsActive"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "IsActive");
                p.AttributeAdd("Type", "bool");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "SortOrder"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "SortOrder");
                p.AttributeAdd("Type", "int");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "ETag"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "ETag");
                p.AttributeAdd("Type", "string");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            if (!propsConfig.Any(x => x.GetAttributeValue<string>("Name") == "ChangeLog"))
            {
                CodeGenConfig p = new CodeGenConfig("Property", config);
                p.AttributeAdd("Name", "ChangeLog");
                p.AttributeAdd("Type", "ChangeLog");
                p.AttributeAdd("Inherited", "true");
                propsConfig.Insert(i++, p);
            }

            var pns = config.GetAttributeValue<string>("RefDataProperties");
            if (!string.IsNullOrEmpty(pns))
            {
                foreach (var pn in pns.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    CodeGenConfig p = new CodeGenConfig(pn, config);
                    p.AttributeAdd("Name", pn);
                    p.AttributeAdd("Type", "ChangeLog");
                    p.AttributeAdd("Inherited", "true");
                    propsConfig.Insert(i++, p);
                }
            }
        }
    }
}
