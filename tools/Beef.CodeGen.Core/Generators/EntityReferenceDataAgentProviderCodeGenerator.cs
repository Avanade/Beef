// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>Root</b> code generator for <see cref="CodeGenConfig"/>.
    /// </summary>
    public class EntityReferenceDataAgentProviderCodeGenerator : CodeGeneratorBase<CodeGenConfig> 
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).EntityScope == "Common" ? new CodeGenConfig[] { config } : System.Array.Empty<CodeGenConfig>();
    }
}