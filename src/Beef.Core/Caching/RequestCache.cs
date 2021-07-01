// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Beef.Caching
{
    /// <summary>
    /// Provides a basic dictionary backed cache for short-lived data within the context of a request scope to reduce data chattiness.
    /// </summary>
    public class RequestCache : IRequestCache
    {
        private readonly Lazy<ConcurrentDictionary<Tuple<Type, UniqueKey>, object>> _caching = new(true);

        /// <summary>
        /// Gets the cached value associated with the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The cached value where found; otherwise, the default value for the <see cref="Type"/>.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue<T>(UniqueKey key, out T value)
        {
            if (_caching.IsValueCreated && _caching.Value.TryGetValue(new Tuple<Type, UniqueKey>(typeof(T), key), out object val))
            {
                value = (T)val;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        /// Sets (adds or overrides) the cache value for the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetValue<T>(UniqueKey key, T value)
        {
            _caching.Value.AddOrUpdate(new Tuple<Type, UniqueKey>(typeof(T), key), value!, (x, y) => value!);
        }

        /// <summary>
        /// Removes the cached value associated with the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns><c>true</c> where found and removed; otherwise, <c>false</c>.</returns>
        public bool Remove<T>(UniqueKey key)
        {
            if (_caching.IsValueCreated)
                return _caching.Value.TryRemove(new Tuple<Type, UniqueKey>(typeof(T), key), out object _);
            else
                return false;
        }

        /// <summary>
        /// Clears the cache for the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        public void Clear<T>()
        {
            if (!_caching.IsValueCreated)
                return;

            foreach (var item in _caching.Value.Where(x => x.Key.Item1 == typeof(T)).ToList())
            {
                _caching.Value.TryRemove(item.Key, out object val);
            }
        }

        /// <summary>
        /// Clears the cache for all <see cref="Type">types</see>.
        /// </summary>
        public void ClearAll()
        {
            if (_caching.IsValueCreated)
                _caching.Value.Clear();
        }
    }
}