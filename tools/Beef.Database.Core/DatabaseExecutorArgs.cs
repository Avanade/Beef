// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using OnRamp;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beef.Database.Core
{
    /// <summary>
    /// Represents the <see cref="DatabaseExecutorArgs"/>.
    /// </summary>
    public class DatabaseExecutorArgs : DatabaseConsoleArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutorArgs"/> class.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        public DatabaseExecutorArgs(DatabaseExecutorCommand command, string connectionString, params Assembly[] assemblies) : base()
        {
            Command = command;
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            AddAssembly(assemblies);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutorArgs"/> class from a <see cref="DatabaseConsoleArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseConsoleArgs"/>.</param>
        public DatabaseExecutorArgs(DatabaseConsoleArgs args) : base() => CopyFrom(args);

        /// <summary>
        /// Adds one or more <paramref name="schemas"/> to the <see cref="DatabaseConsoleArgs.SchemaOrder"/>.
        /// </summary>
        /// <param name="schemas">The schemas to add.</param>
        /// <returns>The current <see cref="DatabaseConsoleArgs"/> instance to support fluent-style method-chaining.</returns>
        public new DatabaseExecutorArgs AddSchemaOrder(params string[] schemas)
        {
            base.AddSchemaOrder(schemas);
            return this;
        }

        /// <summary>
        /// Adds (inserts) one or more <paramref name="assemblies"/> to <see cref="CodeGeneratorArgsBase.Assemblies"/> (before any existing values).
        /// </summary>
        /// <param name="assemblies">The assemblies to add.</param>
        /// <remarks>The order in which they are specified is the order in which they will be probed for embedded resources.</remarks>
        /// <returns>The current <see cref="DatabaseExecutorArgs"/> instance to support fluent-style method-chaining.</returns>
        public new DatabaseExecutorArgs AddAssembly(params Assembly[] assemblies)
        {
            base.AddAssembly(assemblies);
            return this;
        }

        /// <summary>
        /// Adds (merges) the parameter to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>The current <see cref="DatabaseExecutorArgs"/> instance to support fluent-style method-chaining.</returns>
        public new DatabaseExecutorArgs AddParameter(string key, string? value)
        {
            base.AddParameter(key, value);
            return this;
        }

        /// <summary>
        /// Adds (merges) the <paramref name="parameters"/> to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The current <see cref="DatabaseExecutorArgs"/> instance to support fluent-style method-chaining.</returns>
        public new DatabaseExecutorArgs AddParameters(IDictionary<string, object?> parameters)
        {
            base.AddParameters(parameters);
            return this;
        }
    }
}