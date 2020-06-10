/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options
#pragma warning disable CA2227 // Collection properties should be read only; ignored, as acceptable for a DTO.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Beef.Entities;
using Newtonsoft.Json;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Represents the other <see cref="Person"/> without <see cref="EntityBase"/> capabilities entity.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class PersonOther : IGuidIdentifier, IChangeLog, IETag
    {
        /// <summary>
        /// Gets or sets the <see cref="Person"/> identifier.
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the First Name.
        /// </summary>
        [JsonProperty("firstName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        [JsonProperty("lastName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the ETag.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ETag { get; set; }

        /// <summary>
        /// Gets or sets the Change Log (see <see cref="ChangeLog"/>).
        /// </summary>
        [JsonProperty("changeLog", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ChangeLog? ChangeLog { get; set; }
    } 

    /// <summary>
    /// Represents a <see cref="PersonOther"/> collection.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Tightly coupled; OK.")]
    public partial class PersonOtherCollection : List<PersonOther> { }
}

#pragma warning restore CA2227
#pragma warning restore IDE0005
#nullable restore