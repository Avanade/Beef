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
        /// <typeparam name="T">The entity <see cref="System.Type"/>.</typeparam>
        /// <param name="q">The <see cref="IQueryable{T}"/>.</param>
        /// <param name="type">The type name.</param>
        internal static IQueryable<T> AddTypeWhereClause<T>(IQueryable<T> q, string type)
        {
            return q.Where("type = @0", type);
        }
    }
}