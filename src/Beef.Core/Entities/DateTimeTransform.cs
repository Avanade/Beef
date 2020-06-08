// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a transform option for a <see cref="DateTime"/> value.
    /// </summary>
    /// <remarks>The option to perform specified <see cref="DateTime"/> transformations ensures that the <see cref="EntityBase"/> is updated with correctly
    /// specified <see cref="DateTime"/> values and <see cref="DateTime.Kind"/>. This is especially important where serialization is required across the likes
    /// of time-zones to ensure that the correct values are passed.</remarks>
    public enum DateTimeTransform
    {
        /// <summary>Indicates that the <see cref="Cleaner.DefaultDateTimeTransform"/> value should be used.</summary>
        UseDefault,
        /// <summary>No transform required; the <see cref="DateTime"/> value will be updated as-is.</summary>
        None,
        /// <summary>A <b>DateOnly</b> transform is required; the <see cref="DateTime"/> value will be updated with the <see cref="DateTime.Date"/> only
        /// and the <see cref="DateTime.Kind"/> will be set to <see cref="DateTimeKind.Unspecified"/>.</summary>
        DateOnly,
        /// <summary>A <b>DateTime</b> transform is required; the <see cref="DateTime"/> value will be updated as-is
        /// and the <see cref="DateTime.Kind"/> will be set to <see cref="DateTimeKind.Local"/>.</summary>
        DateTimeLocal,
        /// <summary>A <b>DateTime</b> transform is required; the <see cref="DateTime"/> value will be updated as-is
        /// and the <see cref="DateTime.Kind"/> will be set to <see cref="DateTimeKind.Utc"/>.</summary>
        DateTimeUtc,
        /// <summary>A <b>DateTime</b> transform is required; the <see cref="DateTime"/> value will be updated as-is
        /// and the <see cref="DateTime.Kind"/> will be set to <see cref="DateTimeKind.Unspecified"/>.</summary>
        DateTimeUnspecified
    }
}
