// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Provides base configuration capabilities.
    /// </summary>
    public abstract class ConfigBase
    {
        /// <summary>
        /// Defaults the <see cref="string"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static string? DefaultWhereNull(string? value, Func<string?> defaultValue) => string.IsNullOrEmpty(value) ? Check.NotNull(defaultValue, nameof(defaultValue))() : value;

        /// <summary>
        /// Defaults the <see cref="bool"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static bool? DefaultWhereNull(bool? value, Func<bool?> defaultValue) => value.HasValue ? value : Check.NotNull(defaultValue, nameof(defaultValue))();

        /// <summary>
        /// Compares the <see cref="string"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(string? propertyValue, string compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="bool"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(bool? propertyValue, bool compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="string"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal; otherwise, <c>false</c>.</returns>
        public static bool CompareValue(string? propertyValue, string compareTo) => propertyValue != null && propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="bool"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal; otherwise, <c>false</c>.</returns>
        public static bool CompareValue(bool? propertyValue, bool compareTo) => propertyValue != null && propertyValue == compareTo;
    }

    /// <summary>
    /// Provides the base <see cref="Prepare(TParent)"/> configuration capabilities.
    /// </summary>
    /// <typeparam name="TParent">The parent <see cref="Type"/>.</typeparam>
    public abstract class ConfigBase<TParent> : ConfigBase where TParent : class
    {
        /// <summary>
        /// Gets the <b>Parent</b> (set via <see cref="Prepare(TParent)"/> execution).
        /// </summary>
        public TParent? Parent { get; private set; }

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution.
        /// </summary>
        /// <param name="parent">The parent obkect.</param>
        public void Prepare(TParent parent)
        {
            Parent = parent;
            Prepare();
        }

        /// <summary>
        /// Performs the actual prepare logic.
        /// </summary>
        protected abstract void Prepare();
    }
}