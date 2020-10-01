// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database View code generator.
    /// </summary>
    public class DatabaseViewCodeGenerator : CodeGeneratorBase<CodeGenConfig, TableConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<TableConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Tables!.Where(x => x.View.HasValue && x.View.Value);
    }
}