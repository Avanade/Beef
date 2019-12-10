// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Beef
{
    /// <summary>
    /// Adds additional extension methods to the <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Creates a collection from a <see cref="IQueryable{TItem}"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="query">The <see cref="IEnumerable{TItem}"/>.</param>
        /// <returns>A new collection that contains the elements from the input sequence.</returns>
        public static TColl ToCollection<TColl, TItem>(this IQueryable<TItem> query)
            where TColl : ICollection<TItem>, new()
        {
            var coll = new TColl();
            ToCollection(query, coll);
            return coll;
        }

        /// <summary>
        /// Creates a collection from a <see cref="IQueryable{TElement}"/> mapping each element to a corresponding item.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <param name="query">>The <see cref="IQueryable{TElement}"/>.</param>
        /// <param name="mapToItem">The mapping function invoked for each element.</param>
        /// <returns>A new collection that contains the elements from the input sequence.</returns>
        public static TColl ToCollection<TColl, TItem, TElement>(this IQueryable<TElement> query, Func<TElement, TItem> mapToItem)
            where TColl : ICollection<TItem>, new()
        {
            var coll = new TColl();
            ToCollection(query, mapToItem, coll);
            return coll;
        }

        /// <summary>
        /// Add to a collection from a <see cref="IQueryable{TItem}"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="query">The <see cref="IQueryable{TItem}"/>.</param>
        /// <param name="coll">The collection to add the elements from the input sequence.</param>
        public static void ToCollection<TColl, TItem>(this IQueryable<TItem> query, TColl coll)
            where TColl : ICollection<TItem>
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));

            foreach (var item in Check.NotNull(query, nameof(query)))
            {
                coll.Add(item);
            }
        }

        /// <summary>
        /// Add to a collection from a <see cref="IQueryable{TElement}"/> mapping each element to a corresponding item.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <param name="query">>The <see cref="IQueryable{TElement}"/>.</param>
        /// <param name="mapToItem">The mapping function invoked for each element.</param>
        /// <param name="coll">The collection to add the elements from the input sequence.</param>
        public static void ToCollection<TColl, TItem, TElement>(this IQueryable<TElement> query, Func<TElement, TItem> mapToItem, TColl coll)
            where TColl : ICollection<TItem>
        {
            if (mapToItem == null)
                throw new ArgumentNullException(nameof(mapToItem));

            if (coll == null)
                throw new ArgumentNullException(nameof(coll));

            foreach (var element in Check.NotNull(query, nameof(query)))
            {
                coll.Add(mapToItem(element));
            }
        }

        /// <summary>
        /// Filters a sequence of values based on a <paramref name="predicate"/> only <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="when">Indicates to perform an underlying <see cref="Queryable.Where{TElement}(IQueryable{TElement}, Expression{Func{TElement, bool}})"/> only when <c>true</c>;
        /// otherwise, no <b>Where</b> is invoked.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The resulting query.</returns>
        public static IQueryable<TElement> WhereWhen<TElement>(this IQueryable<TElement> query, bool when, Expression<Func<TElement, bool>> predicate)
        {
            Check.NotNull(query, nameof(query));
            if (when)
                return query.Where(predicate);
            else
                return query;
        }

        /// <summary>
        /// Filters a sequence of values based on a <paramref name="predicate"/> only when the <paramref name="with"/> is not the default value for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="TElement">The element <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The with value <see cref="Type"/>.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="with">Indicates to perform an underlying <see cref="Queryable.Where{TElement}(IQueryable{TElement}, Expression{Func{TElement, bool}})"/> only when the with is not the default
        /// value; otherwise, no <b>Where</b> is invoked.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>The resulting query.</returns>
        public static IQueryable<TElement> WhereWith<TElement, T>(this IQueryable<TElement> query, T with, Expression<Func<TElement, bool>> predicate)
        {
            Check.NotNull(query, nameof(query));
            if (Comparer<T>.Default.Compare(with, default) != 0 && Comparer<T>.Default.Compare(with, default) != 0)
            {
                if (!(with is string) && with is System.Collections.IEnumerable ie && !ie.GetEnumerator().MoveNext())
                    return query;

                return query.Where(predicate);
            }
            else
                return query;
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
        public static IQueryable<TElement> WhereWildcard<TElement>(this IQueryable<TElement> query, Expression<Func<TElement, string>> property, string text, bool ignoreCase = true, bool checkForNull = true)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(property, nameof(property));

            // Check the expression.
            if (!(property.Body is MemberExpression me))
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
                s = s.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
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

            var le = Expression.Lambda<Func<TElement, bool>>(exp, property.Parameters);
            return query.Where(le);
        }
    }
}