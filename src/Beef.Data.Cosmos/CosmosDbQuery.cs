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
    /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
    public class CosmosDbQuery<T, TModel> : CosmosDbQueryBase where T : class, new() where TModel : class, new()
    {
        private readonly CosmosDbContainer<T, TModel> _container;
        private readonly Func<IQueryable<TModel>, IQueryable<TModel>>? _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbQuery{T, TModel}"/> class.
        /// </summary>
        /// <param name="container">The <see cref="CosmosDbContainer{T, TModel}"/>.</param>
        /// <param name="query">A function to modify the underlying <see cref="IQueryable{T}"/>.</param>
        internal CosmosDbQuery(CosmosDbContainer<T, TModel> container, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
        {
            _container = Check.NotNull(container, nameof(container));
            _query = query;
        }

        /// <summary>
        /// Gets the <see cref="CosmosDbArgs"/>.
        /// </summary>
        public CosmosDbArgs QueryArgs => _container.DbArgs;

        /// <summary>
        /// Manages the underlying query construction and lifetime.
        /// </summary>
        internal void ExecuteQuery(Action<IQueryable<TModel>> execute) 
            => _container.CosmosDb.Invoker.Invoke(this, () => ExecuteQueryInternal(execute), _container.CosmosDb);

        /// <summary>
        /// Actually manage the underlying query construction and lifetime.
        /// </summary>
        private IQueryable<TModel> ExecuteQueryInternal(Action<IQueryable<TModel>>? execute)
        {
            IQueryable<TModel> q = _container.Container.GetItemLinqQueryable<TModel>(allowSynchronousQueryExecution: true, requestOptions: _container.CosmosDb.GetQueryRequestOptions<T, TModel>(QueryArgs));
            q = _query == null ? q : _query(q);

            if (QueryArgs.AuthorizeFilter != null)
                q = (IQueryable<TModel>)QueryArgs.AuthorizeFilter(q);
            else
            {
                var filter = _container.CosmosDb.GetAuthorizeFilter<TModel>(_container.Container.Id);
                if (filter != null)
                    q = (IQueryable<TModel>)filter(q);
            }

            execute?.Invoke(q);
            return q;
        }

        /// <summary>
        /// Manages the underlying query construction and lifetime.
        /// </summary>
        internal TModel ExecuteQuery(Func<IQueryable<TModel>, TModel> execute)
        {
            return _container.CosmosDb.Invoker.Invoke(this, () =>
            {
                return execute(AsQueryable(false));
            }, _container.CosmosDb);
        }

        /// <summary>
        /// Gets a prepared <see cref="IQueryable{TModel}"/> with any <see cref="CosmosDbValue{TModel}"/> filtering as applicable.
        /// </summary>
        /// <remarks>The <see cref="CosmosDbArgs.Paging"/> is not supported.</remarks>
        public IQueryable<TModel> AsQueryable() => AsQueryable(true);

        /// <summary>
        /// Initiate the IQueryable.
        /// </summary>
        private IQueryable<TModel> AsQueryable(bool checkPaging)
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
            return _container.GetValue(ExecuteQuery(q =>
            {
                q = q.Paging(0, 2);
                return q.AsEnumerable().Single();
            }));
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T? SelectSingleOrDefault()
        {
            return _container.GetValue(ExecuteQuery(q =>
            {
                q = q.Paging(0, 2);
                return q.AsEnumerable().SingleOrDefault();
            }));
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        public T SelectFirst()
        {
            return _container.GetValue(ExecuteQuery(q =>
            {
                q = q.Paging(0, 1);
                return q.AsEnumerable().First();
            }));
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T? SelectFirstOrDefault()
        {
            return _container.GetValue(ExecuteQuery(q =>
            {
                q = q.Paging(0, 1);
                return q.AsEnumerable().FirstOrDefault();
            }));
        }

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        /// <remarks>The <see cref="QueryArgs"/> <see cref="CosmosDbArgs.Paging"/> is also applied, including <see cref="PagingArgs.IsGetCount"/> where requested.</remarks>
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
        /// <remarks>The <see cref="QueryArgs"/> <see cref="CosmosDbArgs.Paging"/> is also applied, including <see cref="PagingArgs.IsGetCount"/> where requested.</remarks>
        public void SelectQuery<TColl>(TColl coll) where TColl : ICollection<T>
        {
            ExecuteQuery(query => 
            {
                foreach (var item in query.Paging(QueryArgs.Paging).AsEnumerable())
                {
                    coll.Add(_container.GetValue(item));
                }

                if (QueryArgs.Paging != null && QueryArgs.Paging.IsGetCount)
                    QueryArgs.Paging.TotalCount = query.Count();
            });
        }

        #endregion
    }
}