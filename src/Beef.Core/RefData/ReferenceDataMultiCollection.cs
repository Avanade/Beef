// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a collection that supports multiple Reference Data collections.
    /// </summary>
    public class ReferenceDataMultiCollection : List<ReferenceDataMultiItem> { }

    /// <summary>
    /// Represents a <see cref="Name">named</see> <see cref="IReferenceDataCollection"/>.
    /// </summary>
    public class ReferenceDataMultiItem : IETag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataMultiItem"/> class for a <paramref name="name"/> and <paramref name="refDataResult"/>.
        /// </summary>
        /// <param name="name">The <see cref="Name"/>.</param>
        /// <param name="refDataResult">The <see cref="IReferenceDataFilterResult"/>.</param>
        public ReferenceDataMultiItem(string name, IReferenceDataFilterResult refDataResult)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(refDataResult, nameof(refDataResult));

            Name = name;
            Items = refDataResult.Collection;
            ETag = refDataResult.ETag;
        }

        /// <summary>
        /// Gets or sets the reference data name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection <see cref="IETag.ETag"/>.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ETag { get; set; }

        /// <summary>
        /// Gets or sets the reference data collection.
        /// </summary>
        [JsonProperty("items")]
        public IEnumerable<ReferenceDataBase> Items { get; set; }
    }
}