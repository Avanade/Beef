// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the named Database Change Data Capture (CDC) generator.
    /// </summary>
    public class DatabaseCdcNamedCodeGenerator : CodeGeneratorBase<CodeGenConfig, CdcConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<CdcConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Cdc!.Where(x => x.Name == x.Root!.GetRuntimeParameter<string>("CdcName"));
    }
}