// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides the entity framework capabilities.
    /// </summary>
    public interface IEfDb
    {
#pragma warning disable CA1716 // Identifiers should not match keywords; by-design (has not been an issue so far!)
        /// <summary>
        /// Invokes the <paramref name="action"/> whilst <see cref="Beef.Data.Database.DatabaseWildcard.Replace(string)">replacing</see> the <b>wildcard</b> characters when the <paramref name="with"/> is not <c>null</c>.
        /// </summary>
        /// <param name="with">The value with which to verify.</param>
        /// <param name="action">The <see cref="Action"/> to invoke when there is a valid <paramref name="with"/> value; passed the database specific wildcard value.</param>
        void WithWildcard(string? with, Action<string> action);

        /// <summary>
        /// Invokes the <paramref name="action"/> when the <paramref name="with"/> is not the default value for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The with value <see cref="Type"/>.</typeparam>
        /// <param name="with">The value with which to verify.</param>
        /// <param name="action">The <see cref="Action"/> to invoke when there is a valid <paramref name="with"/> value.</param>
        void With<T>(T with, Action action);
#pragma warning restore CA1716

        /// <summary>
        /// Creates an <see cref="EfDbQuery{T, TModel, TDbContext}"/> to enable select-like capabilities.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="EfDbArgs{T, TModel}"/>.</param>
        /// <param name="query">The function to further define the query.</param>
        /// <returns>A <see cref="EfDbQuery{T, TModel, TDbContext}"/>.</returns>
        IEfDbQuery<T, TModel> Query<T, TModel>(EfDbArgs<T, TModel> queryArgs, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> mapping from <typeparamref name="TModel"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="EfDbArgs{T, TModel}"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        Task<T?> GetAsync<T, TModel>(EfDbArgs<T, TModel> getArgs, params IComparable[] keys) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Performs a create for the value (reselects and/or automatically saves changes where specified).
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="EfDbArgs{T, TModel}"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (refreshed where specified).</returns>
        Task<T> CreateAsync<T, TModel>(EfDbArgs<T, TModel> saveArgs, T value) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Performs an update for the value (reselects and/or automatically saves changes where specified).
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="EfDbArgs{T, TModel}"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (refreshed where specified).</returns>
        Task<T> UpdateAsync<T, TModel>(EfDbArgs<T, TModel> saveArgs, T value) where T : class, new() where TModel : class, new();

        /// <summary>
        /// Performs a delete for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="EfDbArgs{T, TModel}"/>.</param>
        /// <param name="keys">The key values.</param>
        Task DeleteAsync<T, TModel>(EfDbArgs<T, TModel> saveArgs, params IComparable[] keys) where T : class, new() where TModel : class, new();
    }
}