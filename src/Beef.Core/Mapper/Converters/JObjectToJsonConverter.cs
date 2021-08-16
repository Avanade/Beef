// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents an <see cref="JObject"/> to JSON <see cref="string"/> converter.
    /// </summary>
    public class JObjectToJsonConverter : CustomConverter<JObject, string?>
    {
        private static readonly Lazy<JObjectToJsonConverter> _default = new(() => new JObjectToJsonConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static JObjectToJsonConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JObjectToJsonConverter"/> class.
        /// </summary>
        public JObjectToJsonConverter() : base(
            s => s?.ToString(Formatting.None),
            d => string.IsNullOrEmpty(d) ? default! : JObject.Parse(d))
        { }
    }
}