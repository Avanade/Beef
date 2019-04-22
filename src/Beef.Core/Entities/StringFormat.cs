// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Provides string formatting constants.
    /// </summary>
    public static class StringFormat
    {
        /// <summary>
        /// The string format for a <see cref="DateTime"/> and <see cref="DateTimeOffset"/> with the date-only modifier.
        /// </summary>
        public const string DateOnlyFormat = "d";

        /// <summary>
        /// The string format for a <see cref="DateTime"/>.
        /// </summary>
        public const string DateTimeFormat = "g";

        /// <summary>
        /// The string format for a <see cref="DateTimeOffset"/>.
        /// </summary>
        public const string DateTimeOffsetFormat = "g";
    }
}
