// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.OData
{
    /// <summary>
    /// Encapsulates an OData query enabling all select-like capabilities.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
    public class ODataQuery<T, TModel> where T : class, new() where TModel : class, new()
    {
        private readonly ODataBase _odata;
        private readonly Func<IBoundClient<TModel>, IBoundClient<TModel>>? _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQuery{T, TModel}"/> class.
        /// </summary>
        /// <param name="odata">The <see cref="ODataBase"/>.</param>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="query">A function to modify the underlying <see cref="IQueryable{TModel}"/></param>
        internal ODataQuery(ODataBase odata, ODataArgs queryArgs, Func<IBoundClient<TModel>, IBoundClient<TModel>>? query = null)
        {
            _odata = Check.NotNull(odata, nameof(odata));
            QueryArgs = Check.NotNull(queryArgs, nameof(queryArgs));
            _query = query;
        }

        /// <summary>
        /// Gets the <see cref="ODataArgs"/>.
        /// </summary>
        public ODataArgs QueryArgs { get; private set; }

        /// <summary>
        /// Manages the underlying query construction and lifetime.
        /// </summary>
        private void ExecuteQuery(Action<IBoundClient<TModel>> execute)
        {
            _odata.Invoker.Invoke(this, () =>
            {
                var q = _odata.Client.For<TModel>(QueryArgs.CollectionName);
                execute((_query == null) ? q : _query(q));
            }, _odata);
        }

        /// <summary>
        /// Manages the underlying query construction and lifetime.
        /// </summary>
        private TModel ExecuteQuery(Func<IBoundClient<TModel>, TModel> execute)
        {
            return _odata.Invoker.Invoke(this, () =>
            {
                var q = _odata.Client.For<TModel>(QueryArgs.CollectionName);
                return execute((_query == null) ? q : _query(q));
            }, _odata);
        }

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <returns>The single item.</returns>
        public T SelectSingle()
        {
            return ODataBase.GetValue<T, TModel>(QueryArgs, ExecuteQuery(q =>
            {
                var coll = q.Skip(0).Top(2).FindEntriesAsync().GetAwaiter().GetResult();
                return coll.Single();
            }))!;
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T? SelectSingleOrDefault()
        {
            return ODataBase.GetValue<T, TModel>(QueryArgs, ExecuteQuery(q =>
            {
                var coll = q.Skip(0).Top(2).FindEntriesAsync().GetAwaiter().GetResult();
                return coll.SingleOrDefault();
            }));
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        public T SelectFirst()
        {
            return ODataBase.GetValue<T, TModel>(QueryArgs, ExecuteQuery(q =>
            {
                var coll = q.Skip(0).Top(1).FindEntriesAsync().GetAwaiter().GetResult();
                return coll.First();
            }))!;
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T? SelectFirstOrDefault()
        {
            return ODataBase.GetValue<T, TModel>(QueryArgs, ExecuteQuery(q =>
            {
                var coll = q.Skip(0).Top(1).FindEntriesAsync().GetAwaiter().GetResult();
                return coll.FirstOrDefault();
            }));
        }

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        /// <remarks>The <see cref="QueryArgs"/> <see cref="ODataArgs.Paging"/> is also applied, including <see cref="PagingArgs.IsGetCount"/> where requested.</remarks>
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
        /// <remarks>The <see cref="QueryArgs"/> <see cref="ODataArgs.Paging"/> is also applied, including <see cref="PagingArgs.IsGetCount"/> where requested.</remarks>
        public void SelectQuery<TColl>(TColl coll) where TColl : ICollection<T>
        {
            ExecuteQuery(q =>
            {
                ODataFeedAnnotations ann = null!;

                if (QueryArgs.Paging != null)
                {
                    q = q.Skip(QueryArgs.Paging.Skip).Top(QueryArgs.Paging.Take);
                    if (QueryArgs.Paging.IsGetCount && _odata.IsPagingGetCountSupported)
                        ann = new ODataFeedAnnotations();
                }

                foreach (var item in q.FindEntriesAsync(ann).GetAwaiter().GetResult())
                {
                    coll.Add(ODataBase.GetValue<T, TModel>(QueryArgs, item)!);
                }

                if (ann != null)
                    QueryArgs.Paging!.TotalCount = ann.Count;
            });
        }

        #endregion
    }
}