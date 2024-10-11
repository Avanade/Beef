// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
using OnRamp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.Database
{
    /// <summary>
    /// Provides the extended <i>Beef</i> migration arguments.
    /// </summary>
    public class MigrationArgs : DbEx.Migration.MigrationArgsBase<MigrationArgs>, ICodeGeneratorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationArgs"/> class.
        /// </summary>
        public MigrationArgs() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationArgs"/> class.
        /// </summary>
        /// <param name="migrationCommand">The <see cref="MigrationCommand"/>.</param>
        /// <param name="connectionString">The optional connection string.</param>
        /// <param name="assemblies">The options assemblies for resource probing.</param>
        public MigrationArgs(MigrationCommand migrationCommand, string? connectionString = null, params Assembly[] assemblies)
        {
            MigrationCommand = migrationCommand;
            ConnectionString = connectionString;
            AddAssembly(assemblies);
        }

        /// <inheritdoc/>
        public string? ScriptFileName { get; set; }

        /// <inheritdoc/>
        public string? ConfigFileName { get; set; }

        /// <inheritdoc/>
        public bool ExpectNoChanges { get; set; }

        /// <inheritdoc/>
        public bool IsSimulation { get; set; }

        /// <inheritdoc/>
        List<Assembly> ICodeGeneratorArgs.Assemblies => Assemblies.Select(x => x.Assembly).ToList();

        /// <summary>
        /// Indicates whether to use the standard <i>Beef</i> schema objects.
        /// </summary>
        /// <param name="useBeefSchema">The option to use the standard <i>Beef</i> schema objects. Defaults to <c>true</c> where not specified.</param>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        [Obsolete("This method is no longer supported; please use 'IncludeExtendedSchemaScripts' which will inlcude/add the resources from DbEx.", true)]
        public MigrationArgs UseBeefSchema(bool useBeefSchema = true) => throw new NotSupportedException("This method is no longer supported; please use 'IncludeExtendedSchemaScripts' which will inlcude/add the resources from DbEx.");

        /// <inheritdoc/>
        public T GetParameter<T>(string key, bool throwWhereNotFound = false)
        {
            if (Parameters.TryGetValue(key, out var value))
            {
                if (value is IConvertible c)
                    return (T)Convert.ChangeType(c, typeof(T))!;
                else
                    return (T)value!;
            }

            if (throwWhereNotFound)
                throw new CodeGenException($"Parameter '{key}' does not exist.");

            return default!;
        }

        /// <summary>
        /// Adds (merges) the parameter to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public MigrationArgs AddParameter(string key, string? value)
        {
            ((ICodeGeneratorArgs)this).AddParameter(key, value);
            return this;
        }

        /// <summary>
        /// Adds (merges) the <paramref name="parameters"/> to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public MigrationArgs AddParameters(IDictionary<string, object?> parameters)
        {
            ((ICodeGeneratorArgs)this).AddParameters(parameters);
            return this;
        }

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgsBase"/> to copy from.</param>
        public void CopyFrom(MigrationArgs args)
        {
            base.CopyFrom(args ?? throw new ArgumentNullException(nameof(args)));

            ScriptFileName = args.ScriptFileName;
            ConfigFileName = args.ConfigFileName;
            ExpectNoChanges = args.ExpectNoChanges;
            IsSimulation = args.IsSimulation;

            ClearAssemblies();
            AddAssembly(args.Assemblies.ToArray());

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
        void ICodeGeneratorArgs.CopyFrom(ICodeGeneratorArgs args) => CopyFrom((MigrationArgs)args);
    }
}