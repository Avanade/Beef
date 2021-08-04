using Beef.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Text;

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

        private const string _suffix = "HostedService";  // Standard (code-generated) naming convention.

        /// <summary>
        /// Adds a <typeparamref name="TCdcService"/> as an <see cref="IHostedService"/>. Before adding checks whether the <typeparamref name="TCdcService"/> has been specified within the 
        /// comma-separated list of <c>Services</c> defined in the <paramref name="config"/>.
        /// </summary>
        /// <typeparam name="TCdcService">The <see cref="CdcHostedService"/> / <see cref="IHostedService"/> <see cref="Type"/>.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddCdcHostedService<TCdcService>(this IServiceCollection services, IConfiguration config) where TCdcService : CdcHostedService, IHostedService
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

        /// <summary>
        /// Creates the formatted <i>identifier mapping</i> key (as a <see cref="string"/>) for the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The <i>identifier mapping</i> key for the <paramref name="value"/>.</returns>
        public static string CreateIdentifierMappingKey(this object value)
        {
            var sb = new StringBuilder();
            switch (value ?? throw new ArgumentNullException(nameof(value)))
            {
                case IInt32Identifier ii:
                    sb.Append(ii.Id);
                    break;

                case IInt64Identifier il:
                    sb.Append(il.Id);
                    break;

                case IGuidIdentifier gi:
                    sb.Append(gi.Id);
                    break;

                case IStringIdentifier si:
                    sb.Append(si.Id);
                    break;

                case IUniqueKey uk:
                    if (uk.UniqueKey.Args.Length == 0)
                        throw new InvalidOperationException("A Value that implements IUniqueKey must have a unique key value.");

                    for (int i = 0; i < uk.UniqueKey.Args.Length; i++)
                    {
                        if (i > 0)
                            sb.Append(',');

                        sb.Append(uk.UniqueKey.Args[i]);
                    }

                    break;

                default:
                    throw new InvalidOperationException("Type must implement at least one of the following: IIdentifier, IGuidIdentifier, IStringIdentifier or IUniqueKey.");
            }

            return sb.ToString();
        }
    }
}