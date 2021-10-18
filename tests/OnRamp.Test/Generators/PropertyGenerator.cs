using OnRamp.Test.Config;
using OnRamp.Generators;
using System.Collections.Generic;

namespace OnRamp.Test.Generators
{
    public class PropertyGenerator : CodeGeneratorBase<EntityConfig, PropertyConfig>
    {
        protected override IEnumerable<PropertyConfig> SelectGenConfig(EntityConfig config) => config.Properties!;
    }
}