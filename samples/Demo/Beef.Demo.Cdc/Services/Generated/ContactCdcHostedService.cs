/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Beef;
using Beef.Data.Database.Cdc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using Beef.Demo.Cdc.Data;

namespace Beef.Demo.Cdc.Services
{
    /// <summary>
    /// Provides the <see cref="CdcHostedService"/> capabilities for database object 'Legacy.Contact'.
    /// </summary>
    public partial class ContactCdcHostedService : CdcHostedService<IContactCdcData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCdcHostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ContactCdcHostedService(IServiceProvider serviceProvider, ILogger<ContactCdcHostedService> logger, IConfiguration? config = null) : base(serviceProvider, logger, config) { }
    }
}

#pragma warning restore
#nullable restore