// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a <b>ReferenceData</b> base class with an <see cref="Id"/> <see cref="System.Type"/> of <see cref="System.Int32"/>.
    /// </summary>
    [DebuggerDisplay("Id = {Id}, Code = {Code}, Text = {Text}, IsActive={IsActive}, IsValid={IsValid}")]
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ReferenceDataBaseInt32 : ReferenceDataBase, IInt32Identifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataBaseInt32"/> class.
        /// </summary>
        public ReferenceDataBaseInt32() : base(ReferenceDataIdTypeCode.Int32, 0) { }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable).</remarks>
        [JsonProperty("id", Order = 0)]
        public new int Id
        {
            get { return base.Id == null || base.Id is not int iid ? 0 : iid; }
            set { base.Id = value; }
        }
    }
}
