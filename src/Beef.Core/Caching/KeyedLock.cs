// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Beef.Caching
{
    /// <summary>
    /// Provides concurrency locking for a specified key value.
    /// </summary>
    /// <typeparam name="TKey">The key <see cref="Type"/>.</typeparam>
    public class KeyedLock<TKey>
    {
        private readonly ConcurrentDictionary<TKey, object> _lockDict = new ConcurrentDictionary<TKey, object>();

        /// <summary>
        /// An internal class to manager creation and disposal of a lock essentially for the <c>using</c> statement.
        /// </summary>
        public class KeyedLockManager : IDisposable
        {
            private readonly object _lock;
            private readonly bool _gotLock = false;

            /// <summary>
            /// Private constructor.
            /// </summary>
            internal KeyedLockManager(object lockObj)
            {
                _lock = lockObj;
                Monitor.Enter(_lock, ref _gotLock);
            }

            /// <summary>
            /// Releases the lock.
            /// </summary>
            public void Dispose()
            {
                if (_gotLock)
                    Monitor.Exit(_lock);
            }
        }

        /// <summary>
        /// Creates a lock for use with a <c>using</c> statement.
        /// </summary>
        /// <param name="key">The key value.</param>
        /// <returns>The <see cref="KeyedLockManager"/> with the required <see cref="IDisposable.Dispose"/> to unlock at completion.</returns>
        public KeyedLockManager Lock(TKey key)
        {
            return new KeyedLockManager(GetLock(key));
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
        public object GetLock(TKey key)
        {
            return _lockDict.GetOrAdd(key, new object());
        }

        /// <summary>
        /// Removes the key from the lock.
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
