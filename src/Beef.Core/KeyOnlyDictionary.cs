// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Beef
{
    /// <summary>
    /// Enables a concurrent key-only dictionary.
    /// </summary>
    public class KeyOnlyDictionary<T> : ConcurrentDictionary<T, object>
    {
        /// <summary>
        /// Returns a string with the concatenation of all the keys.
        /// </summary>
        /// <returns>The key concatenation.</returns>
        public override string ToString() => string.Join(", ", Keys);

        /// <summary>
        /// Adds a new key.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="errorOnDuplicate">Indicates that an exception is to be thrown where the key already exists; otherwise, skip/ignore.</param>
        public void Add(T key, bool errorOnDuplicate = false)
        {
            if (!TryAdd(key, null!) && errorOnDuplicate)
                throw new System.InvalidOperationException("Key already exists and would result in a duplicate.");
        }

        /// <summary>
        /// Adds the range of keys.
        /// </summary>
        /// <param name="list">The list of keys.</param>
        /// <param name="errorOnDuplicate">Indicates that an exception is to be thrown where the key already exists; otherwise, skip/ignore.</param>
        public void AddRange(IEnumerable<T> list, bool errorOnDuplicate = false)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    Add(item, errorOnDuplicate);
                }
            }
        }
    }
}