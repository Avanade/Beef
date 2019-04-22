// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Remotion.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Beef.Data.OData.Linq
{
    /// <summary>
    /// Represents the primary <b>OData</b> query executor.
    /// </summary>
    internal class ODataQueryExecutor : IQueryExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryExecutor"/> class for an <see cref="ODataBase"/>.
        /// </summary>
        /// <param name="odata">The <see cref="ODataBase"/>.</param>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        public ODataQueryExecutor(ODataBase odata, ODataArgs queryArgs)
        {
            OData = odata;
            QueryArgs = queryArgs;
        }

        /// <summary>
        /// Gets the <see cref="ODataBase"/>.
        /// </summary>
        public ODataBase OData { get; private set; }

        /// <summary>
        /// Gets the query <see cref="ODataArgs"/>.
        /// </summary>
        public ODataArgs QueryArgs { get; private set; }

        /// <summary>
        /// Executes a select returning one or more items.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <returns>The resultant items.</returns>
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel, null);
        }

        /// <summary>
        /// Executes a select returning one or more items.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="pagingTakeOverride">The optional paging take override value.</param>
        /// <returns>The resultant items.</returns>
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel, int? pagingTakeOverride)
        {
            var coll = new List<T>();
            Task.Run(async () => await OData.Query(QueryArgs, GetQueryAggregator(queryModel, pagingTakeOverride), coll)).Wait();
            return coll;
        }

        /// <summary>
        /// Executes a select scalar.
        /// </summary>
        /// <typeparam name="T">The scalar <see cref="Type"/>.</typeparam>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <returns>The scalar value.</returns>
        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a select single.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="returnDefaultWhenEmpty">Indicates whether to return the default value where nothing found.</param>
        /// <returns>The entity collection.</returns>
        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ? ExecuteCollection<T>(queryModel, 2).SingleOrDefault() : ExecuteCollection<T>(queryModel, 2).Single();
        }

        /// <summary>
        /// Gets the <see cref="ODataQueryAggregator"/> from the <see cref="QueryModel"/>.
        /// </summary>
        private ODataQueryAggregator GetQueryAggregator(QueryModel queryModel, int? pagingTakeOverride)
        {
            var visitor = new ODataQueryModelVisitor(QueryArgs);
            queryModel.Accept(visitor);

            var aggregator = visitor.GetQueryAggregator();
            if (pagingTakeOverride.HasValue)
                aggregator.Paging.OverrideTake(pagingTakeOverride.Value);

            return aggregator;
        }

        /// <summary>
        /// Gets the <see cref="ODataQueryAggregator"/> from the <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/>.</param>
        /// <param name="pagingTakeOverride">The optional paging take override value.</param>
        /// <returns>The <see cref="ODataQueryAggregator"/>.</returns>
        public ODataQueryAggregator GetQueryAggregator(Expression expression, int? pagingTakeOverride)
        {
            var queryParser = Remotion.Linq.Parsing.Structure.QueryParser.CreateDefault();
            var queryModel = queryParser.GetParsedQuery(expression);
            return GetQueryAggregator(queryModel, pagingTakeOverride);
        }
    }
}
