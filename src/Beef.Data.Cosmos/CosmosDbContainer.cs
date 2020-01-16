﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Cosmos.Internal;
using Beef.Entities;
using Beef.RefData;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides all of the common <see cref="Container"/> operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public interface ICosmosDbContainer<T, TModel> where T : class, new() where TModel : class, new()
    {
        /// <summary>
        /// Gets the owning <see cref="CosmosDbBase"/>.
        /// </summary>
        CosmosDbBase CosmosDb { get; }

        /// <summary>
        /// Gets or sets the <see cref="CosmosDbArgs{T, TModel}"/> (used for all operations).
        /// </summary>
        CosmosDbArgs<T, TModel> DbArgs { get; }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.Container"/>.
        /// </summary>
        Container Container { get; }
    }

    /// <summary>
    /// Enables all of the common <see cref="Container"/> operations for a specified <see cref="CosmosDb"/> and <see cref="DbArgs"/>.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
    public class CosmosDbContainer<T, TModel> : ICosmosDbContainer<T, TModel> where T : class, new() where TModel : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbContainer{T, TModel}"/> class.
        /// </summary>
        /// <param name="cosmosDb">The <see cref="CosmosDb"/>.</param>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs{T, TModel}"/>.</param>
        public CosmosDbContainer(CosmosDbBase cosmosDb, CosmosDbArgs<T, TModel> dbArgs)
        {
            CosmosDb = cosmosDb ?? throw new ArgumentNullException(nameof(cosmosDb));
            DbArgs = dbArgs ?? throw new ArgumentNullException(nameof(dbArgs));
            Container = cosmosDb.CosmosContainer(DbArgs.ContainerId);
        }

        /// <summary>
        /// Gets the owning <see cref="CosmosDbBase"/>.
        /// </summary>
        public CosmosDbBase CosmosDb { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="CosmosDbArgs{T, TModel}"/> (used for all operations).
        /// </summary>
        public CosmosDbArgs<T, TModel> DbArgs { get; private set; }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.Container"/>.
        /// </summary>
        public Container Container { get; private set; }

        /// <summary>
        /// Gets the <b>value</b> from the response updating any special properties as required.
        /// </summary>
        /// <param name="resp">The response value.</param>
        /// <returns>The entity value.</returns>
        internal T GetResponseValue(Response<TModel> resp)
        {
            if (resp?.Resource == null)
                return default;

            return GetValue(resp.Resource);
        }

        /// <summary>
        /// Gets the <b>value</b> formatting/updating any special properties as required.
        /// </summary>
        /// <param>The model value.</param>
        /// <returns>The entity value.</returns>
        internal T GetValue(TModel model)
        {
            CosmosDbBase.ReformatValueETag(model);
            return DbArgs.Mapper.MapToSrce(model, Mapper.OperationTypes.Get);
        }

        /// <summary>
        /// Check the value to determine whether users are authorised using the CosmosDbArgs.AuthorizationFilter.
        /// </summary>
        private void CheckAuthorized(TModel model)
        {
            if (model != default && DbArgs.AuthorizationFilter != null && !((IQueryable<TModel>)DbArgs.AuthorizationFilter(new TModel[] { model }.AsQueryable())).Any())
                throw new AuthorizationException();
        }

        #region Query

        /// <summary>
        /// Gets (creates) a <see cref="CosmosDbQuery{T, TModel}"/> to enable LINQ-style queries.
        /// </summary>
        /// <param name="query">The function to perform additional query execution.</param>
        /// <returns>The <see cref="CosmosDbQuery{T, TModel}"/>.</returns>
        public CosmosDbQuery<T, TModel> Query(Func<IQueryable<TModel>, IQueryable<TModel>> query = null)
        {
            return new CosmosDbQuery<T, TModel>(this, query);
        }

        /// <summary>
        /// Creates a <see cref="CosmosDbQuery{T, TModel}"/> and returns the corresponding <see cref="CosmosDbQuery{T, TModel}.AsQueryable()"/> to enable ad-hoc LINQ-style queries.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/>.</returns>
        /// <remarks>The <see cref="ICosmosDbArgs.Paging"/> is not supported.</remarks>
        public IQueryable<TModel> AsQueryable()
        {
            return new CosmosDbQuery<T, TModel>(this).AsQueryable();
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the <b>CosmosDb/DocumentDb</b> entity for the specified <paramref name="keys"/> converting to <typeparamref name="T"/> asynchronously.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c> (see <see cref="ICosmosDbArgs.NullOnNotFoundResponse"/>).</returns>
        public async Task<T> GetAsync(params IComparable[] keys)
        {
            var key = DbArgs.GetCosmosKey(keys);

            return await CosmosDbInvoker.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    var val = await Container.ReadItemAsync<TModel>(key, DbArgs.PartitionKey, CosmosDb.GetItemRequestOptions(DbArgs)).ConfigureAwait(false);
                    CheckAuthorized(val);
                    return GetResponseValue(val);
                }
                catch (CosmosException dcex)
                {
                    if (DbArgs.NullOnNotFoundResponse && dcex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    throw;
                }
            }, CosmosDb).ConfigureAwait(false);
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates the <b>CosmosDb/DocumentDb</b> entity asynchronously.
        /// </summary>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public async Task<T> CreateAsync(T value)
        {
            Check.NotNull(value, nameof(value));

            return await CosmosDbInvoker.Default.InvokeAsync(this, async () =>
            {
                CosmosDbBase.PrepareEntityForCreate(value, DbArgs.SetIdentifierOnCreate);
                var model = DbArgs.Mapper.MapToDest(value, Mapper.OperationTypes.Create);
                CheckAuthorized(model);

                var resp = await Container.CreateItemAsync(model, DbArgs.PartitionKey, CosmosDb.GetItemRequestOptions(DbArgs)).ConfigureAwait(false);
                return GetResponseValue(resp);
            }, CosmosDb).ConfigureAwait(false);
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the <b>CosmosDb/DocumentDb</b> entity asynchronously.
        /// </summary>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public async Task<T> UpdateAsync(T value)
        {
            Check.NotNull(value, nameof(value));

            return await CosmosDbInvoker.Default.InvokeAsync(this, async () =>
            {
                // Where supporting etag then use IfMatch for concurreny.
                var ro = CosmosDb.GetItemRequestOptions(DbArgs);
                if (ro.IfMatchEtag == null && value is IETag etag)
                    ro.IfMatchEtag = etag.ETag.StartsWith("\"", StringComparison.InvariantCultureIgnoreCase) ? etag.ETag : "\"" + etag.ETag + "\"";

                string key = DbArgs.GetCosmosKey(value);
                CosmosDbBase.PrepareEntityForUpdate(value);
                    
                // Must read existing to update.
                var resp = await Container.ReadItemAsync<TModel>(key, DbArgs.PartitionKey, ro).ConfigureAwait(false);
                CheckAuthorized(resp);

                ro.SessionToken = resp.Headers?.Session;
                DbArgs.Mapper.MapToDest(value, resp.Resource, Mapper.OperationTypes.Update);
                resp = await Container.ReplaceItemAsync(resp.Resource, key, DbArgs.PartitionKey, ro).ConfigureAwait(false);
                return GetResponseValue(resp);
            }, CosmosDb).ConfigureAwait(false);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the <b>CosmosDb/DocumentDb</b> entity asynchronously.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task DeleteAsync(params IComparable[] keys)
        {
            var key = DbArgs.GetCosmosKey(keys);

            await CosmosDbInvoker.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    // Must read the existing to validate.
                    var ro = CosmosDb.GetItemRequestOptions(DbArgs);
                    var resp = await Container.ReadItemAsync<TModel>(key, DbArgs.PartitionKey, ro).ConfigureAwait(false);
                    if (resp?.Resource == null)
                        return;

                    CheckAuthorized(resp.Resource);
                    await Container.DeleteItemAsync<T>(key, DbArgs.PartitionKey, CosmosDb.GetItemRequestOptions(DbArgs)).ConfigureAwait(false);
                }
                catch (CosmosException cex)
                {
                    if (cex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return;

                    throw;
                }
            }, CosmosDb).ConfigureAwait(false);
        }

        #endregion
    }
}