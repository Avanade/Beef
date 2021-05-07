// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Caching.Policy
{
    /// <summary>
    /// The <see cref="TimerHostedServiceBase"/> to perform the periodic <see cref="CachePolicyManager.Flush"/>.
    /// </summary>
    public class CachePolicyManagerServiceHost : TimerHostedServiceBase
    {
        private readonly CachePolicyManager _cachePolicyManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicyManagerServiceHost"/> class.
        /// </summary>
        /// <param name="cachePolicyManager">The <see cref="CachePolicyManager"/>.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>; defaults to instance from the <paramref name="serviceProvider"/> where not specified.</param>
        public CachePolicyManagerServiceHost(CachePolicyManager cachePolicyManager, IServiceProvider serviceProvider, ILogger logger, IConfiguration? config = null) : base(serviceProvider, logger, config)
            => _cachePolicyManager = Check.NotNull(cachePolicyManager, nameof(cachePolicyManager));

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="cancellationToken"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _cachePolicyManager.Flush(Logger);
            return Task.CompletedTask;
        }
    }
}