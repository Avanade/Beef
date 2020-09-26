// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Upsert stored procedure code generator.
    /// </summary>
    public class DatabaseUpsertCodeGenerator : CodeGeneratorBase<Config.Database.CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<StoredProcedureConfig> SelectGenConfig(Config.Database.CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Tables.SelectMany(x => x.StoredProcedures).Where(x => x.Type == "Upsert");
    }
}