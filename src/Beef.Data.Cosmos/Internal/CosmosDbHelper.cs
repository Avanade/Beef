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
        internal static IQueryable<T> AddTypeWhereClause<T>(IQueryable<T> q)
        {
            var type = typeof(T);
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(CosmosDbTypeValue<>))
                return q;

            q = q.Where("type = @0", type.GenericTypeArguments[0].Name);
            return q;
        }
    }
}