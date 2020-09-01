﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>EntityData</b> code generator; where not excluded and at least one operation exists.
    /// </summary>
    public class EntityDataCodeGenerator : CodeGeneratorBase<Config.Entity.CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(Config.Entity.CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Where(x => x.ExcludeData == "Mapper" || (x.ExcludeData == "No" && x.Operations!.Count > 0)).AsEnumerable();
    }
}