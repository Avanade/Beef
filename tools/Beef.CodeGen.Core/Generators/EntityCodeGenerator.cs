// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the primary <b>entity</b> code generator; where not excluded, within the selected scope, and where not omitting entity base capabilities.
    /// </summary>
    public class EntityCodeGenerator : CodeGeneratorBase<Config.Entity.CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(Config.Entity.CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Where(x => IsTrue(x.ExcludeEntity) && x.EntityScope == x.Parent!.EntityScope && IsFalse(x.OmitEntityBase)).AsEnumerable();
    }
}