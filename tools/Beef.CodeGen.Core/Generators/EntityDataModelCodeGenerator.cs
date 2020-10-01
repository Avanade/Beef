// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>entity model</b> code generator; where the <see cref="EntityConfig.DataModel"/> is <c>true</c>.
    /// </summary>
    public class EntityDataModelCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Where(x => IsTrue(x.DataModel)).AsEnumerable();
    }
}