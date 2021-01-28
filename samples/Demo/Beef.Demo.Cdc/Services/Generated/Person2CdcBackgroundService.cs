/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using Beef;
using Beef.Data.Database.Cdc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Beef.Demo.Cdc.Data;

namespace Beef.Demo.Cdc.Services
{
    /// <summary>
    /// Provides the CDC background service for database object 'Demo.Person2'.
    /// </summary>
    public partial class Person2CdcBackgroundService : CdcBackgroundService<IPerson2CdcData>
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Person2CdcBackgroundService"/> class.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public Person2CdcBackgroundService(IConfiguration config, IServiceProvider serviceProvider, ILogger<Person2CdcBackgroundService> logger) :
            base(serviceProvider, logger) => _config = Check.NotNull(config, nameof(config));

        /// <summary>
        /// Gets the interval seconds between each execution.
        /// </summary>
        public override int? IntervalSeconds => _config.GetValue<int?>("Person2CdcIntervalSeconds");
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore