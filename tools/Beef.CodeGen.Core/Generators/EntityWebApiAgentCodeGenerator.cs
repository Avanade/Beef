// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>WebAPI AgentController</b> code generator; where not excluded and at least one operation exists.
    /// </summary>
    public class EntityWebApiAgentCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Where(x => IsFalse(x.ExcludeWebApiAgent) && x.Operations!.Count > 0).AsEnumerable();
    }
}