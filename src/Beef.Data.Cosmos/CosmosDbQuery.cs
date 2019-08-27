// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides common <b>CosmosDb/DocumentDb</b> query capabilities.
    /// </summary>
    public abstract class CosmosDbQueryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQueryBase"/> class.
        /// </summary>
        protected CosmosDbQueryBase() { }

        /// <summary>
        /// Gets a <see cref="PagingArgs"/> with a skip of 0 and top/take of 1.
        /// </summary>
        internal static PagingArgs PagingTop1 { get; } = PagingArgs.CreateSkipAndTake(0, 1);

        /// <summary>
        /// Gets a <see cref="PagingArgs"/> with a skip of 0 and top/take of 2.
        /// </summary>
        internal static PagingArgs PagingTop2 { get; } = PagingArgs.CreateSkipAndTake(0, 2);
    }

    /// <summary>
    /// Encapsulates a <b>CosmosDb/DocumentDb</b> query enabling all select-like capabilities.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    public class CosmosDbQuery<T> : CosmosDbQueryBase where T : class, new()
    {
        private readonly CosmosDbBase _db;
        private readonly Func<IQueryable<T>, IQueryable<T>> _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQuery{T}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="CosmosDbBase"/>.</param>
        /// <param name="queryArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        /// <param name="query">A function to modify the underlying <see cref="IQueryable{T}"/>.</param>
        internal CosmosDbQuery(CosmosDbBase db, CosmosDbArgs<T> queryArgs, Func<IQueryable<T>, IQueryable<T>> query = null)
        {
            _db = Check.NotNull(db, nameof(db));
            QueryArgs = Check.NotNull(queryArgs, nameof(queryArgs));
            _query = query;
        }

        /// <summary>
        /// Gets the <see cref="CosmosDbArgs{T}"/>.
        /// </summary>
        public CosmosDbArgs<T> QueryArgs { get; private set; }

        /// <summary>
        /// Manages the underlying query construction and lifetime.
        /// </summary>
        internal void ExecuteQuery(Action<IQueryable<T>> execute)
        {
            CosmosDbInvoker.Default.Invoke(this, () => ExecuteQueryInternal(execute), _db);
        }

        /// <summary>
        /// Actually manage the underlying query construction and lifetime.
        /// </summary>
        private IQueryable<T> ExecuteQueryInternal(Action<IQueryable<T>> execute)
        {
            IQueryable<T> q = _db.GetContainer(QueryArgs.ContainerId).GetItemLinqQueryable<T>(allowSynchronousQueryExecution: true, requestOptions: _db.GetQueryRequestOptions(QueryArgs));
            if (QueryArgs is ICosmosDbArgs qa && qa.IsTypeValue)
            {
                q = _query == null ?
                    Internal.CosmosDbHelper.AddTypeWhereClause<T>(q, qa.TypeValueType) :
                    _query(Internal.CosmosDbHelper.AddTypeWhereClause<T>(q, qa.TypeValueType));
            }
            else
                q = _query == null ? q : _query(q);

            execute?.Invoke(q);
            return q;
        }

        /// <summary>
        /// Manages the underlying query construction and lifetime.
        /// </summary>
        internal T ExecuteQuery(Func<IQueryable<T>, T> execute)
        {
            return CosmosDbInvoker.Default.Invoke(this, () =>
            {
                return execute(AsQueryable(false));
            }, _db);
        }

        /// <summary>
        /// Gets a prepared <see cref="IQueryable{T}"/> with any <see cref="CosmosDbTypeValue{T}"/> filtering as applicable.
        /// </summary>
        /// <remarks>The <see cref="ICosmosDbArgs.Paging"/> is not supported.</remarks>
        public IQueryable<T> AsQueryable()
        {
            return AsQueryable(true);
        }

        /// <summary>
        /// Initiate the IQueryable.
        /// </summary>
        private IQueryable<T> AsQueryable(bool checkPaging)
        {
            if (checkPaging && QueryArgs.Paging != null)
                throw new NotSupportedException("The QueryArgs.Paging must be null for an AsQueryable(); this is a limitation of the Microsoft.Azure.Cosmos SDK in that the paging must be applied last, as such use the IQueryable.Paging provided to perform where appropriate.");

            return ExecuteQueryInternal(null);
        }

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <returns>The single item.</returns>
        public T SelectSingle()
        {
            return ExecuteQuery(q =>
            {
                q = q.Paging(0, 2);
                return CosmosDbBase.GetAndFormatValue(q.AsEnumerable().Single());
            });
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T SelectSingleOrDefault()
        {
            return ExecuteQuery(q =>
            {
                q = q.Paging(0, 2);
                return CosmosDbBase.GetAndFormatValue(q.AsEnumerable().SingleOrDefault());
            });
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        public T SelectFirst()
        {
            return ExecuteQuery(q =>
            {
                q = q.Paging(0, 1);
                return CosmosDbBase.GetAndFormatValue(q.AsEnumerable().First());
            });
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T SelectFirstOrDefault()
        {
            return ExecuteQuery(q =>
            {
                q = q.Paging(0, 1);
                return CosmosDbBase.GetAndFormatValue(q.AsEnumerable().FirstOrDefault());
            });
        }

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        /// <remarks>The <see cref="QueryArgs"/> <see cref="ICosmosDbArgs.Paging"/> is also applied, including <see cref="PagingArgs.IsGetCount"/> where requested.</remarks>
        public TColl SelectQuery<TColl>() where TColl : ICollection<T>, new()
        {
            var coll = new TColl();
            SelectQuery(coll);
            return coll;
        }

        /// <summary>
        /// Executes the query adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <remarks>The <see cref="QueryArgs"/> <see cref="ICosmosDbArgs.Paging"/> is also applied, including <see cref="PagingArgs.IsGetCount"/> where requested.</remarks>
        public void SelectQuery<TColl>(TColl coll) where TColl : ICollection<T>
        {
            ExecuteQuery(query => 
            {
                foreach (var item in query.Paging(QueryArgs.Paging).AsEnumerable())
                {
                    coll.Add(CosmosDbBase.GetAndFormatValue(item));
                }

                if (QueryArgs.Paging != null && QueryArgs.Paging.IsGetCount)
                    QueryArgs.Paging.TotalCount = query.Count();
            });
        }

        #endregion
    }
}