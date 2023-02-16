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
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities!.Where(x => CheckEntity(x)).AsEnumerable();

        /// <summary>
        /// Check the entity configuration.
        /// </summary>
        private bool CheckEntity(EntityConfig ec)
        {
            if (IsTrue(ec.ExcludeEntity))
                return false;

            if (ec.Root!.RuntimeEntityScope == "Common")
            {
                if (ec.EntityScope == "Business")
                    return false;

                if (ec.EntityScope == "Autonomous")
                    return true;

                return IsTrue(ec.OmitEntityBase);
            }

            if (ec.EntityScope == "Common")
                return false;

            if (IsFalse(ec.OmitEntityBase))
                return false;

            return true;
        }
    }
}