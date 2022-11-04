// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the primary <b>entity</b> code generator; where not excluded and where not omitting entity base capabilities.
    /// </summary>
    public class EntityCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config) => config.Entities!.Where(x => IsFalse(x.ExcludeEntity) && IsFalse(x.OmitEntityBase) && x.Root!.RuntimeEntityScope == "Business").AsEnumerable();
    }
}