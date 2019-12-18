// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.OData.Linq;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using System;
using System.Linq;
using System.Linq.Expressions;

/*
 * Inspiration and guidance: https://github.com/SharpRepository/SharpRepository/tree/odata/SharpRepository.ODataRepository
 */

namespace Beef.Data.OData
{
    /// <summary>
    /// Represents an <b>OData LINQ</b> query.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to query.</typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix; by-design, is an "Queryable".
    public class ODataQueryable<T> : QueryableBase<T>
#pragma warning restore CA1710
    {
        /// <summary>
        /// Creates a new <see cref="ODataQueryExecutor"/>.
        /// </summary>
        private static IQueryExecutor CreateExecutor(ODataBase odata, ODataArgs queryArgs, ref IQueryExecutor executor)
        {
            return executor = new ODataQueryExecutor(odata, queryArgs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryable{T}"/> class.
        /// </summary>
        /// <param name="odata">The <see cref="ODataBase"/>.</param>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="executor">The <see cref="IQueryExecutor"/>.</param>
        public ODataQueryable(ODataBase odata, ODataArgs queryArgs, ref IQueryExecutor executor)
            : base(QueryParser.CreateDefault(), CreateExecutor(odata, queryArgs, ref executor))
        {
            QueryExecutor = (ODataQueryExecutor)executor;
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="ODataQueryable{T}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IQueryProvider"/>.</param>
        /// <param name="expression">The <see cref="Expression"/>.</param>
        public ODataQueryable(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            QueryExecutor = (ODataQueryExecutor)((DefaultQueryProvider)provider).Executor;
        }

        /// <summary>
        /// Gets the <see cref="ODataQueryExecutor"/>.
        /// </summary>
        internal ODataQueryExecutor QueryExecutor { get; private set; }

        /// <summary>
        /// Gets the <b>OData</b> query URL.
        /// </summary>
        /// <param name="pagingTakeOverride">The optional paging take override value.</param>
        /// <returns>The URL string representation.</returns>
        public string GetODataQuery(int? pagingTakeOverride = null)
        {
            return QueryExecutor.GetQueryAggregator(this.Expression, pagingTakeOverride).ToString();
        }
    }
}