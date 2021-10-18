// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Provides the <b>root</b> base <see cref="ConfigBase.Prepare(object, object)"/> configuration capabilities.
    /// </summary>
    /// <typeparam name="TRoot">The root <see cref="Type"/>.</typeparam>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public abstract class ConfigRootBase<TRoot> : ConfigBase<TRoot, TRoot>, IRootConfig where TRoot : ConfigRootBase<TRoot>
    {
        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase"/>.
        /// </summary>
        public CodeGeneratorArgsBase? CodeGenArgs { get; private set; }

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        public Dictionary<string, string?> RuntimeParameters { get; } = new Dictionary<string, string?>();

        /// <summary>
        /// Sets the <see cref="CodeGeneratorArgsBase"/>.
        /// </summary>
        /// <param name="codeGenArgs">The <see cref="CodeGeneratorArgsBase"/>.</param>
        public void SetCodeGenArgs(CodeGeneratorArgsBase codeGenArgs) => CodeGenArgs = codeGenArgs ?? throw new ArgumentNullException(nameof(codeGenArgs));

        /// <summary>
        /// Merges (adds or updates) <paramref name="parameters"/> into the <see cref="RuntimeParameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters to merge.</param>
        public void MergeRuntimeParameters(IDictionary<string, string?>? parameters)
        {
            if (parameters == null)
                return;

            foreach (var p in parameters)
            {
                if (RuntimeParameters.ContainsKey(p.Key))
                    RuntimeParameters[p.Key] = p.Value;
                else
                    RuntimeParameters.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// Resets (clears) the <see cref="RuntimeParameters"/>.
        /// </summary>
        public void ResetRuntimeParameters() => RuntimeParameters.Clear();

        /// <summary>
        /// Gets the property value from <see cref="RuntimeParameters"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetRuntimeParameter<T>(string key, T defaultValue = default!)
        {
            if (RuntimeParameters != null && RuntimeParameters.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val?.ToString(), typeof(T))!;
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="RuntimeParameters"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> where the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetRuntimeParameter<T>(string key, out T value)
        {
            if (RuntimeParameters != null && RuntimeParameters.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val?.ToString(), typeof(T))!;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }
    }
}