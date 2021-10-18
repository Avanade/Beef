// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.Collections.Generic;
using System.Reflection;

namespace OnRamp
{
    /// <summary>
    /// Provides the <see cref="CodeGenerator"/> arguments.
    /// </summary>
    public class CodeGeneratorArgs : CodeGeneratorArgsBase
    {
        /// <summary>
        /// Creates a new <see cref="CodeGeneratorArgs"/> using the <typeparamref name="T"/> to infer the <see cref="Type.Assembly"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to automatically infer the <see cref="Type.Assembly"/> to <see cref="ICodeGeneratorArgs.AddAssembly(Assembly[])">add</see>.</typeparam>
        /// <param name="scriptFileName">The script file name.</param>
        /// <param name="configFileName">The configuration file name.</param>
        /// <returns>A new <see cref="CodeGeneratorArgs"/>.</returns>
        public static CodeGeneratorArgs Create<T>(string? scriptFileName = null, string? configFileName = null) => new CodeGeneratorArgs(scriptFileName, configFileName).AddAssembly(typeof(T).Assembly);

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        /// <param name="scriptFileName">The script file name.</param>
        /// <param name="configFileName">The configuration file name.</param>
        public CodeGeneratorArgs(string? scriptFileName = null, string? configFileName = null)
        {
            ScriptFileName = scriptFileName;
            ConfigFileName = configFileName;
        }

        /// <summary>
        /// Adds (inserts) one or more <paramref name="assemblies"/> to <see cref="CodeGeneratorArgsBase.Assemblies"/> (before any existing values).
        /// </summary>
        /// <param name="assemblies">The assemblies to add.</param>
        /// <remarks>The order in which they are specified is the order in which they will be probed for embedded resources.</remarks>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public CodeGeneratorArgs AddAssembly(params Assembly[] assemblies)
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
        public CodeGeneratorArgs AddParameter(string key, string? value)
        {
            ((ICodeGeneratorArgs)this).AddParameter(key, value);
            return this;
        }

        /// <summary>
        /// Adds (merges) the <paramref name="parameters"/> to the <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The current <see cref="CodeGeneratorArgs"/> instance to support fluent-style method-chaining.</returns>
        public CodeGeneratorArgs AddParameters(IDictionary<string, string?> parameters)
        {
            ((ICodeGeneratorArgs)this).AddParameters(parameters);
            return this;
        }

        /// <summary>
        /// Clone the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        /// <returns>A new <see cref="CodeGeneratorArgs"/> instance.</returns>
        public override CodeGeneratorArgsBase Clone()
        {
            var args = new CodeGeneratorArgs();
            args.CopyFrom(this);
            return args;
        }
    }
}