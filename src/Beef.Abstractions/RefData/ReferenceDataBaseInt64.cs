// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a <b>ReferenceData</b> base class with an <see cref="Id"/> <see cref="System.Type"/> of <see cref="System.Int64"/>.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Code = {Code}, Text = {Text}, IsActive={IsActive}, IsValid={IsValid}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ReferenceDataBaseInt64 : ReferenceDataBase, IInt64Identifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataBaseInt64"/> class.
        /// </summary>
        public ReferenceDataBaseInt64() : base(ReferenceDataIdTypeCode.Int64, 0) { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable).</remarks>
        [JsonProperty("id", Order = 0)]
        public new long Id
        {
            get { return base.Id == null || base.Id is not long lid ? 0 : lid; }
            set { base.Id = value; }
        }
    }
}
