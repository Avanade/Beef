// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper.Converters;
using System;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents a database <b>RowVersion</b> converter.
    /// </summary>
    public class DatabaseRowVersionConverter : CustomConverter<string?, byte[]>
    {
        private static readonly Lazy<DatabaseRowVersionConverter> _default = new Lazy<DatabaseRowVersionConverter>(() => new DatabaseRowVersionConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static DatabaseRowVersionConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JObjectToJsonConverter"/> class.
        /// </summary>
        public DatabaseRowVersionConverter() : base(
            s => s == null ? Array.Empty<byte>() : Convert.FromBase64String(s.StartsWith('\"') && s.EndsWith('\"') ? s[1..^1] : s),
            d => $"\"{Convert.ToBase64String(d)}\"")
        { }
    }
}