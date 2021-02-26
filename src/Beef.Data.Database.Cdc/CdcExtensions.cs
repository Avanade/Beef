using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the extensions methods for the Change Data Capture (CDC) capabilities.
    /// </summary>
    public static class CdcExtensions
    {
        /// <summary>
        /// Gets the services configuration name.
        /// </summary>
        public const string ServicesName = "Services";

        private const string _suffix = "BackgroundService";

        /// <summary>
        /// Adds a <typeparamref name="TCdcService"/> as an <see cref="IHostedService"/>. Before adding checks whether the <typeparamref name="TCdcService"/> has been specified within the 
        /// comma-separated list of <c>Services</c> defined in the <paramref name="config"/>.
        /// </summary>
        /// <typeparam name="TCdcService">The <see cref="CdcBackgroundService"/> / <see cref="IHostedService"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddCdcHostedService<TCdcService>(this IServiceCollection services, IConfiguration config) where TCdcService : CdcBackgroundService, IHostedService
        {
            var svcs = (config ?? throw new ArgumentNullException(nameof(config))).GetValue<string?>(ServicesName);
            if (svcs == null)
            {
                services.AddHostedService<TCdcService>();
                return services;
            }

            var svcsList = svcs.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (svcsList.Contains(typeof(TCdcService).Name, StringComparer.OrdinalIgnoreCase) || (typeof(TCdcService).Name.EndsWith(_suffix) && svcsList.Contains(typeof(TCdcService).Name[..^_suffix.Length])))
                services.AddHostedService<TCdcService>();

            return services;
        }
    }
}
