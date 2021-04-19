// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;

namespace Beef.Caching
{
    /// <summary>
    /// Enables the short-lived request caching; intended to reduce data chattiness within the context of a request scope.
    /// </summary>
    public interface IRequestCache
    {
        /// <summary>
        /// Gets the cached value associated with the specified <see cref="Type"/> and <see cref="UniqueKey"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The cached value where found; otherwise, the default value for the <see cref="Type"/>.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        bool TryGetValue<T>(UniqueKey key, out T value);

        /// <summary>
        /// Gets the cached value associated with the specified <see cref="Type"/> and <see cref="IUniqueKey"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The cached value where found; otherwise, the default value for the <see cref="Type"/>.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        bool TryGetValue<T>(IUniqueKey key, out T value) => TryGetValue((key ?? throw new ArgumentNullException(nameof(key))).UniqueKey, out value);

        /// <summary>
        /// Sets (adds or overrides) the cache value for the specified <see cref="Type"/> and <see cref="UniqueKey"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        void SetValue<T>(UniqueKey key, T value);

        /// <summary>
        /// Sets (adds or overrides) the cache value for the specified <see cref="Type"/> and <see cref="IUniqueKey"/> and returns <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>The <paramref name="value"/>.</returns>
        T? SetAndReturnValue<T>(UniqueKey key, T? value)
        {
            SetValue(key, value);
            return value;
        }

        /// <summary>
        /// Sets (adds or overrides) the cache value for the specified <see cref="Type"/> and <see cref="IUniqueKey"/> and reurns <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value to set.</param>
        /// <returns>The <paramref name="value"/>.</returns>
        T? SetAndReturnValue<T>(T? value) where T : IUniqueKey
        {
            SetValue((value ?? throw new ArgumentNullException(nameof(value))).UniqueKey, value);
            return value;
        }

        /// <summary>
        /// Removes the cached value associated with the specified <see cref="Type"/> and <see cref="UniqueKey"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns><c>true</c> where found and removed; otherwise, <c>false</c>.</returns>
        bool Remove<T>(UniqueKey key);

        /// <summary>
        /// Removes the cached value associated with the specified <see cref="Type"/> and <see cref="IUniqueKey"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns><c>true</c> where found and removed; otherwise, <c>false</c>.</returns>
        bool Remove<T>(IUniqueKey key) => Remove<T>((key ?? throw new ArgumentNullException(nameof(key))).UniqueKey);

        /// <summary>
        /// Clears the cache for the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        void Clear<T>();

        /// <summary>
        /// Clears the cache for all <see cref="Type">types</see>.
        /// </summary>
        void ClearAll();
    }
}