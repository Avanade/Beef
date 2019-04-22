// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;

namespace Beef.Business
{
    /// <summary>
    /// Enables standardised connection string handling.
    /// </summary>
    public static class ConnectionString
    {
        private struct CacheKey
        {
            public Type Type;
            public string ConnectionString;
        }

        private static object _lock = new object();
        private static Dictionary<CacheKey, object> _cache = new Dictionary<CacheKey, object>();

        /// <summary>
        /// Parses a connection string and returns a key/value dictionary.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="convertKeyToUpper">Indicates whether the key is converted to uppercase.</param>
        /// <returns>The key/value dictionary.</returns>
        public static Dictionary<string, string> Parse(string connectionString, bool convertKeyToUpper = true)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var pairs = new Dictionary<string, string>();

            foreach (string part in connectionString.Split(';'))
            {
                int pos = part.IndexOf('=');
                if (pos < 0 || pos >= part.Length)
                    throw new ArgumentException("Invalid connection string format.", nameof(connectionString));

                var key = part.Substring(0, pos).Trim();
                if (convertKeyToUpper)
                    key = key.ToUpper();

                if (pairs.ContainsKey(key))
                    throw new ArgumentException("Invalid connection string format.", nameof(connectionString));

                pairs.Add(key, part.Substring(pos + 1).Trim());
            }

            if (pairs.Count == 0)
                throw new ArgumentException("Invalid connection string format.", nameof(connectionString));

            return pairs;
        }

        /// <summary>
        /// Parses a connection string and creates the corresponding connection properties <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The connection <see cref="Type"/>.</typeparam>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <remarks>Maintains an internal cache for the <paramref name="connectionString"/> and will return the same instance where already parsed (improves performance).</remarks>
        public static T Parse<T>(string connectionString) where T : class, new()
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var key = new CacheKey { Type = typeof(T), ConnectionString = connectionString };
            if (_cache.TryGetValue(key, out object value))
                return (T)value;

            var pairs = Parse(connectionString, true);
            var conn = new T();

            foreach (var pi in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty))
            {
                if (!pairs.TryGetValue(pi.Name.ToUpper(), out string val) || string.IsNullOrEmpty(val))
                    continue;

                try
                {
                    if (pi.PropertyType.IsEnum)
                        pi.SetValue(conn, Enum.Parse(pi.PropertyType, val, true));
                    else if (pi.PropertyType == typeof(SecureString))
                    {
                        var ss = new SecureString();
                        foreach (char c in val.ToCharArray())
                            ss.AppendChar(c);

                        pi.SetValue(conn, ss);
                    }
                    else
                        pi.SetValue(conn, Convert.ChangeType(val, pi.PropertyType));
                }
                catch (FormatException fex)
                {
                    throw new ArgumentException($"Connection argument '{pi.Name}' has an invalid value: {val}", nameof(connectionString), fex);
                }
                catch (OverflowException oex)
                {
                    throw new ArgumentException($"Connection argument '{pi.Name}' has an invalid value: {val}", nameof(connectionString), oex);
                }
            }

            lock (_lock)
            {
                if (_cache.TryGetValue(key, out value))
                    return (T)value;

                _cache.Add(key, conn);
                return conn;
            }
        }
    }
}