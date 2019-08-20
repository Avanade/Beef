// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// Represents the XML configuration.
    /// </summary>
    public class CodeGenConfig
    {
        /// <summary>
        /// The list of standard system <see cref="Type"/> names.
        /// </summary>
        public static readonly List<string> SystemTypes = new List<string>
        {
            "void", "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "string", "DateTime", "TimeSpan"
        };

        #region Static

        /// <summary>
        /// Creates the root and children configuration from the XML.
        /// </summary>
        /// <param name="codeGenerator">The <see cref="CodeGenerator"/>.</param>
        internal static void Create(CodeGenerator codeGenerator)
        {
            codeGenerator.Root = new CodeGenConfig(codeGenerator.ConfigXml.Name.LocalName, null);

            Load(codeGenerator, codeGenerator.Root, codeGenerator.ConfigXml, codeGenerator.Parameters);

            UpdateCountAndIndex(codeGenerator.Root);
        }

        /// <summary>
        /// Loads from an <see cref="XElement"/>.
        /// </summary>
        private static void Load(CodeGenerator codeGen, CodeGenConfig config, XElement xml, Dictionary<string, string> parameters = null)
        {
            // Add a SysId with a GUID for global uniqueness.
            config.AttributeAdd("SysId", Guid.NewGuid().ToString());

            // Load the attributes.
            foreach (XAttribute xa in xml.Attributes())
            {
                if (xa.Value != null)
                    config.AttributeUpdate(xa.Name.LocalName, xa.Value);
            }

            // Update/override the attribues with the parameters.
            if (config.Parent == null && parameters != null)
            {
                foreach (KeyValuePair<string, string> kvp in parameters)
                    config.AttributeUpdate(kvp.Key, kvp.Value);
            }

            // Before children load.
            if (codeGen.Loaders.ContainsKey(config.Name))
                codeGen.Loaders[config.Name].LoadBeforeChildren(config);

            // Load the children.
            foreach (XElement xmlChild in xml.Nodes().Where(x => x.NodeType == XmlNodeType.Element || x.NodeType == XmlNodeType.CDATA))
            {
                // Load CDATA nodes separately.
                if (xmlChild.NodeType == XmlNodeType.CDATA)
                {
                    config.CDATA = xmlChild.Value;
                    continue;
                }
                else if (xmlChild.NodeType != XmlNodeType.Element)
                    continue;

                CodeGenConfig child = new CodeGenConfig(xmlChild.Name.LocalName, config);
                Load(codeGen, child, xmlChild);

                if (!config.Children.ContainsKey(child.Name))
                    config.Children.Add(child.Name, new List<CodeGenConfig>());

                config.Children[child.Name].Add(child);
            }

            // After children load.
            if (codeGen.Loaders.ContainsKey(config.Name))
                codeGen.Loaders[config.Name].LoadAfterChildren(config);
        }

        /// <summary>
        /// Update the Count and Index attributes.
        /// </summary>
        internal static void UpdateCountAndIndex(CodeGenConfig config)
        {
            // Update the Count and Index attributes.
            foreach (KeyValuePair<string, List<CodeGenConfig>> kvp in config.Children)
            {
                config.AttributeUpdate(string.Format("{0}Count", kvp.Key), kvp.Value.Count.ToString());
                int index = 0;
                foreach (CodeGenConfig child in kvp.Value)
                {
                    child.AttributeUpdate(string.Format("{0}Index", child.Name), index++.ToString());
                    UpdateCountAndIndex(child);
                }
            }
        }

        /// <summary>
        /// Gets the value for the specified <see cref="XAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The resultant value <see cref="Type"/>.</typeparam>
        /// <param name="xml">The <see cref="XElement"/>.</param>
        /// <param name="name">The <see cref="XAttribute"/> name.</param>
        /// <param name="defaultVal">The default value where not found.</param>
        /// <param name="mandatory">Indicates that the attribute value is mandatory and must exist.</param>
        /// <returns>The attribute value.</returns>
        internal static T GetXmlVal<T>(XElement xml, string name, T defaultVal, bool mandatory)
        {
            if (xml.Attribute(name) == null)
            {
                if (mandatory)
                    throw new CodeGenException(string.Format("Attribute '{0}' value not found; is mandatory.", name));
                else
                    return defaultVal;
            }

            string val = xml.Attribute(name).Value;
            try
            {
                if (typeof(T) == typeof(string))
                {
                    if (val.Length == 0)
                    {
                        if (mandatory)
                            throw new CodeGenException(string.Format("Attribute '{0}' has no value; is mandatory.", name));
                        else
                            return defaultVal;
                    }
                    else
                        return (T)(object)val;
                }
                else if (typeof(T) == typeof(bool))
                    return (T)(object)XmlConvert.ToBoolean(val);
                else if (typeof(T) == typeof(int))
                    return (T)(object)XmlConvert.ToInt32(val);
                else if (typeof(T) == typeof(Enum))
                    return (T)Enum.Parse(typeof(T), val);
                else
                    throw new CodeGenException(string.Format("Attribute '{0} value can not be converted to Type {1}.", name, typeof(T).Name));
            }
            catch (FormatException fex)
            {
                throw new CodeGenException(string.Format("Attribute '{0}' value can not be converted to Type {1}: {2}", name, typeof(T).Name, fex.Message));
            }
            catch (ArgumentException aex)
            {
                throw new CodeGenException(string.Format("Attribute '{0}' value can not be converted to Type {1}: {2}", name, typeof(T).Name, aex.Message));
            }
        }

        /// <summary>
        /// Find the <see cref="CodeGenConfig"/> for the specified name by recursively looking up the stack.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> to start the search from.</param>
        /// <param name="name">The configuration element name to find.</param>
        /// <returns>The <see cref="CodeGenConfig"/> where found; otherwise, <c>null</c>.</returns>
        public static CodeGenConfig FindConfig(CodeGenConfig config, string name)
        {
            CheckParameters(config, name);

            if (config.Name == name)
                return config;

            if (config.Parent == null)
                return null;
            else
                return FindConfig(config.Parent, name);
        }

        /// <summary>
        /// Find the child <see cref="CodeGenConfig"/> list for the specified name by recursively looking up the stack.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> to start the search from.</param>
        /// <param name="name">The configuration element name to find.</param>
        /// <returns>The <see cref="CodeGenConfig"/> list where found; otherwise, <c>null</c>.</returns>
        public static List<CodeGenConfig> FindConfigList(CodeGenConfig config, string name)
        {
            CheckParameters(config, name);

            // Search children for named list.
            if (config.Children != null)
            {
                foreach (KeyValuePair<string, List<CodeGenConfig>> kvp in config.Children)
                {
                    if (kvp.Key == name)
                        return kvp.Value;
                }
            }

            // Search the parent for named list.
            if (config.Parent == null)
                return null;
            else
                return FindConfigList(config.Parent, name);
        }

        /// <summary>
        /// Find the <see cref="CodeGenConfig"/> list for the specified name by recursively looking down and across the stack.
        /// </summary>
        /// <param name="config">The <see cref="CodeGenConfig"/> to start the search from.</param>
        /// <param name="name">The configuration element name to find.</param>
        /// <returns>The <see cref="CodeGenConfig"/> list.</returns>
        public static List<CodeGenConfig> FindConfigAll(CodeGenConfig config, string name)
        {
            CheckParameters(config, name);

            List<CodeGenConfig> list = new List<CodeGenConfig>();

            if (config.Children != null)
            {
                foreach (KeyValuePair<string, List<CodeGenConfig>> kvp in config.Children)
                {
                    if (kvp.Key == name)
                        list.AddRange(kvp.Value);
                    else
                    {
                        foreach (CodeGenConfig item in kvp.Value)
                            list.AddRange(FindConfigAll(item, name));
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Check that the parameters are A-OK.
        /// </summary>
        private static void CheckParameters(CodeGenConfig config, string name)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null)
                throw new ArgumentNullException("name");
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenConfig"/> class.
        /// </summary>
        /// <param name="name">The configuration element name.</param>
        /// <param name="parent">The parent <see cref="CodeGenConfig"/>.</param>
        public CodeGenConfig(string name, CodeGenConfig parent)
        {
            if (string.IsNullOrEmpty(name))
                throw new CodeGenException("Configuration element name is not specified.");

            if (name == "Config" || name == "Header" || name == "Footer" || name == "Condition")
                throw new CodeGenException("Configuration element name 'Config', 'Header', 'Footer' and 'Condition' are reserved.");

            // Set name; root is always 'Config'.
            if (parent == null)
                Name = "Config";
            else
                Name = name;

            Parent = parent;
        }

        /// <summary>
        /// Gets the configuration element name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parent <see cref="CodeGenConfig"/>.
        /// </summary>
        public CodeGenConfig Parent { get; }


        /// <summary>
        /// Gets the root <see cref="CodeGenConfig"/>.
        /// </summary>
        public CodeGenConfig Root
        {
            get { return (Parent == null) ? this : Parent.Root; }
        }

        /// <summary>
        /// Gets the corresponding attribute values.
        /// </summary>
        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Adds/Updates the name/value pair into the <see cref="Attributes"/> collection overridding any previous value.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public void AttributeUpdate(string name, string value)
        {
            if (Attributes.ContainsKey(name))
                Attributes[name] = value;
            else
                Attributes.Add(name, value);
        }

        /// <summary>
        /// Adds the name/value pair into the <see cref="Attributes"/> collection only where an existing value does not exist.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public void AttributeAdd(string name, string value)
        {
            if (!Attributes.ContainsKey(name))
                Attributes.Add(name, value);
        }

        /// <summary>
        /// Gets the specified attribute value.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="name">The attribute name.</param>
        /// <returns>The corresponding value.</returns>
        public T GetAttributeValue<T>(string name)
        {
            if (!Attributes.ContainsKey(name))
                return default(T);

            string val = Attributes[name];
            if (string.IsNullOrEmpty(val))
                return default(T);

            return (T)Convert.ChangeType(val, typeof(T));
        }

        /// <summary>
        /// Gets the corresponding children <see cref="CodeGenConfig"/>.
        /// </summary>
        public Dictionary<string, List<CodeGenConfig>> Children { get; } = new Dictionary<string, List<CodeGenConfig>>();

        /// <summary>
        /// Gets the inner CDATA value if applicable.
        /// </summary>
        public string CDATA { get; set; } = null;
    }
}
