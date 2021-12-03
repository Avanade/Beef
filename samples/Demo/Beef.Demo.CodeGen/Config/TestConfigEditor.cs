using Beef.CodeGen.Config.Entity;
using Newtonsoft.Json.Linq;
using OnRamp.Config;
using System.Threading.Tasks;

namespace Beef.Demo.CodeGen.Config
{
    public class TestConfigEditor : IConfigEditor
    {
        public Task AfterPrepareAsync(IRootConfig config)
        {
            var cgc = (CodeGenConfig)config;
            foreach (var e in cgc.Entities)
            {
                // Look for the additional property added in the configuration file.
                if (e.TryGetExtraProperty("TestCodeGen", out JValue val) && val.ToObject<bool>())
                    e.CustomProperties["TestExtra"] = $"XXX.{e.GetExtraProperty<JValue>("TestExtra")}.XXX"; // Add a new custom property that can be referenced in the template.
            }

            return Task.CompletedTask;
        }
    }
}