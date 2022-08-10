// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the Database Delete stored procedure code generator.
    /// </summary>
    public class DatabaseDeleteCodeGenerator : CodeGeneratorBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<StoredProcedureConfig> SelectGenConfig(CodeGenConfig config)
            => (config ?? throw new System.ArgumentNullException(nameof(config))).Tables!.SelectMany(x => x.StoredProcedures!).Where(x => x.Type == "Delete");
    }
}