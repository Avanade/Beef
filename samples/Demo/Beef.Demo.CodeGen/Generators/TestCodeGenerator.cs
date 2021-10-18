using Beef.CodeGen.Config.Entity;
using OnRamp.Generators;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Demo.CodeGen.Generators
{
    public class TestCodeGenerator : CodeGeneratorBase<CodeGenConfig, EntityConfig>
    {
        protected override IEnumerable<EntityConfig> SelectGenConfig(CodeGenConfig config)
            => Check.NotNull(config, nameof(config)).Entities.Where(x => x.GetExtraProperty<bool>("TestCodeGen")).AsEnumerable();
    }
}