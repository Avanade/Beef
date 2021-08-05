// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides the <b>CosmosDb/DocumentDb</b> capabilities.
    /// </summary>
    public interface ICosmosDb
    {
        /// <summary>
        /// Gets the underlying <see cref="Microsoft.Azure.Cosmos.CosmosClient"/>.
        /// </summary>
        CosmosClient Client { get; }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.Database"/>.
        /// </summary>
        Database Database { get; }

        /// <summary>
        /// Gets the specified <see cref="Container"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <returns>The selected <see cref="Container"/>.</returns>
        Container CosmosContainer(string containerId);

        /// <summary>
        /// Gets (creates) the <see cref="CosmosDbContainer{T, TModel}"/> using the specified <paramref name="dbArgs"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <returns>The <see cref="CosmosDbContainer{T, TModel}"/>.</returns>
        CosmosDbContainer<T, TModel> Container<T, TModel>(CosmosDbArgs dbArgs) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Gets (creates) the <see cref="CosmosDbValueContainer{T, TModel}"/> using the specified <paramref name="dbArgs"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <returns>The <see cref="CosmosDbValueContainer{T, TModel}"/>.</returns>
        CosmosDbValueContainer<T, TModel> ValueContainer<T, TModel>(CosmosDbArgs dbArgs) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Replace or create the <see cref="Container"/> asynchronously.
        /// </summary>
        /// <param name="containerProperties">The <see cref="ContainerProperties"/> used for the create.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        /// <returns>The replaced/created <see cref="Container"/>.</returns>
        Task<Container> ReplaceOrCreateContainerAsync(ContainerProperties containerProperties, int? throughput = 400);

        /// <summary>
        /// Gets (creates) a <see cref="CosmosDbQuery{T, TModel}"/> to enable LINQ-style queries.
        /// </summary>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <param name="query">The function to perform additional query execution.</param>
        /// <returns>The <see cref="CosmosDbQuery{T, TModel}"/>.</returns>
        CosmosDbQuery<T, TModel> Query<T, TModel>(CosmosDbArgs dbArgs, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Gets (creates) a <see cref="CosmosDbValueQuery{T, TModel}"/> to enable LINQ-style queries.
        /// </summary>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <param name="query">The function to perform additional query execution.</param>
        /// <returns>The <see cref="CosmosDbValueQuery{T, TModel}"/>.</returns>
        CosmosDbValueQuery<T, TModel> ValueQuery<T, TModel>(CosmosDbArgs dbArgs, Func<IQueryable<CosmosDbValue<TModel>>, IQueryable<CosmosDbValue<TModel>>>? query = null) where T : class, new() where TModel : class, new();
    }
}