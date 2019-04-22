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
        /// Gets the <see cref="PropertyInfo"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to reflect.</param>
        /// <param name="propertyName">The property name to find.</param>
        /// <param name="bindingFlags">The optional <see cref="BindingFlags"/>.</param>
        /// <returns>The corresponding <see cref="PropertyInfo"/> where found; otherwise, <c>null</c>.</returns>
        public static PropertyInfo GetPropertyInfo(Type type, string propertyName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance)
        {
            Check.NotNull(type, nameof(type));
            Check.NotEmpty(propertyName, nameof(propertyName));

            var pis = type.GetProperties().Where(x => x.Name == propertyName).ToArray();
            if (pis.Length == 0)
                return null;
            else if (pis.Length == 1)
                return pis[0];

            return pis.FirstOrDefault(x => x.DeclaringType == type) ?? pis.First();
        }
    }
}