// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;

namespace Beef.RefData.Model
{
    /// <summary>
    /// Represents the <b>model</b> version of the <see cref="RefData.ReferenceDataBase"/> with an <see cref="System.Int64"/> <see cref="Id"/>.
    /// </summary>
    public abstract class ReferenceDataBaseInt64 : ReferenceDataBase, IInt64Identifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonProperty("id", Order = 0)]
        public long Id { get; set; }
    }
}