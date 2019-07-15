// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.DocumentDb
{
    /// <summary>
    /// Encapsulates a <b>DocumentDb/CosmosDb</b> query enabling all select-like capabilities.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    public class DocDbQuery<T> where T : class, new()
    {
        private static readonly PagingArgs _pagingTop1 = PagingArgs.CreateSkipAndTake(0, 1);
        private static readonly PagingArgs _pagingTop2 = PagingArgs.CreateSkipAndTake(0, 2);

        private readonly DocDbBase _db;
        private readonly Func<IOrderedQueryable<T>, IQueryable<T>> _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocDbQuery{T}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DocDbBase"/>.</param>
        /// <param name="queryArgs">The <see cref="DocDbArgs"/>.</param>
        /// <param name="query">A function to modify the underlying <see cref="IQueryable{T}"/>.</param>
        internal DocDbQuery(DocDbBase db, DocDbArgs queryArgs, Func<IOrderedQueryable<T>, IQueryable<T>> query = null)
        {
            _db = Check.NotNull(db, nameof(db));
            QueryArgs = Check.NotNull(queryArgs, nameof(queryArgs));
            _query = query;
        }

        /// <summary>
        /// Gets the <see cref="DocDbArgs"/>.
        /// </summary>
        public DocDbArgs QueryArgs { get; private set; }

        /// <summary>
        /// Manages the DbContext and underlying query construction and lifetime.
        /// </summary>
        private void ExecuteQuery(Action<IQueryable<T>> execute)
        {
            DocDbInvoker.Default.Invoke(this, () =>
            {
                var q = _db.Client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_db.DatabaseId, QueryArgs.CollectionId), _db.GetFeedOptions(QueryArgs));
                execute(_query(q));
            }, _db);
        }

        /// <summary>
        /// Manages the DbContext and underlying query construction and lifetime.
        /// </summary>
        private T ExecuteQuery(Func<IQueryable<T>, T> execute)
        {
            return DocDbInvoker.Default.Invoke(this, () =>
            {
                var q = _db.Client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(_db.DatabaseId, QueryArgs.CollectionId), _db.GetFeedOptions(QueryArgs));
                return execute(_query(q));
            }, _db);
        }

        /// <summary>
        /// Sets the paging from the <see cref="PagingArgs"/>.
        /// </summary>
        private IQueryable<T> SetPaging(IQueryable<T> query, PagingArgs paging)
        {
            var q = query;
            if (paging != null && paging.Skip > 0)
                q = q.Skip((int)paging.Skip);

            return q.Take((int)(paging == null ? PagingArgs.DefaultTake : paging.Take));
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
                q = SetPaging(q, _pagingTop2);
                return q.ToArray().Single();
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
                q = SetPaging(q, _pagingTop2);
                return q.ToArray().SingleOrDefault();
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
                q = SetPaging(q, _pagingTop1);
                return q.ToArray().First();
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
                q = SetPaging(q, _pagingTop1);
                return q.ToArray().FirstOrDefault();
            });
        }

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        public TColl SelectQuery<TColl>() where TColl : ICollection<T>, new()
        {
            var coll = new TColl();
            SelectQuery(coll);
            return coll;
        }

        /// <summary>
        /// Executes a query adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        public void SelectQuery<TColl>(TColl coll) where TColl : ICollection<T>
        {
            ExecuteQuery(query =>
            {
                foreach (var item in SetPaging(query, QueryArgs.Paging).ToArray())
                { 
                    coll.Add(_db.ETagReformatter(item));
                }

                if (QueryArgs.Paging != null && QueryArgs.Paging.IsGetCount)
                    QueryArgs.Paging.TotalCount = query.Count();
            });
        }

        #endregion
    }
}