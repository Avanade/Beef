// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Scripts;
using OnRamp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beef.CodeGen
{
    /// <summary>
    /// Code-generation orchestrator.
    /// </summary>
    /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
    /// <param name="scripts">The <see cref="CodeGenScript"/>.</param>
    internal class CodeGenerator(ICodeGeneratorArgs args, CodeGenScript scripts) : OnRamp.CodeGenerator(args, scripts)
    {
        private const string _entitiesName = "entities";

        /// <inheritdoc/>
        /// <remarks>Will search for secondary (related) configuration files and will dynamically include (merge) the configured entities into the primary configuration.</remarks>
        protected override void OnConfigurationLoad(CodeGenScript script, string fileName, JsonNode json)
        {
            if (fileName == StreamFileName)
                return;

            var fi = new FileInfo(fileName);
            if (!fi.Exists)
                return;

            var secondaryFiles = new List<FileInfo>();
            JsonArray? rootEntities = null;
            var root = Path.GetFileNameWithoutExtension(fi.Name);
            foreach (var secondaryFile in fi.Directory!.EnumerateFiles($"*.{root}.*", SearchOption.AllDirectories))
            {
                if (!Path.GetFileNameWithoutExtension(secondaryFile.FullName).EndsWith(root, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    using var configReader = secondaryFile.OpenText();

                    var secondaryJson = StreamLocator.GetContentType(secondaryFile.FullName) switch
                    {
                        StreamContentType.Yaml => configReader.YamlToJsonNode(),
                        StreamContentType.Json => configReader.JsonToJsonNode(),
                        _ => null
                    };

                    if (secondaryJson is null)
                        continue;

                    var entities = GetSecondaryEntitiesArray(secondaryJson.Root) ?? throw new CodeGenException($"The secondary root node must be an object, with a single '{_entitiesName}' property that is an array. [{secondaryFile.FullName}]");
                    if (entities is null || entities.Count == 0)
                        continue;

                    rootEntities ??= GetPrimaryEntitiesArray(json);
                    if (rootEntities is null)
                        return;

                    foreach (var ji in entities)
                    {
                        rootEntities.Add(ji!.DeepClone());
                    }

                    secondaryFiles.Add(secondaryFile);
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException($"{ex.Message} [{secondaryFile.FullName}]"); }
            }

            for (int i =  0; i < secondaryFiles.Count; i++) 
            {
                if (i == 0)
                    CodeGenArgs.Logger?.LogInformation("{Content}", "Merged:");

                CodeGenArgs.Logger?.LogInformation("{Content}", $"  {secondaryFiles[i].FullName}");
            }

        }

        /// <summary>
        /// Get the entities array for the secondary configuration.
        /// </summary>
        private static JsonArray? GetSecondaryEntitiesArray(JsonNode root)
        {
            if (root.GetValueKind() != JsonValueKind.Object)
                return null;

            var jo = root.AsObject();
            if (jo.Count == 1 && jo.TryGetPropertyValue(_entitiesName, out var je))
                return je!.GetValueKind() != JsonValueKind.Array ? null : je.AsArray();
            else
                return null;
        }

        /// <summary>
        /// Gets or creates the entities array for the primary configuration.
        /// </summary>
        private static JsonArray? GetPrimaryEntitiesArray(JsonNode root)
        {
            if (root.GetValueKind() != JsonValueKind.Object)
                return null;

            var jo = root.AsObject();
            if (jo.TryGetPropertyValue(_entitiesName, out var je))
                return je!.GetValueKind() != JsonValueKind.Array ? null : je.AsArray();

            je = new JsonArray();
            jo.Add(_entitiesName, je);
            return (JsonArray)je;
        }

        /// <inheritdoc/>
        protected override void OnAfterScript(CodeGenScriptItem script, CodeGenStatistics statistics)
        {
            if (statistics.RootConfig is CodeGenConfig cgc)
            {
                if (script.Template!.StartsWith("EntityWebApiController_cs"))
                {
                    var endpoints = new EndPointStatistics();
                    endpoints.AddEntityEndPoints(cgc);
                    endpoints.WriteInline(cgc.CodeGenArgs!.Logger!);

                }
                else if (script.Template!.StartsWith("ReferenceDataWebApiController_cs"))
                {
                    var endpoints = new EndPointStatistics();
                    endpoints.AddRefDataEndPoints(cgc);
                    endpoints.WriteInline(cgc.CodeGenArgs!.Logger!);
                }
            }

            base.OnAfterScript(script, statistics);
        }
    }
}