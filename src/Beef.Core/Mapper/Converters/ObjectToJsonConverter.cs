// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents an <see cref="object"/> to JSON <see cref="string"/> converter.
    /// </summary>
    /// <typeparam name="TSrceProperty"></typeparam>
    public class ObjectToJsonConverter<TSrceProperty> : CustomConverter<TSrceProperty, string?>
    {
        private static readonly Lazy<ObjectToJsonConverter<TSrceProperty>> _default = new(() => new ObjectToJsonConverter<TSrceProperty>(), true);

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static ObjectToJsonConverter<TSrceProperty> Default { get { return _default.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectToJsonConverter{TSrceProperty}"/> class.
        /// </summary>
        public ObjectToJsonConverter() : base(
            s => s == null ? null : JsonConvert.SerializeObject(s),
            d => string.IsNullOrEmpty(d) ? default! : JsonConvert.DeserializeObject<TSrceProperty>(d)!)
        { }
    }
}