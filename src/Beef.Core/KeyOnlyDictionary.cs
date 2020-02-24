// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;

namespace Beef
{
    /// <summary>
    /// Enables a key-only dictionary.
    /// </summary>
    public class KeyOnlyDictionary<T> : Dictionary<T, object>
    {
        private readonly object _lock = new object();

        /// <summary>
        /// Returns a string with the concatenation of all the keys.
        /// </summary>
        /// <returns>The key concatenation.</returns>
        public override string ToString()
        {
            return string.Join(", ", Keys);
        }

        /// <summary>
        /// Adds a new key.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="errorOnDuplicate">Indicates that an exception is to be thrown where the key already exists.</param>
        public void Add(T key, bool errorOnDuplicate = false)
        {
            lock (_lock)
            {
                if (!errorOnDuplicate && ContainsKey(key))
                    return;

                Add(key, null!);
            }
        }

        /// <summary>
        /// Adds the range of keys.
        /// </summary>
        /// <param name="list">The list of keys.</param>
        /// <param name="errorOnDuplicate">Indicates that an exception is to be thrown where the key already exists.</param>
        public void AddRange(IEnumerable<T> list, bool errorOnDuplicate = false)
        {
            lock (_lock)
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
}