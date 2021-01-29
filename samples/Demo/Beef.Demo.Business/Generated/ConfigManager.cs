/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Entities;
using Beef.Validation;
using Beef.Demo.Common.Entities;
using Beef.Demo.Business.DataSvc;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business
{
    /// <summary>
    /// Provides the <b>Config</b> business functionality.
    /// </summary>
    public partial class ConfigManager : IConfigManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigManager"/> class.
        /// </summary>
        public ConfigManager()
            { ConfigManagerCtor(); }

        partial void ConfigManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get Env Vars.
        /// </summary>
        /// <returns>A resultant <see cref="System.Collections.IDictionary"/>.</returns>
        public async Task<System.Collections.IDictionary> GetEnvVarsAsync()
        {
            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Unspecified;
                return Cleaner.Clean(await GetEnvVarsOnImplementationAsync().ConfigureAwait(false));
            }).ConfigureAwait(false);
        }
    }
}

#pragma warning restore
#nullable restore