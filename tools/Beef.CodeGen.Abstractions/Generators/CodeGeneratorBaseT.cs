// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Provides the base code-generation capabilities leveraging <see cref="Handlebars"/> for where the root and code-gen <see cref="ConfigBase"/> are the same.
    /// </summary>
    /// <typeparam name="TRootConfig">The root <see cref="ConfigBase"/> <see cref="Type"/>.</typeparam>
    public abstract class CodeGeneratorBase<TRootConfig> : CodeGeneratorBase<TRootConfig, TRootConfig> where TRootConfig : ConfigBase, IRootConfig
    {
        /// <summary>
        /// The selection is the <typeparamref name="TRootConfig"/> itself.
        /// </summary>
        /// <param name="config">The root configuration.</param>
        /// <returns>The root configuration.</returns>
        protected override IEnumerable<TRootConfig> SelectGenConfig(TRootConfig config) => new TRootConfig[] { config };
    }
}