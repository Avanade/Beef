// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Ef Model code generator.
    /// </summary>
    public class DatabaseEfModelGenerator : CodeGeneratorBase<Config.Database.CodeGenConfig, TableConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<TableConfig> SelectGenConfig(Config.Database.CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).EFModels;
    }
}