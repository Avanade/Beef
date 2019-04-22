// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using Remotion.Linq.Parsing;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Beef.Data.OData.Linq
{
    /// <summary>
    /// Represents the <b>OData</b> <see cref="ThrowingExpressionVisitor"/> visitor for parsing expressions.
    /// </summary>
    internal class ODataQueryExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private ODataArgs _args;
        private StringBuilder _text = new StringBuilder();
        private Stack<BinaryExpression> _binaryStack = new Stack<BinaryExpression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryExpressionTreeVisitor"/> class.
        /// </summary>
        /// <param name="args">The <see cref="ODataArgs"/>.</param>
        public ODataQueryExpressionTreeVisitor(ODataArgs args)
        {
            _args = args;
        }

        /// <summary>
        /// Return the expression string representation.
        /// </summary>
        /// <returns>The expression string representation.</returns>
        public override string ToString()
        {
            return _text.ToString();
        }

        /// <summary>
        /// Overrides the <see cref="ThrowingExpressionVisitor.CreateUnhandledItemException{T}(T, string)"/>.
        /// </summary>
        /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
        /// <param name="unhandledItem">The unhandled item instance.</param>
        /// <param name="visitMethod">The vistit method.</param>
        /// <returns>The <see cref="Exception"/> to throw.</returns>
        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotSupportedException($"Expression '{unhandledItem.ToString()}' is not supported; Visit '{visitMethod}' not implemented.");
        }

        /// <summary>
        /// Overrides the <see cref="ThrowingExpressionVisitor.VisitBinary(System.Linq.Expressions.BinaryExpression)"/>.
        /// </summary>
        /// <param name="expression">The <see cref="BinaryExpression"/>.</param>
        /// <returns>The <see cref="Expression"/>.</returns>
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _text.Append("(");

            _binaryStack.Push(expression);
            Visit(expression.Left);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    _text.Append(" eq ");
                    break;

                case ExpressionType.NotEqual:
                    _text.Append(" ne ");
                    break;

                case ExpressionType.GreaterThan:
                    _text.Append(" gt ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _text.Append(" ge ");
                    break;

                case ExpressionType.LessThan:
                    _text.Append(" lt ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _text.Append(" le ");
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    _text.Append(" and ");
                    break;

                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    _text.Append(" or ");
                    break;

                case ExpressionType.Not:
                    _text.Append(" not ");
                    break;

                default:
                    base.VisitBinary(expression);
                    break;
            }

            Visit(expression.Right);
            _binaryStack.Pop();

            _text.Append(")");

            return expression;
        }

        /// <summary>
        /// Overrides the <see cref="ThrowingExpressionVisitor.VisitMember(MemberExpression)"/>.
        /// </summary>
        /// <param name="expression">The <see cref="BinaryExpression"/>.</param>
        /// <returns>The <see cref="Expression"/>.</returns>
        protected override Expression VisitMember(MemberExpression expression)
        {
            var pm = _args.Mapper.GetBySrcePropertyName(expression.Member.Name);
            if (pm == null)
                throw new InvalidOperationException($"Member '{expression.Member.Name}' has no corresponding OData mapping ({expression.ToString()}).");

            _text.Append(pm.DestPropertyName);
            return expression;
        }

        /// <summary>
        /// Overrides the <see cref="ThrowingExpressionVisitor.VisitNew(System.Linq.Expressions.NewExpression)"/>.
        /// </summary>
        /// <param name="expression">The <see cref="NewExpression"/>.</param>
        /// <returns>The <see cref="Expression"/>.</returns>
        protected override Expression VisitNew(NewExpression expression)
        {
            // Create an automapper for the new type so it can be deserialized later.
            if (!ODataAutoMapper.IsTypeCached(expression.Type))
            {
                var altProps = new Dictionary<string, MemberInfo>();
                for (int i = 0; i < expression.Arguments.Count; i++)
                {
                    if (!(expression.Arguments[i] is MemberExpression me))
                        throw new InvalidOperationException($"NewExpression has Argument ({expression.Arguments[i].ToString()}) that is not of Type MemberExpression ({expression.ToString()})");

                    altProps.Add(expression.Members[i].Name, me.Member);
                }

                ODataAutoMapper.GetMapper(expression.Type, altProps);
            }

            // Get on with the expression output :-)
            for (int i = 0; i < expression.Arguments.Count; i++)
            {
                if (i > 0)
                    _text.Append(",");

                Visit(expression.Arguments[i]);
            }

            return expression;
        }

        /// <summary>
        /// Overrides the <see cref="ThrowingExpressionVisitor.VisitConstant(ConstantExpression)"/>.
        /// </summary>
        /// <param name="expression">The <see cref="BinaryExpression"/>.</param>
        /// <returns>The <see cref="Expression"/>.</returns>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression.Value == null)
                _text.Append("null");
            else if (expression.Type == typeof(string))
                _text.Append($"'{expression.Value}'");
            else if (expression.Type == typeof(DateTime) || expression.Type == typeof(DateTime?))
                _text.Append(((DateTime)expression.Value).ToString("o"));
            else if (expression.Type == typeof(Guid) || expression.Type == typeof(Guid?))
                _text.Append($"guid'{expression.Value}'");
            else if (expression.Type.IsSubclassOf(typeof(ReferenceDataBase)))
            {
                // ReferenceData values need a Converter to determine their value; otherwise, asssume ReferenceData.Code.
                var binEx = _binaryStack.Peek();
                MemberExpression memEx = null;
                if (binEx != null)
                    memEx = (binEx.Left == expression ? binEx.Right : binEx.Left) as MemberExpression;

                if (memEx == null)
                    throw new InvalidOperationException($"ConstantExpression value Type '{expression.Type.Name}' is a sub class of ReferenceDataBase; need access to a corresponding MemberExpression to get/use the value converter.");

                var pm = _args.Mapper.GetBySrcePropertyName(memEx.Member.Name);
                if (pm == null)
                    throw new InvalidOperationException($"Member '{memEx.Member.Name}' has no corresponding OData mapping ({expression.ToString()}).");

                var val = (ReferenceDataBase)expression.Value;
                if (pm.Converter == null)
                    _text.Append($"'{val.Code}'");
                else if (pm.Converter.DestType == typeof(string))
                    _text.Append($"'{pm.Converter.ConvertToDest(val)}'");
                else
                    _text.Append(pm.Converter.ConvertToDest(val));
            }
            else
                _text.Append(expression.Value);

            return expression;
        }

        /// <summary>
        /// Overrides the <see cref="ThrowingExpressionVisitor.VisitMethodCall(MethodCallExpression)"/>. 
        /// </summary>
        /// <param name="expression">The <see cref="MethodCallExpression"/>.</param>
        /// <returns>The <see cref="Expression"/>.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            var name = expression.Method.Name.ToLower();
            switch (name)
            {
                case "toupper":
                case "tolower":
                    _text.Append(name);
                    _text.Append("(");
                    Visit(expression.Object);
                    _text.Append(")");
                    return expression;

                case "startswith":
                case "endswith":
                case "contains":
                    _text.Append(name);
                    _text.Append("(");
                    Visit(expression.Object);
                    _text.Append(",");
                    Visit(expression.Arguments[0]);
                    _text.Append(")");
                    return expression;
            }

            return base.VisitMethodCall(expression);
        }
    }
}
