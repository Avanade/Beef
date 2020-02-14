// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Encapsulates a SQL query enabling all select-like capabilities.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    public class DatabaseQuery<T> where T : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseQuery{T}"/> class.
        /// </summary>
        /// <param name="database">The <see cref="DatabaseBase"/>.</param>
        /// <param name="queryArgs">The <see cref="IDatabaseArgs"/>.</param>
        /// <param name="queryParams">The query <see cref="DatabaseParameters"/> action/delegate.</param>
        internal DatabaseQuery(DatabaseBase database, DatabaseArgs<T> queryArgs, Action<DatabaseParameters>? queryParams)
        {
            var db = database ?? throw new ArgumentNullException(nameof(database));
            QueryArgs = queryArgs ?? throw new ArgumentNullException(nameof(queryArgs));
            Command = db.StoredProcedure(QueryArgs.StoredProcedure);
            if (queryParams != null)
                Command.Params(queryParams);
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseCommand"/>.
        /// </summary>
        private DatabaseCommand Command { get; set; }

        /// <summary>
        /// Gets the <see cref="DatabaseArgs{T}"/>.
        /// </summary>
        public DatabaseArgs<T> QueryArgs { get; private set; }

        /// <summary>
        /// Sets the paging from the <see cref="PagingArgs"/>.
        /// </summary>
        private void SetPaging(PagingArgs paging)
        {
            Command.Parameters.AddPagingParameters(paging);
        }

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <returns>The single item.</returns>
        public Task<T> SelectSingleAsync() 
        {
            return Command.SelectSingleAsync(QueryArgs.Mapper);
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public Task<T> SelectSingleOrDefaultAsync()
        {
            return Command.SelectSingleOrDefaultAsync(QueryArgs.Mapper);
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        public Task<T> SelectFirstAsync()
        {
            return Command.SelectFirstAsync(QueryArgs.Mapper);
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public Task<T> SelectFirstOrDefaultAsync() 
        {
            return Command.SelectFirstOrDefaultAsync(QueryArgs.Mapper);
        }

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        public async Task<TColl> SelectQueryAsync<TColl>() where TColl : ICollection<T>, new()
        {
            if (QueryArgs.Paging != null)
            {
                SetPaging(QueryArgs.Paging);
                var (coll, returnValue) = await Command.SelectQueryWithValueAsync<TColl, T>(QueryArgs.Mapper).ConfigureAwait(false);
                if (QueryArgs.Paging.IsGetCount && returnValue >= 0)
                    QueryArgs.Paging.TotalCount = returnValue;

                return coll;
            }

            return await Command.SelectQueryAsync<TColl, T>(QueryArgs.Mapper).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        public async Task SelectQueryAsync<TColl>(TColl coll) where TColl : ICollection<T>
        {
            if (QueryArgs.Paging != null)
            {
                SetPaging(QueryArgs.Paging);
                var returnValue = await Command.SelectQueryWithValueAsync(coll, QueryArgs.Mapper).ConfigureAwait(false);
                if (QueryArgs.Paging.IsGetCount && returnValue >= 0)
                    QueryArgs.Paging.TotalCount = returnValue;

                return;
            }

            await Command.SelectQueryAsync(coll, QueryArgs.Mapper).ConfigureAwait(false);
        }

        #endregion
    }
}