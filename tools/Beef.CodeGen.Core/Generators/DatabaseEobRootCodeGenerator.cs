// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Event Outbox (EOB) generator.
    /// </summary>
    public class DatabaseEobRootCodeGenerator : CodeGeneratorBase<CodeGenConfig, CodeGenConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).EventOutbox == true ? new CodeGenConfig[] { config } : System.Array.Empty<CodeGenConfig>();
    }
}