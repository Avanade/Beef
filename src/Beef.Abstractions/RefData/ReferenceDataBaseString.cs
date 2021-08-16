// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a <b>ReferenceData</b> base class with an <see cref="Id"/> <see cref="System.Type"/> of <see cref="System.String"/>.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Code = {Code}, Text = {Text}, IsActive={IsActive}, IsValid={IsValid}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ReferenceDataBaseString : ReferenceDataBase, IStringIdentifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataBaseString"/> class.
        /// </summary>
        public ReferenceDataBaseString() : base(ReferenceDataIdTypeCode.String, null!) { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable).</remarks>
        [JsonProperty("id", Order = 0)]
        public new string? Id
        {
            get { return base.Id == null || base.Id is not string sid ? null : sid; }
            set { base.Id = value; }
        }
    }
}