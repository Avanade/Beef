// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Enables all of the common <b>container</b> operations for a specified <see cref="CosmosDb"/> and <see cref="DbArgs"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CosmosDbContainer<T> where T : class, IIdentifier, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbContainer{T}"/> class.
        /// </summary>
        /// <param name="cosmosDb">The <see cref="CosmosDb"/>.</param>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        public CosmosDbContainer(CosmosDbBase cosmosDb, CosmosDbArgs<T> dbArgs)
        {
            CosmosDb = Check.NotNull(cosmosDb, nameof(cosmosDb));
            DbArgs = Check.NotNull(dbArgs, nameof(dbArgs));
            Container = cosmosDb.GetContainer(DbArgs.ContainerId);
        }

        /// <summary>
        /// Gets the owning <see cref="CosmosDbBase"/>.
        /// </summary>
        public CosmosDbBase CosmosDb { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="CosmosDbArgs{T}"/> (used for all operations).
        /// </summary>
        public CosmosDbArgs<T> DbArgs { get; private set; }

        private ICosmosDbArgs CosmosDbArgs => DbArgs;

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.Container"/>.
        /// </summary>
        public Container Container { get; private set; }

        #region Query

        /// <summary>
        /// Creates a <see cref="CosmosDbQuery{T}"/> and returns the corresponding <see cref="CosmosDbQuery{T}.AsQueryable()"/> to enable LINQ-style queries.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/>.</returns>
        /// <remarks>The <see cref="ICosmosDbArgs.Paging"/> is not supported.</remarks>
        public IQueryable<T> AsQueryable()
        {
            return new CosmosDbQuery<T>(CosmosDb, DbArgs).AsQueryable();
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
            if (keys.Length != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            return await CosmosDbInvoker.Default.InvokeAsync<T>(this, async () =>
            {
                try
                {
                    var val = await Container.ReadItemAsync<T>(keys[0].ToString(), DbArgs.PartitionKey, CosmosDb.GetItemRequestOptions(DbArgs));

                    // Check if T Type is CosmosDbValue<> and where Type is different it should be assumed that it does not exist.
                    if (CosmosDbArgs.IsTypeValue && val?.Resource is CosmosDbTypeValue ctv && ctv.Type != CosmosDbArgs.TypeValueType)
                        return null;

                    return CosmosDbBase.GetResponseValue(val);
                }
                catch (CosmosException dcex)
                {
                    if (DbArgs.NullOnNotFoundResponse && dcex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    throw;
                }
            }, CosmosDb);
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

            return await CosmosDbInvoker.Default.InvokeAsync<T>(this, async () =>
            {
                if (value is CosmosDbTypeValue ctv)
                {
                    CosmosDbBase.PrepareEntityForCreate(ctv.GetValue(), DbArgs.SetIdentifierOnCreate);
                    ctv.PrepareBefore(CosmosDbArgs.TypeValueType);
                }
                else
                    CosmosDbBase.PrepareEntityForCreate(value, DbArgs.SetIdentifierOnCreate);

                return CosmosDbBase.GetResponseValue(await Container.CreateItemAsync(value, DbArgs.PartitionKey, CosmosDb.GetItemRequestOptions(DbArgs)));
            }, CosmosDb);
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

            return await CosmosDbInvoker.Default.InvokeAsync<T>(this, async () =>
            {
                // Where supporting etag then use IfMatch for concurreny.
                var ro = CosmosDb.GetItemRequestOptions(DbArgs);
                if (ro.IfMatchEtag == null && value is IETag etag)
                    ro.IfMatchEtag = etag.ETag.StartsWith("\"") ? etag.ETag : "\"" + etag.ETag + "\"";

                string key;
                if (value is CosmosDbTypeValue ctv)
                {
                    ctv.PrepareBefore(CosmosDbArgs.TypeValueType);
                    var val = ctv.GetValue();
                    key = GetKeyFromValue(val);

                    // Must read existing and make sure we are updating for the correct Type; don't just trust the key.
                    var resp = await Container.ReadItemAsync<T>(key, DbArgs.PartitionKey, ro);
                    var orig = resp.Resource as CosmosDbTypeValue;
                    ro.SessionToken = resp.Headers?.Session;

                    if (orig.Type != CosmosDbArgs.TypeValueType)
                        throw new NotFoundException();

                    if (val is IChangeLog cl)
                        ChangeLogUpdate(cl, (IChangeLog)orig.GetValue());
                }
                else
                {
                    key = GetKeyFromValue(value);

                    if (value is IChangeLog cl)
                    {
                        // Must read existing where updating IChangeLog to get the Created* values.
                        var resp = await Container.ReadItemAsync<T>(key, DbArgs.PartitionKey, ro);
                        ChangeLogUpdate(cl, (IChangeLog)resp.Resource);
                        ro.SessionToken = resp.Headers?.Session;
                    }
                }

                // Replace/Update the value.
                return CosmosDbBase.GetResponseValue<T>(await Container.ReplaceItemAsync<T>(value, key, DbArgs.PartitionKey, ro));
            }, CosmosDb);
        }

        /// <summary>
        /// Get the key from the value.
        /// </summary>
        private string GetKeyFromValue(object value)
        {
            switch (value)
            {
                case IGuidIdentifier gid: return gid.Id.ToString();
                case IStringIdentifier isi: return isi.Id;
                case IIntIdentifier iid: return iid.Id.ToString();
                default: throw new InvalidOperationException("An Identifier cannot be inferred for this Type.");
            }
        }

        /// <summary>
        /// Update the change log; reset Created* to original.
        /// </summary>
        private void ChangeLogUpdate(IChangeLog cl, IChangeLog orig)
        {
            if (cl.ChangeLog == null)
                cl.ChangeLog = new ChangeLog();

            cl.ChangeLog.CreatedBy = orig.ChangeLog?.CreatedBy;
            cl.ChangeLog.CreatedDate = orig.ChangeLog?.CreatedDate;
            cl.ChangeLog.UpdatedBy = ExecutionContext.Current.Username;
            cl.ChangeLog.UpdatedDate = ExecutionContext.Current.Timestamp;
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
            if (keys.Length != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            // Where the T Type is CosmosDbValue<> do a get; which will confirm existence before proceeding - the GetAsync will check the Type property.
            if (CosmosDbArgs.IsTypeValue && await GetAsync(keys) == null)
                return;

            await CosmosDbInvoker.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    await Container.DeleteItemAsync<T>(keys[0].ToString(), DbArgs.PartitionKey, CosmosDb.GetItemRequestOptions(DbArgs));
                }
                catch (CosmosException cex)
                {
                    if (cex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return;

                    throw;
                }
            }, CosmosDb);
        }

        #endregion
    }
}