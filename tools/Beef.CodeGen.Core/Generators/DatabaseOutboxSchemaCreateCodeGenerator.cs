// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
using OnRamp.Generators;
using System.Collections.Generic;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Represents a <see cref="CodeGenConfig.OutboxSchemaCreate"/>-driven code generator.
    /// </summary>
    public class DatabaseOutboxSchemaCreateCodeGenerator : CodeGeneratorBase<CodeGenConfig, CodeGenConfig>
    {
        /// <inheritdoc/>
        protected override IEnumerable<CodeGenConfig> SelectGenConfig(CodeGenConfig config) => IsTrue(config.OutboxSchemaCreate) ? config.SelectGenResult : null!;
    }
}