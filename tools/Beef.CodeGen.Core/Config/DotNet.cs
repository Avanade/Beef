// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Provides <b>.NET</b> specific capabilities.
    /// </summary>
    public static class DotNet
    {
        /// <summary>
        /// The list of standard system <see cref="Type"/> names.
        /// </summary>
        public static List<string> SystemTypes => new()
        {
            "void", "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "string", "DateTime", "DateTimeOffset", "TimeSpan", "Guid"
        };

        /// <summary>
        /// The list of system <see cref="Type"/> names that should not be nullable by default.
        /// </summary>
        public static List<string> IgnoreNullableTypes => new()
        {
            "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "DateTime", "DateTimeOffset", "TimeSpan", "Guid"
        };
    }
}