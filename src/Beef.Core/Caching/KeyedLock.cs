// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Concurrent;

namespace Beef.Caching
{
    /// <summary>
    /// Provides concurrency locking for a specified key value.
    /// </summary>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    public sealed class KeyedLock<TKey>
    {
        private readonly ConcurrentDictionary<TKey, object> _lockDict = new();

        /// <summary>
        /// Gets/creates a lock object for the specified key.
        /// </summary>
        /// <param name="key">The key value.</param>
        public object Lock(TKey key)
        {
            return GetLock(key);
        }

        /// <summary>
        /// Runs the <paramref name="func"/> with a key lock and returns the resultant value.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="key">The key value.</param>
        /// <param name="func">The function to invoke within the lock.</param>
        /// <returns>The resultant value.</returns>
        public TResult Lock<TResult>(TKey key, Func<TResult> func)
        {
            Check.NotNull(func, nameof(func));

            lock (GetLock(key))
            {
                return func();
            }
        }

        /// <summary>
        /// Runs the <paramref name="action"/> with a key lock.
        /// </summary>
        /// <param name="key">The key value.</param>
        /// <param name="action">The action to invoke within the lock.</param>
        public void Lock(TKey key, Action action)
        {
            Check.NotNull(action, nameof(action));

            lock (GetLock(key))
            {
                action();
            }
        }

        /// <summary>
        /// Gets the lock for the key.
        /// </summary>
        /// <param name="key">The key value.</param>
        /// <returns>The key lock.</returns>
        private object GetLock(TKey key)
        {
            return _lockDict.GetOrAdd(key, new object());
        }

        /// <summary>
        /// Removes the key from the lock (where it exists).
        /// </summary>
        /// <param name="key">The key value.</param>
        public void Remove(TKey key)
        {
            _lockDict.TryRemove(key, out _);
        }

        /// <summary>
        /// Clears all keys from the lock.
        /// </summary>
        public void Clear()
        {
            _lockDict.Clear();
        }
    }
}
