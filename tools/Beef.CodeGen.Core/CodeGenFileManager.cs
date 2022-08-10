// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Converters;
using Microsoft.Extensions.Logging;
using OnRamp;
using OnRamp.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// Provides code generation file management.
    /// </summary>
    public static class CodeGenFileManager
    {
        /// <summary>
        /// Gets the list of supported <see cref="CommandType.Entity"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> EntityFilenames { get; } = new List<string>(new string[] { "entity.beef.yaml", "entity.beef.yml", "entity.beef.json", "entity.beef.xml", "{{Company}}.{{AppName}}.xml" });

        /// <summary>
        /// Gets the list of supported <see cref="CommandType.RefData"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> RefDataFilenames { get; } = new List<string>(new string[] { "refdata.beef.yaml", "refdata.beef.yml", "refdata.beef.json", "refdata.beef.xml", "{{Company}}.RefData.xml" });

        /// <summary>
        /// Gets the list of supported <see cref="CommandType.RefData"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> DataModelFilenames { get; } = new List<string>(new string[] { "datamodel.beef.yaml", "datamodel.beef.yml", "datamodel.beef.json", "datamodel.beef.xml", "{{Company}}.{{AppName}}.DataModel.xml" });

        /// <summary>
        /// Gets the list of supported <see cref="CommandType.Database"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> DatabaseFilenames { get; } = new List<string>(new string[] { "database.beef.yaml", "database.beef.yml", "database.beef.json", "database.beef.xml", "{{Company}}.{{AppName}}.Database.xml" });

        /// <summary>
        /// Get the configuration filename.
        /// </summary>
        /// <param name="directory">The directory/path.</param>
        /// <param name="type">The <see cref="CommandType"/>.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application name.</param>
        /// <returns>The filename</returns>
        public static string GetConfigFilename(string directory, CommandType type, string company, string appName)
        {
            List<string> files = new List<string>();
            foreach (var n in GetConfigFilenames(type))
            {
                var fi = new FileInfo(Path.Combine(directory, n.Replace("{{Company}}", company, StringComparison.OrdinalIgnoreCase).Replace("{{AppName}}", appName, StringComparison.OrdinalIgnoreCase)));
                if (fi.Exists)
                    return fi.FullName;

                files.Add(fi.Name);
            }

            throw new CodeGenException($"Configuration file not found; looked for one of the following: {string.Join(", ", files)}.");
        }

        /// <summary>
        /// Gets the list of possible filenames for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="CommandType"/>.</param>
        /// <returns>The list of filenames.</returns>
        public static List<string> GetConfigFilenames(CommandType type) => type switch
            {
                CommandType.Entity => EntityFilenames,
                CommandType.RefData => RefDataFilenames,
                CommandType.DataModel => DataModelFilenames,
                CommandType.Database => DatabaseFilenames,
                _ => throw new InvalidOperationException("Command Type is not valid.")
            };

        /// <summary>
        /// Convert existing XML file to YAML.
        /// </summary>
        /// <param name="type">The <see cref="CommandType"/>.</param>
        /// <param name="filename">The XML filename.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <returns><c>true</c> indicates success; otherwise, <c>false</c>.</returns>
        public static async Task<bool> ConvertXmlToYamlAsync(CommandType type, string filename, ILogger? logger)
        {
            logger?.LogInformation("{Content}", $"Convert XML to YAML file configuration: {filename}");
            logger?.LogInformation("{Content}", string.Empty);

            var xfi = new FileInfo(filename);
            if (!xfi.Exists)
            {
                logger?.LogError("{Content}", "File does not exist.");
                logger?.LogInformation("{Content}", string.Empty);
                return false;
            }

            if (string.Compare(xfi.Extension, ".XML", StringComparison.OrdinalIgnoreCase) != 0)
            {
                logger?.LogError("{Content}", "File extension must be XML.");
                logger?.LogInformation("{Content}", string.Empty);
                return false;
            }

            var yfi = new FileInfo(Path.Combine(xfi.DirectoryName!, GetConfigFilenames(type).First()));
            if (yfi.Exists)
            {
                logger?.LogError("{Content}", $"YAML file already exists: {yfi.Name}");
                logger?.LogInformation("{Content}", string.Empty);
                return false;
            }

            try
            {
                using var xfs = xfi.OpenRead();
                var xml = await XDocument.LoadAsync(xfs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
                var result = type == CommandType.Database ? new DatabaseXmlToYamlConverter().ConvertXmlToYaml(xml) : new EntityXmlToYamlConverter().ConvertXmlToYaml(xml);
                using var ysw = yfi.CreateText();
                await ysw.WriteAsync(result.Yaml).ConfigureAwait(false);

                logger?.LogWarning("{Content}", $"YAML file created: {yfi.Name}");
                logger?.LogInformation("{Content}", string.Empty);
                logger?.LogInformation("{Content}", "Please check the contents of the YAML conversion and when ready to proceed please delete the existing XML file.");
                logger?.LogInformation("{Content}", string.Empty);
                logger?.LogInformation("{Content}", "Note: the existing XML formatting and comments may not have been converted correctly; these will need to be refactored manually.");
                logger?.LogInformation("{Content}", "Note: the YAML file will now be used as the configuration source even where the existing XML file exists; if this is not the desired state then the YAML file should be deleted.");

                if (result.UnknownAttributes.Count > 0)
                {
                    logger?.LogInformation("{Content}", string.Empty);
                    logger?.LogWarning("{Content}", "The following element.attributes combinations are not considered core Beef; please delete if no longer required:");
                    foreach (var ua in result.UnknownAttributes)
                    {
                        logger?.LogInformation("{Content}", $" > {ua}");
                    }
                }

                logger?.LogInformation("{Content}", string.Empty);
            }
            catch (Exception ex)
            {
                logger?.LogError("{Content}", ex.Message);
                logger?.LogInformation("{Content}", string.Empty);
                return false;
            }

            return true;
        }
    }
}