// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using OnRamp;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beef.Database.Core
{
    /// <summary>
    /// Defines the <see cref="DatabaseConsole"/> arguments.
    /// </summary>
    public class DatabaseConsoleArgs : CodeGeneratorArgsBase
    {
        /// <summary>
        /// Creates a new <see cref="DatabaseConsoleArgs"/> using the <typeparamref name="T"/> to infer the <see cref="Type.Assembly"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to automatically infer the <see cref="Type.Assembly"/> to <see cref="AddAssembly(Assembly[])">add</see>.</typeparam>
        /// <returns>A new <see cref="DatabaseConsoleArgs"/>.</returns>
        public static DatabaseConsoleArgs Create<T>() => new DatabaseConsoleArgs().AddAssembly(typeof(T).Assembly);

        /// <summary>
        /// Gets or sets the <see cref="DatabaseExecutorCommand"/> to invoke.
        /// </summary>
        public DatabaseExecutorCommand Command { get; set; } = DatabaseExecutorCommand.None;

        /// <summary>
        /// Gets or sets the supported <see cref="DatabaseExecutorCommand"/>(s).
        /// </summary>
        /// <remarks>Defaults to everything: <see cref="DatabaseExecutorCommand.All"/>, <see cref="DatabaseExecutorCommand.Reset"/>, <see cref="DatabaseExecutorCommand.Drop"/>, <see cref="DatabaseExecutorCommand.Execute"/> and <see cref="DatabaseExecutorCommand.Script"/>.</remarks>
        public DatabaseExecutorCommand SupportedCommands { get; set; } = DatabaseExecutorCommand.All | DatabaseExecutorCommand.Reset | DatabaseExecutorCommand.Drop | DatabaseExecutorCommand.Execute | DatabaseExecutorCommand.Script;

        /// <summary>
        /// Indicates whether to use the standard <i>Beef</i> <b>dbo</b> schema objects (defaults to <c>true</c>).
        /// </summary>
        public bool UseBeefDbo { get; set; } = true;

        /// <summary>
        /// Gets the schema priority order list.
        /// </summary>
        public List<string> SchemaOrder { get; private set; } = new List<string>();

        /// <summary>
        /// Adds one or more <paramref name="schemas"/> to the <see cref="SchemaOrder"/>.
        /// </summary>
        /// <param name="schemas">The schemas to add.</param>
        /// <returns>The current <see cref="DatabaseConsoleArgs"/> instance to support fluent-style method-chaining.</returns>
        public DatabaseConsoleArgs AddSchemaOrder(params string[] schemas)
        {
            if (schemas != null)
            {
                foreach (var s in schemas)
                {
                    if (!SchemaOrder.Contains(s))
                        SchemaOrder.Add(s);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds (inserts) one or more <paramref name="assemblies"/> to <see cref="CodeGeneratorArgsBase.Assemblies"/> (before any existing values).
        /// </summary>
        /// <param name="assemblies">The assemblies to add.</param>
        /// <remarks>The order in which they are specified is the order in which they will be probed for embedded resources.</remarks>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public DatabaseConsoleArgs AddAssembly(params Assembly[] assemblies)
        {
            ((ICodeGeneratorArgs)this).AddAssembly(assemblies);
            return this;
        }

        /// <summary>
        /// Adds (merges) the parameter to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public DatabaseConsoleArgs AddParameter(string key, string? value)
        {
            ((ICodeGeneratorArgs)this).AddParameter(key, value);
            return this;
        }

        /// <summary>
        /// Adds (merges) the <paramref name="parameters"/> to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public DatabaseConsoleArgs AddParameters(IDictionary<string, object?> parameters)
        {
            ((ICodeGeneratorArgs)this).AddParameters(parameters);
            return this;
        }

        /// <summary>
        /// Copy and replace from <paramref name="args"/>.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseConsoleArgs"/> to copy from.</param>
        protected void CopyFrom(DatabaseConsoleArgs args)
        {
            base.CopyFrom(args);
            Command = args.Command;
            SupportedCommands = args.SupportedCommands;
            UseBeefDbo = args.UseBeefDbo;
            SchemaOrder.Clear();
            SchemaOrder.AddRange(args.SchemaOrder);
            ScriptName = args.ScriptName;
            if (args.ScriptArguments != null)
                ScriptArguments = new Dictionary<string, string?>(args.ScriptArguments);

            if (args.ExecuteStatements != null)
                ExecuteStatements = new List<string>(args.ExecuteStatements);
        }


        /// <summary>
        /// Gets or sets the <see cref="DatabaseExecutorCommand.Script"/> name.
        /// </summary>
        public string? ScriptName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseExecutorCommand.Script"/> arguments.
        /// </summary>
        public IDictionary<string, string?>? ScriptArguments { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseExecutorCommand.Execute"/> statements.
        /// </summary>
        public List<string>? ExecuteStatements { get; set; }
    }
}