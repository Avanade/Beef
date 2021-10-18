// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Beef.CodeGen
{
    /// <summary>
    /// Represents the base arguments for a <see cref="CodeGenerator"/>.
    /// </summary>
    public abstract class CodeGeneratorArgsBase : ICodeGeneratorArgs
    {
        /// <summary>
        /// Gets or sets the <b>Script</b> file name to load the content from the <c>Scripts</c> folder within the file system (primary) or <see cref="Assemblies"/> (secondary, recursive until found).
        /// </summary>
        public string? ScriptFileName { get; set; }

        /// <summary>
        /// Gets or sets the <b>Configuration</b> file name.
        /// </summary>
        public string? ConfigFileName { get; set; }

        /// <summary>
        /// Gets or sets the output <see cref="DirectoryInfo"/> where the generated artefacts are to be written.
        /// </summary>
        public DirectoryInfo? OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to explicitly specify).
        /// </summary>
        public List<Assembly> Assemblies { get; } = new List<Assembly>();

        /// <summary>
        /// Dictionary of <see cref="IRootConfig.RuntimeParameters"/> name/value pairs.
        /// </summary>
        public Dictionary<string, string?> Parameters { get; } = new Dictionary<string, string?>();

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/> to optionally log the underlying code-generation.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="CodeGenerator.Generate(string)"/> is expecting to generate <i>no</i> changes; e.g. within in a build pipeline.
        /// </summary>
        /// <remarks>Where changes are found then </remarks>
        public bool ExpectNoChanges { get; set; }

        /// <summary>
        /// Indicates whether the code-generation is a simulation; i.e. does not update the artefacts.
        /// </summary>
        public bool IsSimulation { get; set; }

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the environment variable name to get the connection string.
        /// </summary>
        public string? ConnectionStringEnvironmentVariableName { get; set; }

        /// <summary>
        /// Updates the <see cref="ConnectionString"/> based on following order of precedence: <paramref name="overrideConnectionString"/>, from the <see cref="ConnectionStringEnvironmentVariableName"/>, then existing <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="overrideConnectionString">The connection string override.</param>
        public void UpdateConnectionString(string? overrideConnectionString = null)
        {
            if (!string.IsNullOrEmpty(overrideConnectionString))
                ConnectionString = overrideConnectionString;
            else if (!string.IsNullOrEmpty(ConnectionStringEnvironmentVariableName))
            {
                var ev = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariableName);
                if (!string.IsNullOrEmpty(ev))
                    ConnectionString = ev;
            }
        }

        /// <inheritdoc/>
        void ICodeGeneratorArgs.CopyFrom(ICodeGeneratorArgs args) => CopyFrom((CodeGeneratorArgsBase)args);

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgsBase"/> to copy from.</param>
        public void CopyFrom(CodeGeneratorArgsBase args)
        {
            ScriptFileName = (args ?? throw new ArgumentNullException(nameof(args))).ScriptFileName;
            ConfigFileName = args.ConfigFileName;
            OutputDirectory = args.OutputDirectory == null ? null : new DirectoryInfo(args.OutputDirectory.FullName);
            Logger = args.Logger;
            ExpectNoChanges = args.ExpectNoChanges;
            IsSimulation = args.IsSimulation;
            ConnectionString = args.ConnectionString;
            ConnectionStringEnvironmentVariableName = args.ConnectionStringEnvironmentVariableName;

            Assemblies.Clear();
            Assemblies.AddRange(args.Assemblies);

            Parameters.Clear();
            if (args.Parameters != null)
            {
                foreach (var p in args.Parameters)
                {
                    Parameters.Add(p.Key, p.Value);
                }
            }
        }

        /// <inheritdoc/>
        ICodeGeneratorArgs ICodeGeneratorArgs.Clone() => Clone();

        /// <summary>
        /// Clone the <see cref="CodeGeneratorArgsBase"/>.
        /// </summary>
        /// <returns>A new <see cref="CodeGeneratorArgsBase"/> instance.</returns>
        public abstract CodeGeneratorArgsBase Clone();

        /// <summary>
        /// Gets the specified parameter from the <see cref="Parameters"/> collection.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="KeyNotFoundException"/> when the specified key is not found.</param>
        /// <returns>The parameter value where found; otherwise, <c>null</c>.</returns>
        public string? GetParameter(string key, bool throwWhereNotFound = false)
        {
            if (Parameters.TryGetValue(key, out var value))
                return value;

            return !throwWhereNotFound ? null : throw new KeyNotFoundException($"Parameter '{key}' does not exist.");
        }
    }
}