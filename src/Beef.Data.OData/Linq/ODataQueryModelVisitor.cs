// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Remotion.Linq;
using System;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Beef.Data.OData.Linq
{
    /// <summary>
    /// Represents the <b>OData</b> <see cref="QueryModel"/> visitor for parsing the query components.
    /// </summary>
    internal class ODataQueryModelVisitor : QueryModelVisitorBase
    {
        private ODataArgs _args;
        private ODataQueryAggregator _aggregator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryModelVisitor"/> class.
        /// </summary>
        /// <param name="args">The <see cref="ODataArgs"/>.</param>
        public ODataQueryModelVisitor(ODataArgs args)
        {
            _args = args;
            _aggregator = new ODataQueryAggregator(_args);
        }

        /// <summary>
        /// Gets the <see cref="ODataQueryAggregator"/>.
        /// </summary>
        /// <returns>The <see cref="ODataQueryAggregator"/>.</returns>
        public ODataQueryAggregator GetQueryAggregator()
        {
            return _aggregator;
        }

        /// <summary>
        /// Gets the OData expression snippit for an <see cref="Expression"/>.
        /// </summary>
        private string GetODataExpression(Expression expression)
        {
            var etv = new ODataQueryExpressionTreeVisitor(_args);
            etv.Visit(expression);
            return etv.ToString();
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitQueryModel(QueryModel)"/>.
        /// </summary>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitWhereClause(WhereClause, QueryModel, int)"/>.
        /// </summary>
        /// <param name="whereClause">The <see cref="WhereClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            _aggregator.Where.Add(GetODataExpression(whereClause.Predicate));
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitMainFromClause(MainFromClause, QueryModel)"/>.
        /// </summary>
        /// <param name="fromClause">The <see cref="MainFromClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            base.VisitMainFromClause(fromClause, queryModel);
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitResultOperator(ResultOperatorBase, QueryModel, int)"/>.
        /// </summary>
        /// <param name="orderByClause">The <see cref="OrderByClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            foreach (var o in orderByClause.Orderings)
            {
                var s = GetODataExpression(o.Expression);
                _aggregator.OrderBy.Add(o.OrderingDirection == OrderingDirection.Asc ? s : s + " desc");
            }

            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitOrdering(Ordering, QueryModel, OrderByClause, int)"/>.
        /// </summary>
        /// <param name="ordering">The <see cref="Ordering"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="orderByClause">The <see cref="OrderByClause"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
        {
            base.VisitOrdering(ordering, queryModel, orderByClause, index);
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitSelectClause(SelectClause, QueryModel)"/>.
        /// </summary>
        /// <param name="selectClause">The <see cref="SelectClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            _aggregator.SelectClause = selectClause;
            if (selectClause.Selector.NodeType == ExpressionType.Extension)
                _aggregator.Select.AddRange(_args.Mapper.GetODataFieldNames());
            else
                _aggregator.Select.Add(GetODataExpression(selectClause.Selector));

            base.VisitSelectClause(selectClause, queryModel);
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitAdditionalFromClause(AdditionalFromClause, QueryModel, int)"/>.
        /// </summary>
        /// <param name="fromClause">The <see cref="AdditionalFromClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            throw new NotSupportedException($"Additional From clause is not supported: {fromClause.ToString()}.");
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitGroupJoinClause(GroupJoinClause, QueryModel, int)"/>.
        /// </summary>
        /// <param name="groupJoinClause">The <see cref="GroupJoinClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            throw new NotSupportedException($"Group Join clause is not supported: {groupJoinClause.ToString()}.");
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitJoinClause(JoinClause, QueryModel, GroupJoinClause)"/>.
        /// </summary>
        /// <param name="joinClause">The <see cref="JoinClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="groupJoinClause">The <see cref="GroupJoinClause"/>.</param>
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            throw new NotSupportedException($"Join clause is not supported: {joinClause.ToString()}.");
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitJoinClause(JoinClause, QueryModel, int)"/>.
        /// </summary>
        /// <param name="joinClause">The <see cref="JoinClause"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            throw new NotSupportedException($"Join clause is not supported: {joinClause.ToString()}.");
        }

        /// <summary>
        /// Overrides the <see cref="QueryModelVisitorBase.VisitResultOperator(ResultOperatorBase, QueryModel, int)"/>.
        /// </summary>
        /// <param name="resultOperator">The <see cref="ResultOperatorBase"/>.</param>
        /// <param name="queryModel">The <see cref="QueryModel"/>.</param>
        /// <param name="index">The index.</param>
        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            if (resultOperator is FirstResultOperator fro)
            {
                _aggregator.Paging.OverrideTake(1);
                return;
            }

            if (resultOperator is TakeResultOperator tro)
            {
                _aggregator.Paging.OverrideTake(tro.GetConstantCount());
                return;
            }

            if (resultOperator is SkipResultOperator sro)
            {
                _aggregator.Paging.OverrideSkip(sro.GetConstantCount());
                return;
            }

            if (resultOperator is Remotion.Linq.Clauses.ResultOperators.FirstResultOperator)
                return;

            if (resultOperator is Remotion.Linq.Clauses.ResultOperators.SingleResultOperator)
                return;

            throw new NotSupportedException($"Result operator is not supported: {resultOperator.ToString()}.");
        }
    }
}
