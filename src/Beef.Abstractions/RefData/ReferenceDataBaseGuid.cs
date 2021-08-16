// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a <b>ReferenceData</b> base class with an <see cref="Id"/> <see cref="Type"/> of <see cref="Guid"/>.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Code = {Code}, Text = {Text}, IsActive={IsActive}, IsValid={IsValid}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ReferenceDataBaseGuid : ReferenceDataBase, IGuidIdentifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataBaseGuid"/> class.
        /// </summary>
        public ReferenceDataBaseGuid() : base(ReferenceDataIdTypeCode.Guid, Guid.Empty) { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable).</remarks>
        [JsonProperty("id", Order = 0)]
        public new Guid Id
        {
            get { return base.Id == null || base.Id is not Guid gid ? Guid.Empty : gid; }
            set { base.Id = value; }
        }
    }
}