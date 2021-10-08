using Beef.CodeGen.Abstractions.Test.Config;
using Beef.CodeGen.Generators;
using System.Collections.Generic;

namespace Beef.CodeGen.Abstractions.Test.Generators
{
    public class PropertyGenerator : CodeGeneratorBase<EntityConfig, PropertyConfig>
    {
        protected override IEnumerable<PropertyConfig> SelectGenConfig(EntityConfig config) => config.Properties!;
    }
}