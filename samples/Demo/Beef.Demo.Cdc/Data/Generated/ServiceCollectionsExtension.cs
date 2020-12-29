/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable

using Microsoft.Extensions.DependencyInjection;

namespace Beef.Demo.Cdc.Data
{
    /// <summary>
    /// Provides the generated CDC <b>Data</b>-layer services.
    /// </summary>
    public static class ServiceCollectionsExtension
    {
        /// <summary>
        /// Adds the generated CDC <b>Data</b>-layer services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGeneratedCdcDataServices(this IServiceCollection services)
        {
            return services.AddScoped<IPostsCdcData, PostsCdcData>();
        }
    }
}

#nullable restore