// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using OnRamp;
using System;
using System.Collections.Generic;
using System.IO;

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
        public static List<string> EntityFilenames { get; } = new List<string>(new string[] { "entity.beef-5.yaml", "entity.beef-5.yml", "entity.beef-5.json" });

        /// <summary>
        /// Gets the list of supported <see cref="CommandType.RefData"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> RefDataFilenames { get; } = new List<string>(new string[] { "refdata.beef-5.yaml", "refdata.beef-5.yml", "refdata.beef-5.json" });

        /// <summary>
        /// Gets the list of supported <see cref="CommandType.RefData"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> DataModelFilenames { get; } = new List<string>(new string[] { "datamodel.beef-5.yaml", "datamodel.beef-5.yml", "datamodel.beef-5.json" });

        /// <summary>
        /// Gets the list of supported <see cref="CommandType.Database"/> filenames (will search in order specified).
        /// </summary>
        public static List<string> DatabaseFilenames { get; } = new List<string>(new string[] { "database.beef-5.yaml", "database.beef-5.yml", "database.beef-5.json" });

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
            List<string> files = new();
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
    }
}