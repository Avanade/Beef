// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Enables the <b>Beef</b> database extension(s).
    /// </summary>
    public static class EfDbExtensions
    {
        /// <summary>
        /// Adds the required <b>CosmosDb/DocumentDb</b> <i>singleton</i> services.
        /// </summary>
        /// <typeparam name="TCosmosDb">The <b>CosmosDb/DocumentDb</b> <see cref="ICosmosDb"/> <see cref="Type"/>.</typeparam>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The <see cref="IConfigurationSection"/> with the following keys: <c>EndPoint</c>, <c>AuthKey</c> and <c>Database</c>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        /// <remarks>Uses reflection to instantiate the <typeparamref name="TCosmosDb"/> using the constructor <c>Ctor(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = null)</c>.
        /// The <c>createDatabaseIfNotExists</c> will be set to <c>false</c>, and the <c>throughput</c> set to <c>null</c>.</remarks>
        public static IServiceCollection AddBeefCosmosDbServices<TCosmosDb>(this IServiceCollection serviceCollection, IConfigurationSection config) where TCosmosDb : ICosmosDb
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var ctor = typeof(TCosmosDb).GetConstructor(new Type[] { typeof(CosmosClient), typeof(string), typeof(bool), typeof(int?) })
                ?? throw new InvalidOperationException("TCosmosDb does not have the required constructor that accepts four arguments: Ctor(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = null).");

            serviceCollection.AddSingleton<ICosmosDb>(_ =>
            {
                var client = new CosmosClient(config.GetValue<string>("EndPoint"), config.GetValue<string>("AuthKey"));
                return (TCosmosDb)ctor.Invoke(new object[] { client, config.GetValue<string>("Database"), false, null! });
            });

            return serviceCollection;
        }
    }
}