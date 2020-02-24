// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;
using System.Reflection;

namespace Beef.Reflection
{
    /// <summary>
    /// Provides common <see cref="Type"/> reflection capabilities.
    /// </summary>
    public static class TypeReflector
    {
        /// <summary>
        /// Gets all of the properties (<see cref="PropertyInfo"/>) for a <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to reflect.</param>
        /// <returns>The corresponding <see cref="PropertyInfo"/> <see cref="Array"/>.</returns>
        public static PropertyInfo[] GetProperties(Type type)
        {
            return Check.NotNull(type, nameof(type)).GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
                .Where(x => x.CanRead && x.CanWrite).GroupBy(x => x.Name).Select(g => g.First()).ToArray();
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> for a <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to reflect.</param>
        /// <param name="propertyName">The property name to find.</param>
        /// <returns>The corresponding <see cref="PropertyInfo"/> where found; otherwise, <c>null</c>.</returns>
        public static PropertyInfo? GetPropertyInfo(Type type, string propertyName)
        {
            Check.NotNull(type, nameof(type));
            Check.NotEmpty(propertyName, nameof(propertyName));

            var pis = type.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
                .Where(x => x.Name == propertyName && x.CanRead && x.CanWrite).ToArray();

            if (pis.Length == 0)
                return null;
            else if (pis.Length == 1)
                return pis[0];
            else
                return pis.FirstOrDefault(x => x.DeclaringType == type) ?? pis.First();
        }
    }
}