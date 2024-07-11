/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CoreEx.Entities;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Represents the Work History entity.
    /// </summary>
    public partial class WorkHistory : IPrimaryKey
    {
        /// <summary>
        /// Gets or sets the <c>Person</c> identifier (not serialized/read-only for internal data merging).
        /// </summary>
        [JsonIgnore]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the Start Date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the End Date.
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Creates the primary <see cref="CompositeKey"/>.
        /// </summary>
        /// <returns>The primary <see cref="CompositeKey"/>.</returns>
        /// <param name="name">The <see cref="Name"/>.</param>
        public static CompositeKey CreatePrimaryKey(string? name) => new CompositeKey(name);

        /// <summary>
        /// Gets the primary <see cref="CompositeKey"/> (consists of the following property(s): <see cref="Name"/>).
        /// </summary>
        [JsonIgnore]
        public CompositeKey PrimaryKey => CreatePrimaryKey(Name);
    }

    /// <summary>
    /// Represents the <c>WorkHistory</c> collection.
    /// </summary>
    public partial class WorkHistoryCollection : List<WorkHistory> { }
}

#pragma warning restore
#nullable restore