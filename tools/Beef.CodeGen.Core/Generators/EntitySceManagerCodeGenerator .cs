// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>Manager</b> Service Collection Extensions code generator; where not excluded and at least one operation exists.
    /// </summary>
    public class EntitySceManagerCodeGenerator : CodeGeneratorBase<Config.Entity.CodeGenConfig, Config.Entity.CodeGenConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<Config.Entity.CodeGenConfig> SelectGenConfig(Config.Entity.CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Any(x => IsFalse(x.ExcludeManager) && x.Operations!.Count > 0) ? new Config.Entity.CodeGenConfig[] { config } : System.Array.Empty<Config.Entity.CodeGenConfig>();
    }
}