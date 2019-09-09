// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Linq;
using System.Linq.Dynamic.Core;

namespace Beef.Data.Cosmos.Internal
{
    /// <summary>
    /// Helper methods.
    /// </summary>
    internal static class CosmosDbHelper
    {
        /// <summary>
        /// Adds a <b>where</b> clause for `type` where T is CosmosDbValue.
        /// </summary>
        /// <typeparam name="TModel">The model <see cref="System.Type"/>.</typeparam>
        /// <param name="q">The <see cref="IQueryable{T}"/>.</param>
        /// <param name="type">The type name.</param>
        internal static IQueryable<CosmosDbValue<TModel>> AddTypeWhereClause<TModel>(IQueryable<CosmosDbValue<TModel>> q, string type) where TModel : class, new()
        {
            return q.Where("type = @0", type);
        }
    }
}