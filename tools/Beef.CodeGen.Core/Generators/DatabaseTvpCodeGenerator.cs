// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Table code generator.
    /// </summary>
    public class DatabaseTvpCodeGenerator : CodeGeneratorBase<CodeGenConfig, TableConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<TableConfig> SelectGenConfig(CodeGenConfig config)
            => (config ?? throw new System.ArgumentNullException(nameof(config))).Tables!.Where(x => !string.IsNullOrEmpty(x.Tvp));
    }
}