// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a transform option for a <see cref="String"/> value.
    /// </summary>
    public enum StringTransform
    {
        /// <summary>Indicates that the <see cref="Cleaner.DefaultStringTransform"/> value should be used.</summary>
        UseDefault,
        /// <summary>No transform required; the <see cref="String"/> value will remain as-is.</summary>
        None,
        /// <summary>The string will be transformed from a <c>null</c> to <see cref="String.Empty"/> value.</summary>
        NullToEmpty,
        /// <summary>The string will be transformed from an <see cref="String.Empty"/> value to a <c>null</c>.</summary>
        EmptyToNull
    }
}
