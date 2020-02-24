// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Extends <see cref="EfDbBase{TDbContext}"/> adding <see cref="Default"/> capabilities.
    /// </summary>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDefault">The <see cref="EfDbBase{TDbContext}"/> <see cref="Type"/>.</typeparam>
    public abstract class EfDb<TDbContext, TDefault> : EfDbBase<TDbContext> where TDefault : EfDb<TDbContext, TDefault> where TDbContext : DbContext, new()
    {
        private static readonly Lazy<TDefault> _default = new Lazy<TDefault>(true);

#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, is ok.
        /// <summary>
        /// Gets the current default <see cref="EfDbBase{TDbContext}"/> instance.
        /// </summary>
        public static TDefault Default { get => _default.Value; }

        /// <summary>
        /// Invokes the <paramref name="action"/> whilst <see cref="DatabaseWildcard.Replace(string)">replacing</see> the <b>wildcard</b> characters when the <paramref name="with"/> is not <c>null</c>.
        /// </summary>
        /// <param name="with">The value with which to verify.</param>
        /// <param name="action">The <see cref="Action"/> to invoke when there is a valid <paramref name="with"/> value; passed the database specific wildcard value.</param>
        public static void WithWildcard(string? with, Action<string> action)
        {
            if (with != null)
            {
                with = Default.Wildcard.Replace(with);
                if (with != null)
                    action?.Invoke(with);
            }
        }

        /// <summary>
        /// Invokes the <paramref name="action"/> when the <paramref name="with"/> is not the default value for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The with value <see cref="Type"/>.</typeparam>
        /// <param name="with">The value with which to verify.</param>
        /// <param name="action">The <see cref="Action"/> to invoke when there is a valid <paramref name="with"/> value.</param>
        public static void With<T>(T with, Action action)
        {
            if (Comparer<T>.Default.Compare(with, default) != 0 && Comparer<T>.Default.Compare(with, default) != 0)
            {
                if (!(with is string) && with is System.Collections.IEnumerable ie && !ie.GetEnumerator().MoveNext())
                    return;

                action?.Invoke();
            }
        }
#pragma warning restore CA1000
    }
}