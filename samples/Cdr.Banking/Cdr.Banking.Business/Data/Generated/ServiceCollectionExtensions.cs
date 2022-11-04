/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides the generated <b>Data</b>-layer services.
    /// </summary>
    public static partial class ServiceCollectionsExtension
    {
        /// <summary>
        /// Adds the generated <b>Data</b>-layer services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGeneratedDataServices(this IServiceCollection services)
        {
            return services.AddScoped<IAccountData, AccountData>()
                           .AddScoped<ITransactionData, TransactionData>();
        }
    }
}

#pragma warning restore
#nullable restore