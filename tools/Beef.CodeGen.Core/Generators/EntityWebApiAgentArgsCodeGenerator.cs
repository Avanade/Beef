// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <see cref="Beef.WebApi.IWebApiAgentArgs"/> code generator; where not excluded and at least one operation exists.
    /// </summary>
    public class EntityWebApiAgentArgsCodeGenerator : CodeGeneratorBase<CodeGenConfig, CodeGenConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).AppBasedAgentArgs ? new CodeGenConfig[] { config } : System.Array.Empty<CodeGenConfig>();
    }
}