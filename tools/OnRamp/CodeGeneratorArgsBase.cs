// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OnRamp
{
    /// <summary>
    /// Represents the base arguments for a <see cref="CodeGenerator"/>.
    /// </summary>
    /// <remarks>Note that the <see cref="ConnectionString"/> treatment is managed by <see cref="OverrideConnectionString(string?)"/>.</remarks>
    public abstract class CodeGeneratorArgsBase : ICodeGeneratorArgs
    {
        /// <inheritdoc/>
        public string? ScriptFileName { get; set; }

        /// <inheritdoc/>
        public string? ConfigFileName { get; set; }

        /// <inheritdoc/>
        public DirectoryInfo? OutputDirectory { get; set; }

        /// <inheritdoc/>
        public List<Assembly> Assemblies { get; } = new List<Assembly>();

        /// <inheritdoc/>
        public Dictionary<string, string?> Parameters { get; } = new Dictionary<string, string?>();

        /// <inheritdoc/>
        public ILogger? Logger { get; set; }

        /// <inheritdoc/>
        public bool ExpectNoChanges { get; set; }

        /// <inheritdoc/>
        public bool IsSimulation { get; set; }

        /// <inheritdoc/>
        public string? ConnectionString { get; set; }

        /// <inheritdoc/>
        public string? ConnectionStringEnvironmentVariableName { get; set; }

        /// <inheritdoc/>
        public Func<CodeGeneratorArgsBase, string?>? CreateConnectionStringEnvironmentVariableName { get; set; }

        /// <inheritdoc/>
        public void OverrideConnectionString(string? overrideConnectionString = null)
        {
            if (HasOverriddenConnectionString)
                return;

            if (!string.IsNullOrEmpty(overrideConnectionString))
            {
                ConnectionString = overrideConnectionString;
                HasOverriddenConnectionString = true;
                return;
            }

            string? cs = null;
            if (!string.IsNullOrEmpty(ConnectionStringEnvironmentVariableName))
            {
                cs = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariableName);
                if (!string.IsNullOrEmpty(cs))
                {
                    ConnectionString = cs;
                    HasOverriddenConnectionString = true;
                    return;
                }
            }

            if (CreateConnectionStringEnvironmentVariableName != null)
            {
                var evn = CreateConnectionStringEnvironmentVariableName(this);
                if (!string.IsNullOrEmpty(evn))
                {
                    cs = Environment.GetEnvironmentVariable(evn);
                    if (!string.IsNullOrEmpty(cs))
                    {
                        ConnectionString = cs;
                        HasOverriddenConnectionString = true;
                        return;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool HasOverriddenConnectionString { get; private set; }

        /// <inheritdoc/>
        void ICodeGeneratorArgs.CopyFrom(ICodeGeneratorArgs args) => CopyFrom((CodeGeneratorArgsBase)args);

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgsBase"/> to copy from.</param>
        public void CopyFrom(CodeGeneratorArgsBase args)
        {
            HasOverriddenConnectionString = args.HasOverriddenConnectionString;
            ScriptFileName = (args ?? throw new ArgumentNullException(nameof(args))).ScriptFileName;
            ConfigFileName = args.ConfigFileName;
            OutputDirectory = args.OutputDirectory == null ? null : new DirectoryInfo(args.OutputDirectory.FullName);
            Logger = args.Logger;
            ExpectNoChanges = args.ExpectNoChanges;
            IsSimulation = args.IsSimulation;
            ConnectionString = args.ConnectionString;
            ConnectionStringEnvironmentVariableName = args.ConnectionStringEnvironmentVariableName;
            CreateConnectionStringEnvironmentVariableName = args.CreateConnectionStringEnvironmentVariableName;

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
        public string? GetParameter(string key, bool throwWhereNotFound = false)
        {
            if (Parameters.TryGetValue(key, out var value))
                return value;

            return !throwWhereNotFound ? null : throw new CodeGenException($"Parameter '{key}' does not exist.");
        }
    }
}