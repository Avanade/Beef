// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents an <see cref="JToken"/> to JSON <see cref="string"/> converter.
    /// </summary>
    public class JTokenToJsonConverter : CustomConverter<JToken, string?>
    {
        private static readonly Lazy<JTokenToJsonConverter> _default = new(() => new JTokenToJsonConverter(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static JTokenToJsonConverter Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="JTokenToJsonConverter"/> class.
        /// </summary>
        public JTokenToJsonConverter() : base(
            s => s?.ToString(Formatting.None),
            d => string.IsNullOrEmpty(d) ? default! : JToken.Parse(d))
        { }
    }
}