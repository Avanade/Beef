// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// Represents the template XML used for code generation.
    /// </summary>
    internal sealed class CodeGenTemplate : IDisposable
    {
        private readonly CodeGenerator _codeGenerator;
        private readonly XElement _xmlTemplate;
        private XNode _xmlCurrent;
        private TextWriter _tw;
        private StringBuilder _sb;

        private CodeGeneratorEventArgs _eventArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenTemplate"/> class.
        /// </summary>
        /// <param name="codeGenerator">The owning <see cref="CodeGenerator"/>.</param>
        /// <param name="xmlTemplate">The template <see cref="XElement"/>.</param>
        public CodeGenTemplate(CodeGenerator codeGenerator, XElement xmlTemplate)
        {
            _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
            _xmlTemplate = xmlTemplate ?? throw new ArgumentNullException(nameof(xmlTemplate));

            if (xmlTemplate.Name != "Template")
                throw new ArgumentException("Template XML element root must be named 'Template'.");
        }

        /// <summary>
        /// Executes a <see cref="CodeGenTemplate"/> replacing the placeholders with their respective values.
        /// </summary>
        public void Execute()
        {
            // Get the generated directory name.
            _eventArgs = new CodeGeneratorEventArgs { OutputGenDirName = CodeGenConfig.GetXmlVal<string>(_xmlTemplate, "OutputGenDirName", null, false) };

            // Invoke the XML.
            ExecuteXml(_xmlTemplate, _codeGenerator.Root);
        }

        /// <summary>
        /// Executes the <see cref="XElement"/>.
        /// </summary>
        private void ExecuteXml(XElement xmlNode, CodeGenConfig config)
        {
            // Open the output file where appropriate.
            bool fileSpecified = GetOutputFileName(xmlNode, config);

            // Go through the XML nodes and output as defined.
            foreach (XNode xml in xmlNode.Nodes())
            {
                _xmlCurrent = xml;
                switch (xml.NodeType)
                {
                    case XmlNodeType.CDATA:
                        if (_tw == null)
                            OpenOutputWriter(config);

                        _tw.Write(TemplateReplace(((XCData)xml).Value, config));
                        break;

                    case XmlNodeType.Comment:
                        break;

                    case XmlNodeType.Element:
                        XElement xmlE = (XElement)xml;
                        switch (xmlE.Name.LocalName)
                        {
                            case "Header":
                            case "Footer":
                                // Ignore as these are handled separately.
                                break;

                            case "If":
                                ExecuteIf(xmlE, config);
                                break;

                            case "Set":
                                ExecuteSet(xmlE, config);
                                break;

                            case "Increment":
                                ExecuteIncrement(xmlE, config);
                                break;

                            case "Exception":
                                ExecuteException(xmlE, config);
                                break;

                            case "ForEachList":
                                ExecuteForEachListList(xmlE, config);
                                break;

                            case "Switch":
                                ExecuteSwitch(xmlE, config);
                                break;

                            case "Config":
                                throw new CodeGenException($"Unexpected XML Element '{xmlE.Name.LocalName}' encountered.");

                            default:
                                List<CodeGenConfig> configList;

                                if (xmlE.Attribute("FullSearch") != null && xmlE.Attribute("FullSearch").Value.ToUpperInvariant() == "TRUE")
                                    configList = CodeGenConfig.FindConfigAll(_codeGenerator.Root, xmlE.Name.LocalName);
                                else
                                    configList = CodeGenConfig.FindConfigList(config, xmlE.Name.LocalName);

                                if (configList != null)
                                {
                                    string prevIndex = _codeGenerator.System.Attributes["Index"];
                                    int index = 0;
                                    foreach (CodeGenConfig item in configList)
                                    {
                                        if (ExecuteIfCondition((XElement)xml, item))
                                        {
                                            _codeGenerator.System.AttributeUpdate("Index", index.ToString(CultureInfo.InvariantCulture));
                                            ExecuteXml(xmlE, item);
                                            index++;
                                        }
                                    }

                                    _codeGenerator.System.Attributes["Index"] = prevIndex;
                                }

                                break;
                        }

                        break;

                    default:
                        throw new CodeGenException($"Unexpected XML Node Type '{xml.NodeType}' encountered: '{xml.ToString()}' - '{xml.Parent.ToString()}'.");
                }
            }

            // Where we opened the output file, then we should also close it when finished.
            if (fileSpecified)
                CloseOutputWriter(config);
        }

        /// <summary>
        /// Gets the generated output file name where specified.
        /// </summary>
        private bool GetOutputFileName(XElement xml, CodeGenConfig config)
        {
            // Check if a file name has been specified.
            string fileName = CodeGenConfig.GetXmlVal<string>(xml, "OutputFileName", null, false);
            if (fileName == null)
                return false;

            // Record the specified file name.
            if (fileName.Length == 0)
                throw new CodeGenException("'OutputFileName' attribute has no value; this must be specified.");

            if (_eventArgs.OutputFileName != null)
                throw new CodeGenException("'OutputFileName' attribute unexpected; only one output file can be open at one time.");

            string dirName = CodeGenConfig.GetXmlVal<string>(xml, "OutputDirName", null, false);
            if (dirName != null && dirName.Length > 0)
                _eventArgs.OutputDirName = TemplateReplace(dirName, config);

            _eventArgs.OutputFileName = TemplateReplace(fileName, config);

            string isOutputNewOnly = CodeGenConfig.GetXmlVal<string>(xml, "IsOutputNewOnly", null, false);
            if (!string.IsNullOrEmpty(isOutputNewOnly))
            {
                var val = GetValue(isOutputNewOnly, config);
                if (val != null)
                    _eventArgs.IsOutputNewOnly = val is bool ? (bool)val : throw new CodeGenException($"'IsOutputNewOnly' attribute value '{isOutputNewOnly}' does not result in a boolean value.");
            }

            _eventArgs.IsOutputNewOnly = false;
            return true;
        }

        /// <summary>
        /// Open the generated output writer.
        /// </summary>
        private void OpenOutputWriter(CodeGenConfig config)
        {
            if (_eventArgs.OutputFileName == null)
                throw new CodeGenException("Can not write CDATA as a preceeding 'OutputFileName' attribute has not been specified.");

            // Create the string writer.
            _sb = new StringBuilder();
            _tw = new StringWriter(_sb);

            // Output the header.
            ExecuteTemplateNamed("Header", config);
        }

        /// <summary>
        /// Close the generated output writer and raise the code generated event.
        /// </summary>
        private void CloseOutputWriter(CodeGenConfig config)
        {
            if (_tw != null)
            {
                // Output the footer.
                ExecuteTemplateNamed("Footer", config);

                // Close and cleanup.
                _tw.Flush();
                _tw.Dispose();
                _tw = null;

                // Raise the code generated event.
                _eventArgs.Content = Regex.Replace(_sb.ToString(), "(?<!\r)\n", "\r\n");
                _codeGenerator.RaiseCodeGenerated(_eventArgs);
            }

            // Initialize for a potential subsequent file.
            _sb = null;
            _eventArgs = new CodeGeneratorEventArgs { OutputGenDirName = _eventArgs.OutputGenDirName };
        }

        /// <summary>
        /// Executes a template level execution for a named element.
        /// </summary>
        private void ExecuteTemplateNamed(string name, CodeGenConfig config)
        {
            var xml = _xmlTemplate.Elements(name).SingleOrDefault();
            if (xml != null)
                ExecuteXml(xml, config);
        }

        /// <summary>
        /// Invoke an 'If' wtih a 'Then' and an 'Else'.
        /// </summary>
        private void ExecuteIf(XElement xmlCon, CodeGenConfig config)
        {
            try
            {
                XElement thenXml = xmlCon.Elements("Then").SingleOrDefault();
                XElement elseXml = xmlCon.Elements("Else").SingleOrDefault();

                if (ExecuteIfCondition(xmlCon, config))
                {
                    if (thenXml != null)
                        ExecuteXml(thenXml, config);
                    else if (elseXml == null)
                        ExecuteXml(xmlCon, config);
                }
                else if (elseXml != null)
                    ExecuteXml(elseXml, config);
            }
            catch (CodeGenException) { throw; }
            catch (Exception ex) { throw new CodeGenException($"If/Then/Else element is invalid ({ex.Message}): {xmlCon.ToString()}."); }
        }

        /// <summary>
        /// Invoke an 'If' style condition.
        /// </summary>
        private bool ExecuteIfCondition(XElement xmlCon, CodeGenConfig config)
        {
            string condition = CodeGenConfig.GetXmlVal<string>(xmlCon, "Condition", null, false);
            if (string.IsNullOrEmpty(condition))
                return true;

            bool notCondition = CodeGenConfig.GetXmlVal<bool>(xmlCon, "Not", false, false);

            string[] parts = condition.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> stmt = new List<string>();

            foreach (string part in parts)
            {
                switch (part.ToUpperInvariant())
                {
                    case "AND":
                    case "&&":
                        if (!ExecuteIfConditionStmt(stmt, config, condition))
                            return ApplyNotCondition(false, notCondition);

                        stmt.Clear();
                        break;

                    case "OR":
                    case "||":
                        if (ExecuteIfConditionStmt(stmt, config, condition))
                            return ApplyNotCondition(true, notCondition);

                        stmt.Clear();
                        break;

                    default:
                        stmt.Add(part);
                        break;
                }
            }

            if (stmt.Count > 0)
                return ApplyNotCondition(ExecuteIfConditionStmt(stmt, config, condition), notCondition);

            throw new CodeGenException($"Condition is invalid: {condition}.");
        }

        /// <summary>
        /// Apply Not condition (reverse condition result).
        /// </summary>
        private static bool ApplyNotCondition(bool result, bool not)
        {
            return (!not) ? result : !result;
        }

        /// <summary>
        /// Invoke the condition sub statement and manage any errors.
        /// </summary>
        private bool ExecuteIfConditionStmt(List<string> stmt, CodeGenConfig config, string condition)
        {
            try
            {
                bool? stmtResult = ExecuteIfConditionStmt(stmt, config);
                if (stmtResult == null)
                    throw new CodeGenException($"Condition is invalid: {condition}.");

                return stmtResult.Value;
            }
            catch (CodeGenException) { throw; }
            catch (Exception) { throw new CodeGenException($"Condition is invalid: {condition}."); }
        }

        /// <summary>
        /// Invoke the condition sub statement.
        /// </summary>
        private bool? ExecuteIfConditionStmt(List<string> stmt, CodeGenConfig config)
        {
            if (stmt.Count != 1 && stmt.Count != 3)
                return null;

            object lVal = GetValue(stmt[0], config);
            if (stmt.Count == 1)
            {
                if (lVal is bool)
                    return (bool)lVal;

                if (lVal is string slVal)
                {
                    if (slVal.ToUpperInvariant() == "TRUE")
                        return true;
                    else if (slVal.ToUpperInvariant() == "FALSE")
                        return false;
                }

                // Not good!
                return null;
            }

            if (string.IsNullOrEmpty(stmt[2]))
                return null;

            object rVal = GetValue(stmt[2], config);

            if (lVal == null && rVal is bool)
                lVal = false;
            else if (lVal is bool && rVal == null)
                rVal = false;

            if (lVal == null && rVal is decimal)
                lVal = 0f;
            else if (lVal is decimal && rVal == null)
                rVal = 0f;

            int res = 0;
            if (lVal != null || rVal != null)
            {
                if (lVal is bool || rVal is bool)
                    res = Comparer<bool>.Default.Compare((bool)Convert.ChangeType(lVal, typeof(bool), CultureInfo.InvariantCulture), (bool)Convert.ChangeType(rVal, typeof(bool), CultureInfo.InvariantCulture));
                else if (lVal is decimal || rVal is decimal)
                    res = Comparer<decimal>.Default.Compare((decimal)Convert.ChangeType(lVal, typeof(decimal), CultureInfo.InvariantCulture), (decimal)Convert.ChangeType(rVal, typeof(decimal), CultureInfo.InvariantCulture));
                else if (lVal is int || rVal is int)
                    res = Comparer<int>.Default.Compare((int)Convert.ChangeType(lVal, typeof(int), CultureInfo.InvariantCulture), (int)Convert.ChangeType(rVal, typeof(int), CultureInfo.InvariantCulture));
                else
                    res = Comparer<string>.Default.Compare((string)Convert.ChangeType(lVal, typeof(string), CultureInfo.InvariantCulture), (string)Convert.ChangeType(rVal, typeof(string), CultureInfo.InvariantCulture));
            }

            switch (stmt[1].ToUpperInvariant())
            {
                case "==":
                    return (res == 0);

                case "!=":
                    return (res != 0);

                case ">":
                    return (res > 0);

                case ">=":
                    return (res >= 0);

                case "<":
                    return (res < 0);

                case "<=":
                    return (res <= 0);

                case "CONTAINS":
                    if (!(lVal is string slVal))
                        throw new CodeGenException("LVal is not string; required for a 'contains' condition.");

                    if (!(rVal is string srVal))
                        throw new CodeGenException("RVal is not string; required for a 'contains' condition.");

                    return slVal.Contains(srVal, StringComparison.InvariantCulture);
            }

            return null;
        }

        /// <summary>
        /// Gets the value from a string.
        /// </summary>
        private object GetValue(string value, CodeGenConfig config)
        {
            if (value == null)
                return null;

            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Check for standard strings.
            switch (value.ToUpperInvariant())
            {
                case "TRUE":
                    return true;

                case "FALSE":
                    return false;

                case "NULL":
                    return null;
            }

            // Check for a string constant.
            if (value.Length > 1 && value.StartsWith("'", StringComparison.InvariantCultureIgnoreCase) && value.EndsWith("'", StringComparison.InvariantCultureIgnoreCase))
                return TemplateReplace(value.Substring(1, value.Length - 2), config);

            // Check and see if it is a number constant.
            if (decimal.TryParse(value, out decimal fVal))
                return fVal;

            // Finally, it must be a parameter value.
            return GetConfigValue(value, config);
        }

        /// <summary>
        /// Invoke the switch-case command.
        /// </summary>
        private void ExecuteSwitch(XElement xml, CodeGenConfig config)
        {
            var lval = xml.Attribute("Value")?.Value;
            if (string.IsNullOrEmpty(lval))
                throw new CodeGenException("Switch element has no 'Value' attribute specified.", xml.ToString());

            if (!ExecuteIfCondition(xml, config))
                return;

            foreach (var cXml in xml.Elements("Case"))
            {
                string rval = cXml.Attribute("Value")?.Value;
                if (string.IsNullOrEmpty(rval))
                    throw new CodeGenException("Case element has no 'Value' attribute specified.", xml.ToString());

                var stmt = new List<string> { lval, "==", rval };
                var stmtResult = ExecuteIfConditionStmt(stmt, config);
                if (stmtResult == null)
                    throw new CodeGenException(string.Format(CultureInfo.InvariantCulture, "Switch-case conditional statement is invalid: {0}.", string.Join(" ", stmt)), xml.ToString());

                if (!stmtResult.Value)
                    continue;

                ExecuteXml(cXml, config);
                return;
            }

            var dXml = xml.Elements("Default").FirstOrDefault();
            if (dXml != null)
                ExecuteXml(dXml, config);
        }

        /// <summary>
        /// Invoke the set command.
        /// </summary>
        private void ExecuteSet(XElement xml, CodeGenConfig config)
        {
            string name = xml.Attribute("Name")?.Value;
            if (string.IsNullOrEmpty(name))
                throw new CodeGenException("Set element has no 'Name' attribute specified.", xml.ToString());

            if (ExecuteIfCondition(xml, config))
                SetConfigValue(name, config, xml.Attribute("Value")?.Value);
            else
            {
                var otherwise = xml.Attribute("Otherwise")?.Value;
                if (otherwise != null)
                    SetConfigValue(name, config, otherwise);
            }
        }

        /// <summary>
        /// Transforms the value.
        /// </summary>
        private string Transform(string transform, string value)
        {
            if (string.IsNullOrEmpty(transform) || string.IsNullOrEmpty(value))
                return value;

            return (transform.ToUpperInvariant()) switch
            {
#pragma warning disable CA1308 // This is an intended "to lower" action.
                "TOLOWERCASE" => value.ToLowerInvariant(),
#pragma warning restore CA1308
                "TOUPPERCASE" => value.ToUpperInvariant(),
                "TOPRIVATECASE" => CodeGenerator.ToPrivateCase(value),
                "TOARGUMENTCASE" => CodeGenerator.ToCamelCase(value),
                "TOSENTENCECASE" => CodeGenerator.ToSentenceCase(value),
                "TOPASCALCASE" => CodeGenerator.ToPascalCase(value),
                "TOCAMELCASE" => CodeGenerator.ToCamelCase(value),
                "TOSNAKECASE" => CodeGenerator.ToSnakeCase(value),
                "TOKEBABCASE" => CodeGenerator.ToKebabCase(value),
                "TOPLURAL" => CodeGenerator.ToPlural(value),
                "TOCOMMENTS" => CodeGenerator.ToComments(value),
                "TOSEECOMMENTS" => CodeGenerator.ToSeeComments(value),
                _ => throw new CodeGenException($"Transform operation {transform} is not valid.", _xmlCurrent.ToString()),
            };
        }

        /// <summary>
        /// Invoke the increment command.
        /// </summary>
        private void ExecuteIncrement(XElement xml, CodeGenConfig config)
        {
            string name = xml.Attribute("Name")?.Value;
            if (string.IsNullOrEmpty(name))
                throw new CodeGenException("Increment element has no 'Name' attribute specified.", xml.ToString());

            object lval = GetValue(name, config);
            decimal dlval = 0m;
            if (lval != null)
            {
                if (lval is decimal)
                    dlval = (decimal)lval;
                else
                    throw new CodeGenException($"Increment 'Name' attribute value '{lval}' is not a valid decimal", xml.ToString());
            }

            var value = xml.Attribute("Value")?.Value;
            decimal drval = 1m;
            if (!string.IsNullOrEmpty(value))
            {
                object rval = GetValue(value, config);
                if (rval != null)
                {
                    if (rval is decimal)
                        drval = (decimal)rval;
                    else
                        throw new CodeGenException($"Increment 'Value' attribute value '{rval}' is not a valid decimal", xml.ToString());
                }
            }

            if (ExecuteIfCondition(xml, config))
            {
                dlval += drval;
                SetConfigValue(name, config, dlval.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Invoke the foreach list command.
        /// </summary>
        private void ExecuteForEachListList(XElement xml, CodeGenConfig config)
        {
            string name = xml.Attribute("Name")?.Value;
            if (string.IsNullOrEmpty(name))
                throw new CodeGenException("ForEachList element has no 'Name' attribute specified.", xml.ToString());

            object val = GetValue(name, config);
            if (val == null)
                return;

            if (!(val is string sval))
                throw new CodeGenException($"ForEachList 'Name' attribute value '{val}' is not a valid string", xml.ToString());

            var list = sval.Split(new string[] { xml.Attribute("Separator")?.Value ?? "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            if (list != null && list.Length > 0)
            {
                _codeGenerator.System.Attributes.TryGetValue("Value", out string prevValue);
                _codeGenerator.System.Attributes.TryGetValue("Index", out string prevIndex);
                int index = 0;
                foreach (var item in list)
                {
                    _codeGenerator.System.Attributes["Value"] = item;
                    if (ExecuteIfCondition(xml, config))
                    {
                        _codeGenerator.System.AttributeUpdate("Index", index.ToString(CultureInfo.InvariantCulture));
                        ExecuteXml(xml, config);
                        index++;
                    }
                }

                _codeGenerator.System.Attributes["Index"] = prevIndex;
                _codeGenerator.System.Attributes["Value"] = prevValue;
            }
        }

        /// <summary>
        /// Invoke the exception command.
        /// </summary>
        private void ExecuteException(XElement xml, CodeGenConfig config)
        {
            string message = xml.Attribute("Message")?.Value;
            if (string.IsNullOrEmpty(message))
                throw new CodeGenException("Exception element has no 'Message' attribute specified.", xml.ToString());

            if (ExecuteIfCondition(xml, config))
            {
                throw new CodeGenException($"Template Exception: {TemplateReplace(message, config)}");
            }
        }

        /// <summary>
        /// Replaces a "{{name}}" with the appropriate config value.
        /// </summary>
        private string TemplateReplace(string value, CodeGenConfig config)
        {
            string temp = value;
            int start;
            int end;

            if (temp == null)
                return temp;

            while (true)
            {
                start = temp.IndexOf("{{", StringComparison.InvariantCultureIgnoreCase);
                end = temp.IndexOf("}}", StringComparison.InvariantCultureIgnoreCase);

                if (start < 0 && end < 0)
                    return temp;

                if (start < 0 || end < 0 || end < start)
                    throw new CodeGenException("Start and End {{ }} parameter mismatch.", _xmlCurrent.ToString());

                string fullName = temp.Substring(start, end - start + 2);
                string fName = temp.Substring(start + 2, end - start - 2);
                var fValue = GetConfigValue(fName, config);

                temp = temp.Replace(fullName, fValue ?? "", StringComparison.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the config value for a specified parameter.
        /// </summary>
        private string GetConfigValue(string name, CodeGenConfig config)
        {
            if (name.StartsWith("$", StringComparison.InvariantCultureIgnoreCase))
                return TemplateReplace(name.Substring(1), config);

            CodeGenConfig val = GetConfig(name, config, out string propertyName);

            var parts = propertyName.Split(':');
            var value = val.Attributes.ContainsKey(parts[0]) ? val.Attributes[parts[0]] : null;

            for (int i = 1; i < parts.Length; i++)
            {
                value = Transform(parts[i], value);
            }

            return value;
        }

        /// <summary>
        /// Sets the config value for a specified parameter.
        /// </summary>
        private void SetConfigValue(string name, CodeGenConfig config, string value)
        {
            CodeGenConfig val = GetConfig(name, config, out string propertyName);
            var oval = GetValue(value, config);
            string sval = (oval == null) ? null : ((oval is bool) ? ((bool)oval ? "true" : "false") : oval.ToString());
            val.AttributeUpdate(propertyName, TemplateReplace(sval, config));
        }

        /// <summary>
        /// Get the specified <see cref="CodeGenConfig"/> and property name for a specified parameter.
        /// </summary>
        private CodeGenConfig GetConfig(string name, CodeGenConfig config, out string propertyName)
        {
            string[] parts = name.Split('.');
            if (parts.Length != 2)
                throw new CodeGenException($"Parameter '{name}' is invalid.", _xmlCurrent.ToString());

            CodeGenConfig val;
            if (parts[0] == "System")
                val = _codeGenerator.System;
            else
                val = CodeGenConfig.FindConfig(config, parts[0]);

            if (val == null)
                throw new CodeGenException($"Parameter '{name}' is invalid.", _xmlCurrent.ToString());

            propertyName = parts[1];
            return val;
        }

        public void Dispose()
        {
            if (_tw != null)
            {
                _tw.Dispose();
                _tw = null;
            }
        }
    }
}