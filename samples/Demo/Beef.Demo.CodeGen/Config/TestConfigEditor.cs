using Beef.CodeGen.Config;
using Beef.CodeGen.Config.Entity;

namespace Beef.Demo.CodeGen.Config
{
    public class TestConfigEditor : IConfigEditor
    {
        public void EditConfig(ConfigBase config)
        {
            var cgc = (CodeGenConfig)config;
            foreach (var e in cgc.Entities)
            {
                // Look for the additional property added in the configuration file.
                if (e.TryGetExtraProperty<bool>("TestCodeGen", out var val) && val)
                    e.CustomProperties["TestExtra"] = $"XXX.{e.GetExtraProperty<string>("TestExtra")}.XXX"; // Add a new custom property that can be referenced in the template.
            }
        }
    }
}