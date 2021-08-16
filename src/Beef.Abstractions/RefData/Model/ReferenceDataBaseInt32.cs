// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;

namespace Beef.RefData.Model
{
    /// <summary>
    /// Represents the <b>model</b> version of the <see cref="RefData.ReferenceDataBase"/> with an <see cref="System.Int32"/> <see cref="Id"/>.
    /// </summary>
    public abstract class ReferenceDataBaseInt32 : ReferenceDataBase, IInt32Identifier
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [JsonProperty("id", Order = 0)]
        public int Id { get; set; }
    }
}