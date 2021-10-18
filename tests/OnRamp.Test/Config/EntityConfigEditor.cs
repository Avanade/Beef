using OnRamp.Config;

namespace OnRamp.Test.Config
{
    public class EntityConfigEditor : IConfigEditor
    {
        public void BeforePrepare(IRootConfig config)
        {
            var ec = config as EntityConfig;
            ec.Name = ec.Name.ToUpperInvariant();
        }
    }
}