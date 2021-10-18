// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Beef.CodeGen
{
    /// <summary>
    /// Defines the <see cref="CodeGenerator"/> arguments.
    /// </summary>
    public interface ICodeGeneratorArgs
    {
        /// <summary>
        /// Gets or sets the <b>Script</b> file name to load the content from the <c>Scripts</c> folder within the file system (primary) or <see cref="Assemblies"/> (secondary, recursive until found).
        /// </summary>
        public string? ScriptFileName { get; }

        /// <summary>
        /// Gets or sets the <b>Configuration</b> file name.
        /// </summary>
        public string? ConfigFileName { get; }

        /// <summary>
        /// Gets or sets the output <see cref="DirectoryInfo"/> where the generated artefacts are to be written.
        /// </summary>
        public DirectoryInfo? OutputDirectory { get; }

        /// <summary>
        /// Gets or sets the assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to explicitly specify).
        /// </summary>
        List<Assembly> Assemblies { get; } 

        /// <summary>
        /// Dictionary of <see cref="IRootConfig.RuntimeParameters"/> name/value pairs.
        /// </summary>
        Dictionary<string, string?> Parameters { get; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/> to optionally log the underlying code-generation.
        /// </summary>
        public ILogger? Logger { get; }

        /// <summary>
        /// Indicates whether the <see cref="CodeGenerator.Generate(string)"/> is expecting to generate <i>no</i> changes; e.g. within in a build pipeline.
        /// </summary>
        /// <remarks>Where changes are found then </remarks>
        bool ExpectNoChanges { get; }

        /// <summary>
        /// Indicates whether the code-generation is a simulation; i.e. does not update the artefacts.
        /// </summary>
        bool IsSimulation { get; }

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        string? ConnectionString { get; }

        /// <summary>
        /// Gets or sets the environment variable name to get the connection string.
        /// </summary>
        string? ConnectionStringEnvironmentVariableName { get; }

        /// <summary>
        /// Updates the <see cref="ConnectionString"/> based on following order of precedence: <paramref name="overrideConnectionString"/>, from the <see cref="ConnectionStringEnvironmentVariableName"/>, then existing <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="overrideConnectionString">The connection string override.</param>
        void UpdateConnectionString(string? overrideConnectionString = null);

        /// <summary>
        /// Adds (inserts) one or more <paramref name="assemblies"/> to <see cref="CodeGeneratorArgsBase.Assemblies"/> (before any existing values).
        /// </summary>
        /// <param name="assemblies">The assemblies to add.</param>
        /// <remarks>The order in which they are specified is the order in which they will be probed for embedded resources.</remarks>
        void AddAssembly(params Assembly[] assemblies) => Assemblies.InsertRange(0, assemblies);

        /// <summary>
        /// Adds (merges) the parameter to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        void AddParameter(string key, string? value)
        {
            if (!Parameters.TryAdd(key, value))
                Parameters[key] = value;
        }

        /// <summary>
        /// Adds (merges) the <paramref name="parameters"/> to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        void AddParameters(IDictionary<string, string?> parameters)
        {
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    AddParameter(p.Key, p.Value);
                }
            }
        }

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgsBase"/> to copy from.</param>
        void CopyFrom(ICodeGeneratorArgs args);

        /// <summary>
        /// Clone the <see cref="CodeGeneratorArgsBase"/>.
        /// </summary>
        /// <returns>A new <see cref="CodeGeneratorArgsBase"/> instance.</returns>
        ICodeGeneratorArgs Clone();
    }
}