using CoreEx;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using OnRamp;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beef.CodeGen.OpenApi
{
    /// <summary>
    /// Provides the ability to convert an OpenAPI document to an equivalent Beef-5 YAML configuration.
    /// </summary>
    internal class OpenApiConverter
    {
        private static readonly char[] _separators = "^&*$!@#%*{}[]|:;\"'_- .,".ToCharArray();
        private static readonly char[] _yamlEscapeChars = "!@#%*{}[]|:;\"'".ToCharArray();

        private readonly OpenApiDocument _document;
        private readonly YamlConfig _config;
        private readonly CodeGeneratorArgs _codeGenArgs;
        private readonly OpenApiArgs _openApiArgs;
        private readonly string[] _refDataTypes;
        private readonly string[] _ignoreTypes;
        private readonly bool _outputText = true;

        /// <summary>
        /// Read the OpenAPI document and create the <see cref="OpenApiConverter"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/>.</param>
        /// <param name="path">The OpenAPI document path.</param>
        /// <returns>The <see cref="OpenApiConverter"/>.</returns>
        public static async Task<OpenApiConverter> ReadAsync(CodeGeneratorArgs args, string path)
        {
            var oasr = new OpenApiStreamReader(new OpenApiReaderSettings { });
            OpenApiDocument oad;
            OpenApiDiagnostic oadi;

            try
            {
                if (File.Exists(path))
                {
                    using var sr = File.OpenRead(path);
                    oad = oasr.Read(sr, out oadi);
                }
                else
                {
                    using var hr = new HttpClient();
                    using var sr = await hr.GetStreamAsync(path).ConfigureAwait(false);
                    oad = oasr.Read(sr, out oadi);
                }
            }
            catch (Exception ex)
            {
                throw new CodeGenException($"The OpenAPI document path '{path}' is invalid.", ex);
            }

            if (oadi.Errors.Count > 0)
            {
                var errors = string.Join(Environment.NewLine, oadi.Errors.Select(x => x.ToString()));
                throw new CodeGenException($"The OpenAPI document '{path}' contains errors:{Environment.NewLine}{errors}");
            }

            return new OpenApiConverter(path, args, oad);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiConverter"/> class.
        /// </summary>
        private OpenApiConverter(string path, CodeGeneratorArgs args, OpenApiDocument document)
        {
            _document = document;
            _codeGenArgs = args ?? throw new ArgumentNullException(nameof(args));
            _openApiArgs = _codeGenArgs.GetOpenApiArgs();

            _refDataTypes = _codeGenArgs.GetParameter<string>("refdata")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
            _ignoreTypes = _codeGenArgs.GetParameter<string>("ignore")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
            _outputText = !(_codeGenArgs.Parameters.TryGetValue("text", out var val) && val is string text && text.Equals("false", StringComparison.OrdinalIgnoreCase));

            var sb = new StringBuilder($"openapi \"{path}\"");
            foreach (var p in args.Parameters.Where(x => new List<string> { "include", "exclude", "ignore", "refdata", "text" }.Contains(x.Key, StringComparer.OrdinalIgnoreCase)))
            {
                sb.Append($" --param \"{p.Key}={p.Value}\"");
            }

            _config = new(sb.ToString(), _openApiArgs);
        }

        /// <summary>
        /// Performs the conversion and outputs the YAML configuration.
        /// </summary>
        /// <returns>The YAML configuration <see cref="FileInfo"/>.</returns>
        public async Task<FileInfo> ConvertAsync()
        {
            const string templateFileName = "OpenApi_yaml";

            // Create the owning beef "entity" to house all the operations.
            var ename = GetDotNetName(_document.Info.Title) + "Service";
            var entity = new YamlEntity(string.IsNullOrWhiteSpace(ename) ? "NoTitleSpecifiedService" : ename);
            if (_outputText)
                entity.AddAttribute("text", _document.Info.Description);

            static (string? method, string path) MethodPathSplitter(string text)
            {
                var parts = text.Trim().Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1 || parts.Length > 2)
                    throw new CodeGenException($"The parameter must be in the format 'path' or 'method:path': {text}");

                return parts.Length == 1 ? (null, parts[0]) : (parts[0], parts[1]);
            }

            // Filter and process each underlying API path (operation).
            var exclude = (_codeGenArgs.GetParameter<string>("exclude")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? []).Select(MethodPathSplitter).ToArray();
            var include = (_codeGenArgs.GetParameter<string>("include")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? []).Select(MethodPathSplitter).ToArray();

            foreach (var oap in _document.Paths)
            {
                foreach (var ooo in oap.Value.Operations)
                {
                    if (exclude.Any(x => (x.method is null || x.method.Equals(ooo.Key.ToString(), StringComparison.OrdinalIgnoreCase)) && oap.Key.StartsWith(x.path, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    if (include.Length == 0 || include.Any(x => (x.method is null || x.method.Equals(ooo.Key.ToString(), StringComparison.OrdinalIgnoreCase)) && oap.Key.StartsWith(x.path, StringComparison.OrdinalIgnoreCase)))
                        CreateOperation(entity, new ApiOperation(oap.Key, ooo.Key, ooo.Value));
                }
            }

            // Sort all the entities then add the owning entity so it appears first.
            _config.Entities.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));
            _config.Entities!.Insert(0, entity);

            _config.Enums.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

            // Finalize the configurations.
            _openApiArgs.OnYamlPrimaryEntity?.Invoke(entity, _document);
            _openApiArgs.OnYamlConfig?.Invoke(_config, _document);

            // Set the path/filename.
            var fn = Path.Combine(OnRamp.Console.CodeGenConsole.GetBaseExeDirectory(), CodeGenFileManager.TemporaryEntityFilename);
            var fi = new FileInfo(fn);

            // Execute the code-generation proper and write contents (new or overwrite).
            using var sr = StreamLocator.GetTemplateStreamReader(templateFileName, [.. _codeGenArgs.Assemblies], StreamLocator.HandlebarsExtensions).StreamReader!;
            await File.WriteAllTextAsync(fi.FullName, new HandlebarsCodeGenerator(sr).Generate(_config)).ConfigureAwait(false);

            return fi;
        }

        /// <summary>
        /// Converts the operation.
        /// </summary>
        private void CreateOperation(YamlEntity entity, ApiOperation ao)
        {
            var operation = new YamlOperation(ao.Operation.OperationId);

            operation.AddAttribute("type", ao.Method switch
            {
                Microsoft.OpenApi.Models.OperationType.Get => "Get",
                Microsoft.OpenApi.Models.OperationType.Post => "Create",
                Microsoft.OpenApi.Models.OperationType.Put => "Update",
                Microsoft.OpenApi.Models.OperationType.Patch => "Patch",
                Microsoft.OpenApi.Models.OperationType.Delete => "Delete",
                _ => "Custom"
            });

            // Add the parameters.
            foreach (var p in ao.Operation.Parameters)
            {
                if (_openApiArgs.IgnoreParametersThatStartWith.Length > 0 && _openApiArgs.IgnoreParametersThatStartWith.Contains(p.Name.FirstOrDefault()))
                    continue;

                var pname = StringConverter.ToPascalCase(GetDotNetName(p.Name))!;
                var pp = new YamlParameter(pname);
                pp.AddAttribute("type", GetDotNetType(pname, p.Schema), false, "string");
                pp.AddAttribute("isMandatory", p.Required);
                if (_outputText)
                    pp.AddAttribute("text", FormatText(p.Description));

                var comparer = _openApiArgs.JsonNameComparer;
                if (comparer.Compare(p.Name, StringConverter.ToCamelCase(pp.Name)) != 0)
                    pp.AddAttribute("argumentName", p.Name);

                operation.Parameters.Add(pp);
            }

            // Add the request (body).
            if (ao.Operation.RequestBody is not null)
            {
                if (ao.Operation.RequestBody.Reference is not null)
                    operation.AddAttribute("valueType", GetDotNetType(ao.Operation.RequestBody));
                else
                {
                    var content = ao.Operation.RequestBody.Content.FirstOrDefault();
                    if (content.Value is not null && content.Value.Schema is not null)
                        operation.AddAttribute("valueType", GetDotNetType(content.Value.Schema));
                }
            }

            // Add the response (body); being the first 2XX response, - if any.
            var response = ao.Operation.Responses.FirstOrDefault(x => x.Key.StartsWith('2'));
            if (response.Value is null)
                operation.AddAttribute("resultType", "void");
            else if (response.Value.Reference is not null)
                operation.AddAttribute("resultType", GetDotNetType(response.Value));
            else
            {
                var content = response.Value.Content.FirstOrDefault();
                if (content.Value is null || content.Value.Schema is null)
                    operation.AddAttribute("resultType", "void");
                else
                    operation.AddAttribute("resultType", GetDotNetType(string.Empty, content.Value.Schema));
            }

            // Add the web api attributes.
            operation.AddAttribute("webApiMethod", ao.Method switch
            {
                Microsoft.OpenApi.Models.OperationType.Get or Microsoft.OpenApi.Models.OperationType.Post or Microsoft.OpenApi.Models.OperationType.Put or Microsoft.OpenApi.Models.OperationType.Patch or Microsoft.OpenApi.Models.OperationType.Delete => null,
                _ => "Http" + ao.Method.ToString()
            });

            operation.AddAttribute("webApiRoute", "'" + ao.Path + "'");
            operation.AddAttribute("webApiStatus", response.Key is null ? "NoContent" : ((HttpStatusCode)int.Parse(response.Key)).ToString());

            // Finish it!
            if (_outputText)
                operation.AddAttribute("text", FormatText(ao.Operation.Summary));

            entity.Operations.Add(operation);
            _openApiArgs.OnYamlOperation?.Invoke(operation, ao.Operation);
        }

        /// <summary>
        /// Converts the schema to an entity.
        /// </summary>
        private YamlEntity CreateOrGetSchemaEntity(string name, OpenApiSchema schema, bool createWhereDifferences)
        {
            // Check if a faux entity is to be created as it should be ignored.
            var ename = GetDotNetName(name);
            if (_ignoreTypes.Contains(ename, StringComparer.OrdinalIgnoreCase))
                return new YamlEntity(ename);

            using var sw = new StringWriter();
            var oajw = new OpenApiJsonWriter(sw, new OpenApiJsonWriterSettings { Terse = true });
            schema.SerializeAsV3WithoutReference(oajw);
            var json = sw.ToString();

            // Create or get.
            YamlEntity? entity = null;

            if (createWhereDifferences)
            {
                int count = 0;
                while (true)
                {
                    var uename = count == 0 ? ename : $"{ename}{count}";
                    var eentity = _config.Entities!.FirstOrDefault(x => x.Name == GetDotNetName(uename));
                    if (eentity is null)
                    {
                        ename = uename;
                        break;
                    }

                    // Where same name and no differences in configuration then reuse.
                    if (eentity.Json == json)
                        return eentity;

                    count++;
                }
            }
            else
            {
                // Only create if not already exists.
                entity = _config.Entities!.FirstOrDefault(x => x.Name == ename);
                if (entity is not null)
                    return entity;
            }

            // Create the entity with its properties.
            entity = new YamlEntity(ename) { Json = json };
            if (_outputText)
                entity.AddAttribute("text", FormatText(schema.Description));

            _config.Entities!.Add(entity);

            string ApplyRefDataTypeNotation(string name, string type) => type == "string" && _refDataTypes.Contains(name, StringComparer.OrdinalIgnoreCase) ? "^" + name : type;

            foreach (var prop in schema.Properties)
            {
                var pname = StringConverter.ToPascalCase(GetDotNetName(prop.Key))!;
                var p = new YamlProperty(pname);
                p.AddAttribute("type", ApplyRefDataTypeNotation(pname, GetDotNetType(pname, prop.Value)), false, "string");

                var comparer = _openApiArgs.JsonNameComparer;
                if (comparer.Compare(prop.Key, StringConverter.ToCamelCase(p.Name)) != 0)
                    p.AddAttribute("jsonName", prop.Key);

                if (_outputText)
                    p.AddAttribute("text", FormatText(prop.Value.Description));

                _openApiArgs.OnYamlProperty?.Invoke(p, prop.Key, prop.Value);
                entity.Properties.Add(p);
            }

            // Remove any reference data text properties as not required.
            var indexes = new List<int>();
            foreach (var p in entity.Properties.Where(x => x.Attributes.Contains("type") && x.Attributes["type"]!.ToString()!.StartsWith('^')))
            {
                var index = entity.Properties.FindIndex(x => x.Name!.StartsWith(p.Name + "Text", StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                    indexes.Add(index);
            }

            foreach (var index in indexes.OrderBy(x => x).Reverse())
            {
                entity.Properties.RemoveAt(index);
            }

            // Done! Finalize.
            _openApiArgs.OnYamlEntity?.Invoke(entity, name, schema);
            return entity;
        }

        /// <summary>
        /// Gets a .NET friendly name.
        /// </summary>
        private static string GetDotNetName(string name)
        {
            name.ThrowIfNullOrEmpty(nameof(name));
            var sb = new StringBuilder();
            name.Split(_separators, StringSplitOptions.RemoveEmptyEntries).ForEach(part => sb.Append(StringConverter.ToPascalCase(part)));
            return sb.ToString();
        }

        /// <summary>
        /// Format the text for YAML.
        /// </summary>
        private static string? FormatText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            text = string.Join(" ", text.TrimEnd().Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (!text.EndsWith('.'))
                text = text + ".";

            if (text.Any(c => _yamlEscapeChars.Contains(c)))
                return "'+" + text.Replace("'", "''") + "'";
            else
                return "+" + text;
        }

        private string GetDotNetType(IOpenApiReferenceable reference)
        {
            var schema = _document.Components.Schemas[reference.Reference.Id];
            return GetDotNetType(reference.Reference.Id, schema);
        }

        /// <summary>
        /// Gets the equivalent .NET type.
        /// </summary>
        private string GetDotNetType(string pname, OpenApiSchema schema)
        {
            if (schema.Type == "string")
            {
                if (schema.Format == "date-time")
                    return "DateTime" + (schema.Nullable ? "?" : string.Empty);
                else if (schema.Format == "date")
                    return "DateTime" + (schema.Nullable ? "?" : string.Empty);
                else if (schema.Format == "time")
                    return "TimeSpan" + (schema.Nullable ? "?" : string.Empty);
                else
                {
                    if (schema.Reference is not null && schema.Reference.Id is not null)
                    {
                        var yenum = new YamlEnum(GetDotNetName(schema.Reference.Id));
                        foreach (var v in (schema.Enum ?? []).OfType<OpenApiString>())
                        {
                            yenum.Values.Add(v.Value);
                        }

                        _openApiArgs.OnYamlEnum?.Invoke(yenum, schema.Reference.Id, schema);
                        _config.Enums.Add(yenum);
                        return (_openApiArgs.EnumAsReferenceData ? "^" : "") + yenum.Name;
                    }
                    else
                        return "string";
                }
            }
            else if (schema.Type == "integer")
            {
                if (schema.Format == "int32")
                    return "int" + (schema.Nullable ? "?" : string.Empty);
                else if (schema.Format == "int64")
                    return "long" + (schema.Nullable ? "?" : string.Empty);
                else
                    return "int" + (schema.Nullable ? "?" : string.Empty);
            }
            else if (schema.Type == "number")
            {
                if (schema.Format == "float")
                    return "float" + (schema.Nullable ? "?" : string.Empty);
                else if (schema.Format == "double")
                    return "double" + (schema.Nullable ? "?" : string.Empty);
                else
                    return "decimal" + (schema.Nullable ? "?" : string.Empty);
            }
            else if (schema.Type == "boolean")
            {
                return "bool" + (schema.Nullable ? "?" : string.Empty);
            }
            else if (schema.Type == "array")
            {
                if (schema.Items.Type == "object")
                {
                    var aname = GetDotNetName(schema.Items.Reference.Id);
                    var entity = CreateOrGetSchemaEntity(aname, schema.Items, false);
                    entity!.AddAttribute("collection", true);
                    return entity.Name + "Collection";
                }
                else
                    return "List<" + GetDotNetType(pname, schema.Items) + ">";
            }
            else
            {
                if (schema.Reference is not null && schema.Reference.Type == ReferenceType.Schema)
                {
                    if (_document.Components.Schemas.TryGetValue(schema.Reference.Id, out var refSchema))
                        return CreateOrGetSchemaEntity(schema.Reference.Id, refSchema, false)!.Name!;

                    return GetDotNetName(schema.Reference.Id);
                }

                if (schema.Properties.Count == 0)
                    return "object";

                // Embedded object definition; therefore create a new _unique_ entity.
                var entity = CreateOrGetSchemaEntity(pname, schema, true);
                return entity.Name!;
            }
        }

        /// <summary>
        /// Represents the <see cref="OpenApiOperation"/> with the path and method.
        /// </summary>
        private class ApiOperation(string path, Microsoft.OpenApi.Models.OperationType method, OpenApiOperation operation)
        {
            public string Path { get; } = path;

            public Microsoft.OpenApi.Models.OperationType Method { get; } = method;

            public OpenApiOperation Operation { get; } = operation;
        }
    }
}