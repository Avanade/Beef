// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database View code generator.
    /// </summary>
    public class DatabaseChangeDataCaptureCodeGenerator : CodeGeneratorBase<CodeGenConfig, CdcConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CdcConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Cdc!;
    }
}