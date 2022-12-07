// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the omit base <b>entity</b> code generator; where the <see cref="EntityConfig.OmitEntityBase"/> is <c>true</c>.
    /// </summary>
    public class EntityOmitBaseCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config) => config.Entities!.Where(x => IsFalse(x.ExcludeEntity) && (IsTrue(x.OmitEntityBase) || (x.Root!.RuntimeEntityScope == "Common" && IsFalse(x.InternalOnly)))).AsEnumerable();
    }
}