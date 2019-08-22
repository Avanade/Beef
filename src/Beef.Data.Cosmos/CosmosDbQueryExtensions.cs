// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Adds additional extension methods to the <see cref="CosmosDbQuery{T}"/>.
    /// </summary>
    public static class CosmosDbQueryExtensions
    {
        /// <summary>
        /// Executes the query creating a resultant collection for the underlying <see cref="CosmosDbTypeValue{T}.Value"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="CosmosDbTypeValue{T}.Value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TColl">THe collection <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <returns>A resultant collection.</returns>
        public static TColl SelectValueQuery<T, TColl>(this CosmosDbQuery<CosmosDbTypeValue<T>> query) where T : class, IIdentifier where TColl : ICollection<T>, new()
        {
            var coll = new TColl();
            SelectValueQuery(query, coll);
            return coll;
        }

        /// <summary>
        /// Executes the query adding to the passed collection for the underlying <see cref="CosmosDbTypeValue{T}.Value"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="CosmosDbTypeValue{T}.Value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TColl">THe collection <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="coll">The collection to add items to.</param>
        public static void SelectValueQuery<T, TColl>(this CosmosDbQuery<CosmosDbTypeValue<T>> query, TColl coll) where T : class, IIdentifier where TColl : ICollection<T>
        {
            query.ExecuteQuery(q =>
            {
                foreach (var item in query.SetPaging(q, query.QueryArgs.Paging).AsEnumerable())
                {
                    coll.Add(CosmosDbBase.GetAndFormatValue(item).Value);
                }
            });
        }
    }
}