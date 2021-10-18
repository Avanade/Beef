// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System.Collections.Generic;

namespace OnRamp.Config
{
    /// <summary>
    /// Enables the additional root configuration capabilities.
    /// </summary>
    public interface IRootConfig
    {
        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        CodeGeneratorArgsBase? CodeGenArgs { get; }

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        Dictionary<string, string?> RuntimeParameters { get; }

        /// <summary>
        /// Sets the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        /// <param name="codeGenArgs">The <see cref="CodeGeneratorArgsBase"/>.</param>
        void SetCodeGenArgs(CodeGeneratorArgsBase codeGenArgs);

        /// <summary>
        /// Merges (adds or updates) <paramref name="parameters"/> into the <see cref="RuntimeParameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters to merge.</param>
        void MergeRuntimeParameters(IDictionary<string, string?>? parameters);

        /// <summary>
        /// Resets (clears) the <see cref="RuntimeParameters"/>.
        /// </summary>
        void ResetRuntimeParameters();
    }
}