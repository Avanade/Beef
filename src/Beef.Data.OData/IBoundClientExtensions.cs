using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Beef.Data.OData
{
    /// <summary>
    /// Adds additional extension methods to the <see cref="IBoundClient{T}"/>.
    /// </summary>
    public static class IBoundClientExtensions
    {
        /// <summary>
        /// Filters a sequence of values based on a <paramref name="predicate"/> only <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="when">Indicates to perform an underlying <see cref="IFluentClient{TElement, FT}.Filter(Expression{Func{TElement, bool}})"/> only when <c>true</c>;
        /// otherwise, no <b>Where</b> is invoked.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The resulting query.</returns>
        public static IBoundClient<TElement> FilterWhen<TElement>(this IBoundClient<TElement> query, bool when, Expression<Func<TElement, bool>> predicate) where TElement : class
        {
            var q = Check.NotNull(query, nameof(query));
            if (when)
                return q.Filter(predicate);
            else
                return q;
        }

        /// <summary>
        /// Filters a sequence of values based on a <paramref name="predicate"/> only when the <paramref name="with"/> is not the default value for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The with value <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="with">Indicates to perform an underlying <see cref="IFluentClient{TElement, FT}.Filter(Expression{Func{TElement, bool}})"/> only when the with is not the default
        /// value; otherwise, no <b>Where</b> is invoked.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The resulting query.</returns>
        public static IBoundClient<TElement> FilterWith<TElement, T>(this IBoundClient<TElement> query, T with, Expression<Func<TElement, bool>> predicate) where TElement : class
        {
            var q = Check.NotNull(query, nameof(query));
            if (Comparer<T>.Default.Compare(with, default!) != 0 && Comparer<T>.Default.Compare(with, default!) != 0)
            {
                if (!(with is string) && with is System.Collections.IEnumerable ie && !ie.GetEnumerator().MoveNext())
                    return q;

                return q.Filter(predicate);
            }
            else
                return q;
        }

        /// <summary>
        /// Filters a sequence of values using the specified <paramref name="property"/> and <paramref name="text"/> containing <see cref="Wildcard.MultiBasic"/> supported wildcards.
        /// </summary>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="property">The <see cref="MemberExpression"/>.</param>
        /// <param name="text">The text to query.</param>
        /// <param name="ignoreCase">Indicates whether the comparison should ignore case (default) or not; will use <see cref="string.ToUpper()"/> when selected for comparisons.</param>
        /// <param name="checkForNull">Indicates whether a null check should also be performed before the comparion occurs (defaults to <c>true</c>).</param>
        /// <returns>The resulting (updated) query.</returns>
        public static IBoundClient<TElement> FilterWildcard<TElement>(this IBoundClient<TElement> query, Expression<Func<TElement, string?>> property, string? text, bool ignoreCase = true, bool checkForNull = true) where TElement : class
        {
            var q = Check.NotNull(query, nameof(query));
            var p = Check.NotNull(property, nameof(property));

            // Check the expression.
            if (!(p.Body is MemberExpression me))
                throw new ArgumentException("Property expression must be of Type MemberExpression.", nameof(property));

            Expression exp = me;
            var wc = Wildcard.MultiBasic;
            var wr = wc.Parse(text).ThrowOnError();

            // Exit stage left where nothing to do.
            if (wr.Selection.HasFlag(WildcardSelection.None) || wr.Selection.HasFlag(WildcardSelection.Single))
                return query;

            var s = wr.GetTextWithoutWildcards();
            if (ignoreCase)
            {
                s = s?.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                exp = Expression.Call(me, typeof(string).GetMethod("ToUpper", System.Type.EmptyTypes));
            }

            if (wr.Selection.HasFlag(WildcardSelection.Equal))
                exp = Expression.Equal(exp, Expression.Constant(s));
            else if (wr.Selection.HasFlag(WildcardSelection.EndsWith))
                exp = Expression.Call(exp, "EndsWith", null, Expression.Constant(s));
            else if (wr.Selection.HasFlag(WildcardSelection.StartsWith))
                exp = Expression.Call(exp, "StartsWith", null, Expression.Constant(s));
            else if (wr.Selection.HasFlag(WildcardSelection.Contains))
                exp = Expression.Call(exp, "Contains", null, Expression.Constant(s));
            else
                throw new ArgumentException("Wildcard selection text is not supported.", nameof(text));

            // Add check for not null.
            if (checkForNull)
            {
                var ee = Expression.NotEqual(me, Expression.Constant(null));
                exp = Expression.AndAlso(ee, exp);
            }

            var le = Expression.Lambda<Func<TElement, bool>>(exp, p.Parameters);
            return q.Filter(le);
        }
    }
}