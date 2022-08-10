﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>Grpc EntityService</b> code generator; where not excluded and at least one operation exists.
    /// </summary>
    public class EntityGrpcServiceCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config)
            => (config ?? throw new System.ArgumentNullException(nameof(config))).Entities!.Where(x => IsTrue(x.Grpc) && x.GrpcOperations!.Count > 0).AsEnumerable();
    }
}