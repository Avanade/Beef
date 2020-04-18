// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;

namespace Beef.RefData.Model
{
    /// <summary>
    /// Represents the <b>model</b> version of the <see cref="RefData.ReferenceDataBase"/>.
    /// </summary>
    public abstract class ReferenceDataBase : Entities.IChangeLog
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        [JsonProperty("code")]
        public string? Code { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        [JsonProperty("text")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        [JsonProperty("sortOrder", NullValueHandling = NullValueHandling.Ignore)]
        public int SortOrder { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase"/> is Active.
        /// </summary>
        [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the validity start date.
        /// </summary>
        [JsonProperty("startDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the validity end date.
        /// </summary>
        [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        [JsonProperty("etag", NullValueHandling = NullValueHandling.Ignore)]
        public string? ETag { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Entities.ChangeLog"/>.
        /// </summary>
        [JsonProperty("changeLog", NullValueHandling = NullValueHandling.Ignore)]
        public Entities.ChangeLog? ChangeLog { get; set; }
    }
}