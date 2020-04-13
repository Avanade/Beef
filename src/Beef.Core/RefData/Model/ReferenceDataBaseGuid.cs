// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;

namespace Beef.RefData.Model
{
    /// <summary>
    /// Represents the <b>model</b> version of the <see cref="RefData.ReferenceDataBase"/> with a <see cref="Guid"/> <see cref="Id"/>.
    /// </summary>
    public abstract class ReferenceDataBaseGuid : ReferenceDataBase, IGuidIdentifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonProperty("id", Order = 0)]
        public Guid Id { get; set; }
    }
}