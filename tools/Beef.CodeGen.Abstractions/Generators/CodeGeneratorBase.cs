// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Scripts;
using System;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Provides the base code-generation capabilities leveraging <b>Handlebars</b>.
    /// </summary>
    public abstract class CodeGeneratorBase
    {
        /// <summary>
        /// Check whether the nullable <see cref="bool"/> is <c>true</c>.
        /// </summary>
        protected static bool IsTrue(bool? value) => value.HasValue && value.Value;

        /// <summary>
        /// Check whether the nullable <see cref="bool"/> is <c>null</c> or <c>false</c>.
        /// </summary>
        protected static bool IsFalse(bool? value) => !value.HasValue || !value.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGeneratorBase"/> class.
        /// </summary>
        internal CodeGeneratorBase() { }

        /// <summary>
        /// Gets the root <see cref="Type"/>.
        /// </summary>
        internal abstract Type RootType { get; }

        /// <summary>
        /// Performs the code generation.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScript"/>.</param>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <param name="artefactGenerated">The <see cref="Action{T}"/> to invoked per artefact generated to be output.</param>
        public abstract void Generate(CodeGenScript script, ConfigBase config, Action<CodeGenOutputArgs> artefactGenerated);
    }
}