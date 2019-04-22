// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides extension methods to support testing.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts an <see cref="int"/> to a <see cref="Guid"/>; e.g. '1' will be '00000001-0000-0000-0000-000000000000'.
        /// </summary>
        /// <param name="value">The <see cref="int"/> value.</param>
        /// <returns>The corresponding <see cref="Guid"/>.</returns>
        /// <remarks>Sets the first argument with the <paramref name="value"/> and the remainder with zeroes using <see cref="Guid(int, short, short, byte[])"/></remarks>
        public static Guid ToGuid(this int value)
        {
            return new Guid(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
