// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a collection that supports multiple Reference Data collections.
    /// </summary>
    /// <remarks>Enables the passing of </remarks>
    public class ReferenceDataMultiCollection : List<ReferenceDataMultiItem> { }

    /// <summary>
    /// Represents a <see cref="Name">named</see> <see cref="IReferenceDataCollection"/>.
    /// </summary>
    public class ReferenceDataMultiItem : IETag
    {
        private IReferenceDataCollection _items;

        /// <summary>
        /// Gets or sets the reference data name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection <see cref="IETag.ETag"/>.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the reference data collection.
        /// </summary>
        [JsonProperty("items")]
        public IReferenceDataCollection Items
        {
            get => _items;

            set
            {
                _items = value;
                ETag = _items?.ETag;
            }
        }
    }
}
