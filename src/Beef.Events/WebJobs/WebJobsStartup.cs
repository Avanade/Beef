// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Beef.Events.WebJobs.WebJobsStartup))]

namespace Beef.Events.WebJobs
{
    /// <summary>
    /// Provides the webjobs startup capabilities.
    /// </summary>
    public class WebJobsStartup : IWebJobsStartup
    {
        /// <summary>
        /// Configure the auto-registering of the "resilient event hubs".
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/>.</param>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddBeefResilientEventHubs();
        }
    }
}