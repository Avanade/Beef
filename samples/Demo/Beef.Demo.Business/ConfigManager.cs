using System;
using System.Threading.Tasks;

namespace Beef.Demo.Business
{
    public partial class ConfigManager
    {
        private Task<System.Collections.IDictionary> GetEnvVarsOnImplementationAsync() => Task.FromResult<System.Collections.IDictionary>(Environment.GetEnvironmentVariables());
    }
}