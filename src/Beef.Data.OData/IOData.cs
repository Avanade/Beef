// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;
using Soc = Simple.OData.Client;

namespace Beef.Data.OData
{
    /// <summary>
    /// Provides the <b>OData</b> capabilities.
    /// </summary>
    public interface IOData
    {
        /// <summary>
        /// Gets the underlying <see cref="Soc.ODataClient"/>.
        /// </summary>
        Soc.ODataClient Client { get; }

        /// <summary>
        /// Gets the <see cref="Soc.ODataClientSettings"/>.
        /// </summary>
        Soc.ODataClientSettings ClientSettings { get; }

        /// <summary>
        /// Creates an <see cref="ODataQuery{T, TModel}"/> to enable select-like capabilities. 
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="query">The function to further define the query.</param>
        /// <returns>A <see cref="ODataQuery{T, TModel}"/>.</returns>
        ODataQuery<T, TModel> Query<T, TModel>(ODataArgs queryArgs, Func<Soc.IBoundClient<TModel>, Soc.IBoundClient<TModel>>? query = null) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> (mapping from <typeparamref name="TModel"/> to <typeparamref name="T"/>) asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        Task<T?> GetAsync<T, TModel>(ODataArgs getArgs, params IComparable?[] keys) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Creates the entity (mapping from <typeparamref name="T"/> to <typeparamref name="TModel"/>) asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The created entity value.</returns>
        Task<T> CreateAsync<T, TModel>(ODataArgs saveArgs, T value) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Updates the entity (mapping from <typeparamref name="T"/> to <typeparamref name="TModel"/>) asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The value to update.</param>
        /// <returns>The updated entity value.</returns>
        Task<T> UpdateAsync<T, TModel>(ODataArgs saveArgs, T value) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Deletes the entity for the specified <paramref name="keys"/> asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task DeleteAsync<T, TModel>(ODataArgs saveArgs, params IComparable?[] keys) where T : class, new() where TModel : class, new();
    }
}