// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using OnRamp.Generators;
using System;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>Root</b> code generator for <see cref="CodeGenConfig"/>.
    /// </summary>
    public class DatabaseEfModelBuilderCodeGenerator : CodeGeneratorBase<CodeGenConfig> 
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
        {
            return Check.NotNull(config, nameof(config)).EFModels.Count > 0 ? base.SelectGenConfig(config) : Array.Empty<CodeGenConfig>();
        }
    }
}