// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;

namespace Beef.RefData.Model
{
    /// <summary>
    /// Represents the <b>model</b> version of the <see cref="RefData.ReferenceDataBase"/> with a <see cref="string"/> <see cref="Id"/>.
    /// </summary>
    public abstract class ReferenceDataBaseString : ReferenceDataBase, IStringIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonProperty("id", Order = 0)]
        public string? Id { get; set; }
    }
}