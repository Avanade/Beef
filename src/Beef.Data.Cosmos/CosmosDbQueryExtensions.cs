// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Adds additional extension methods to the <see cref="CosmosDbQuery{T, TModel}"/>.
    /// </summary>
    public static class CosmosDbQueryExtensions
    {
        // TODO: COME BACK HERE!
        ///// <summary>
        ///// Executes the query creating a resultant collection for the underlying <see cref="CosmosDbValue{T}.Value"/>.
        ///// </summary>
        ///// <typeparam name="T">The <see cref="CosmosDbValue{T}.Value"/> <see cref="Type"/>.</typeparam>
        ///// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        ///// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        ///// <param name="query">The query.</param>
        ///// <returns>A resultant collection.</returns>
        //public static TColl SelectValueQuery<T, TColl, TModel>(this CosmosDbQuery<CosmosDbValue<T>, TModel> query) where T : class, IIdentifier where TColl : ICollection<T>, new() where TModel : class, new()
        //{
        //    var coll = new TColl();
        //    SelectValueQuery(query, coll);
        //    return coll;
        //}

        ///// <summary>
        ///// Executes the query adding to the passed collection for the underlying <see cref="CosmosDbValue{T}.Value"/>.
        ///// </summary>
        ///// <typeparam name="T">The <see cref="CosmosDbValue{T}.Value"/> <see cref="Type"/>.</typeparam>
        ///// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        ///// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        ///// <param name="query">The query.</param>
        ///// <param name="coll">The collection to add items to.</param>
        //public static void SelectValueQuery<T, TColl, TModel>(this CosmosDbQuery<CosmosDbValue<T>, TModel> query, TColl coll) where T : class, IIdentifier where TColl : ICollection<T> where TModel : class, new()
        //{
        //    query.ExecuteQuery(q =>
        //    {
        //        foreach (var item in q.Paging(query.QueryArgs.Paging).AsEnumerable())
        //        {
        //            coll.Add(CosmosDbBase.GetAndFormatValue(query.QueryArgs, item).Value);
        //        }
        //    });
        //}

        /// <summary>
        /// Adds paging to the query.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> being queried.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The query.</returns>
        public static IQueryable<T> Paging<T>(this IQueryable<T> query, PagingArgs? paging)
        {
            return paging == null ? query.Paging(0, null) : query.Paging(paging.Skip, paging.Take);
        }

        /// <summary>
        /// Adds paging to the query using the specified <paramref name="skip"/> and <paramref name="take"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> being queried.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="skip">The specified number of elements in a sequence to bypass.</param>
        /// <param name="take">The specified number of contiguous elements from the start of a sequence.</param>
        /// <returns>The query.</returns>
        public static IQueryable<T> Paging<T>(this IQueryable<T> query, long skip, long? take = null)
        {
            var q = query.Skip(skip <= 0 ? 0 :(int)skip);
            q = q.Take(take == null || take.Value < 1 ? (int)PagingArgs.DefaultTake : (int)take.Value);
            return q;
        }

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> being queried.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The single item.</returns>
        public static T SelectSingle<T>(this IQueryable<T> query)
        {
            return query.Paging(0, 2).AsEnumerable().Single();
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> being queried.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The single item or default.</returns>
        public static T SelectSingleOrDefault<T>(this IQueryable<T> query)
        {
            return query.Paging(0, 2).AsEnumerable().SingleOrDefault();
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> being queried.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The first item.</returns>
        public static T SelectFirst<T>(this IQueryable<T> query)
        {
            return query.Paging(0, 1).AsEnumerable().First();
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> being queried.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>The single item or default.</returns>
        public static T SelectFirstOrDefault<T>(this IQueryable<T> query)
        {
            return query.Paging(0, 1).AsEnumerable().FirstOrDefault();
        }
    }
}