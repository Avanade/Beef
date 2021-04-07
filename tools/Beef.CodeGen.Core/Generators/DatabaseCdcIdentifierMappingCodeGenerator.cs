// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Change Data Capture (CDC) <c>IdentifierMapping</c> generator.
    /// </summary>
    public class DatabaseCdcIdentifierMappingCodeGenerator : CodeGeneratorBase<CodeGenConfig, CodeGenConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config)
            => IsTrue(Check.NotNull(config, nameof(config)).CdcIdentifierMapping) && config!.Cdc!.Count > 0 ? new CodeGenConfig[] { config } : System.Array.Empty<CodeGenConfig>();
    }
}