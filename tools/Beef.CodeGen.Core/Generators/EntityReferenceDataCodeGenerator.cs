// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents the <b>Root</b> code generator for <see cref="CodeGenConfig"/>.
    /// </summary>
    public class EntityReferenceDataCodeGenerator : CodeGeneratorBase<CodeGenConfig> 
    {
        /// <inheritdoc/>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config) => new CodeGenConfig[] { config };
    }
}