/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, CA2227, CA1819, CA1056, CA1034

using Beef;
using Beef.Data.Database.Cdc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Beef.Demo.Cdc.Data;

namespace Beef.Demo.Cdc.Services
{
    /// <summary>
    /// Provides the CDC background service for database object 'Legacy.Posts'.
    /// </summary>
    public class PostsCdcBackgroundService : CdcBackgroundService<IPostsCdcData>
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostsCdcBackgroundService"/> class.
        /// </summary>
        /// <param name="data">The <see cref="IPostsCdcData"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public PostsCdcBackgroundService(IPostsCdcData data, IConfiguration config, IServiceProvider serviceProvider, ILogger<PostsCdcBackgroundService> logger) :
            base(data, serviceProvider, logger) => _config = Check.NotNull(config, nameof(config));

        /// <summary>
        /// Gets the service name.
        /// </summary>
        public override string ServiceName => nameof(PostsCdcBackgroundService);

        /// <summary>
        /// Gets the interval seconds between each execution.
        /// </summary>
        public override int IntervalSeconds => _config.GetValue<int>("PostsCdcIntervalSeconds");
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, CA2227, CA1819, CA1056, CA1034
#nullable restore