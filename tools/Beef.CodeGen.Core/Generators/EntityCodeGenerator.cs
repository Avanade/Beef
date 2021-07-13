// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the primary <b>entity</b> code generator; where not excluded, within the selected scope, and where not omitting entity base capabilities.
    /// </summary>
    public class EntityCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Where(x => IsFalse(x.ExcludeEntity) && ((IsFalse(x.OmitEntityBase) && x.EntityScope == x.Root!.RuntimeEntityScope) || (x.EntityScope == "Autonomous" && x.Root!.RuntimeEntityScope == "Business"))).AsEnumerable();
    }
}