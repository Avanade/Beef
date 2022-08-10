// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>model</b> code generator; assumes all entities will be generated as a model unless explicitly excluded.
    /// </summary>
    public class EntityModelCodeGenerator : CodeGeneratorBase<Config.Entity.CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(Config.Entity.CodeGenConfig config)
            => (config ?? throw new System.ArgumentNullException(nameof(config))).Entities!.Where(x => IsFalse(x.ExcludeEntity)).AsEnumerable();
    }
}