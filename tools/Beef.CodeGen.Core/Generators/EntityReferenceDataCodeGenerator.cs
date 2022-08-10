// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>Root</b> code generator for <see cref="CodeGenConfig"/>.
    /// </summary>
    public class EntityReferenceDataCodeGenerator : CodeGeneratorBase<CodeGenConfig> 
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
            => ((config ?? throw new System.ArgumentNullException(nameof(config))).EntityScope == config!.Root!.RuntimeEntityScope) || (config!.EntityScope == "Autonomous" && config!.RuntimeEntityScope == "Business") ? new CodeGenConfig[] { config } : System.Array.Empty<CodeGenConfig>();
    }
}