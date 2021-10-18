// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
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
        public DatabaseExecutorCommand SupportedCommands { get; set; } = DatabaseExecutorCommand.All;

        /// <summary>
        /// Indicates whether to use the standard <i>Beef</i> <b>dbo</b> schema objects (defaults to <c>true</c>).
        /// </summary>
        public bool UseBeefDbo { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase.Parameters"/> value with a key of <see cref="CodeGenConsole.CompanyParamName"/>.
        /// </summary>
        public string Company => GetParameter(CodeGenConsole.CompanyParamName, true)!;

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase.Parameters"/> value with a key of <see cref="CodeGenConsole.AppNameParamName"/>.
        /// </summary>
        public string AppName => GetParameter(CodeGenConsole.AppNameParamName, true)!;

        /// <summary>
        /// Gets the list of <see cref="DatabaseExecutorCommand.ScriptNew"/> arguments.
        /// </summary>
        public List<string> ScriptNewArguments { get; } = new List<string>();

        /// <summary>
        /// Adds one or more <paramref name="arguments"/> to <see cref="ScriptNewArguments"/>.
        /// </summary>
        /// <param name="arguments">The arguments to add.</param>
        /// <returns>The current <see cref="DatabaseConsoleArgs"/> instance to support fluent-style method-chaining.</returns>
        public DatabaseConsoleArgs AddScriptNewArguments(params string[] arguments)
        {
            if (arguments != null)
                ScriptNewArguments.AddRange(arguments);

            return this;
        }

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
                SchemaOrder.AddRange(schemas);

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
        public DatabaseConsoleArgs AddParameters(IDictionary<string, string?> parameters)
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
            ScriptNewArguments.Clear();
            ScriptNewArguments.AddRange(args.ScriptNewArguments);
            SchemaOrder.Clear();
            SchemaOrder.AddRange(args.SchemaOrder);
        }

        /// <summary>
        /// Clone the <see cref="DatabaseConsoleArgs"/>.
        /// </summary>
        /// <returns>A new <see cref="DatabaseConsoleArgs"/> instance.</returns>
        public override CodeGeneratorArgsBase Clone()
        {
            var args = new DatabaseConsoleArgs();
            args.CopyFrom(this);
            return args;
        }
    }
}