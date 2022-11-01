// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using OnRamp.Generators;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Event Outbox generator.
    /// </summary>
    public class DatabaseOutboxCodeGenerator : CodeGeneratorBase<CodeGenConfig, CodeGenConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
            => (config ?? throw new System.ArgumentNullException(nameof(config))).Outbox == true ? new CodeGenConfig[] { config } : System.Array.Empty<CodeGenConfig>();
    }
}