// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Beef.RefData
{
    /// <summary>
    /// Provides the <see cref="Collection"/> and <see cref="IETag.ETag"/>.
    /// </summary>
    public interface IReferenceDataFilterResult : IETag
    {
        /// <summary>
        /// Gets the underlying <see cref="ReferenceDataBase"/> collection.
        /// </summary>
        IEnumerable<ReferenceDataBase> Collection { get; }
    }

#pragma warning disable CA1710 // Identifiers should have correct suffix; by-design, class is a result (that supports collection serialization).
    /// <summary>
    /// The underlying <see cref="ReferenceDataFilter"/> result/collection.
    /// </summary>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
    public class ReferenceDataFilterResult<TItem> : EntityBaseCollection<TItem>, IReferenceDataFilterResult where TItem : ReferenceDataBase
#pragma warning restore CA1710 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataFilterResult{TItem}" /> class.
        /// </summary>
        public ReferenceDataFilterResult() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataFilterResult{TItem}" /> class.
        /// </summary>
        /// <param name="collection">The entities.</param>
        public ReferenceDataFilterResult(IEnumerable<TItem> collection) : base(collection) { }

        /// <summary>
        /// Gets or sets the <see cref="IETag.ETag"/>.
        /// </summary>
        public string? ETag { get; set; }

#pragma warning disable CA1033 // Interface methods should be callable by child types; by-design, hiding is the desired outcome.
        /// <summary>
        /// Gets the underlying <see cref="ReferenceDataBase"/> collection.
        /// </summary>
        IEnumerable<ReferenceDataBase> IReferenceDataFilterResult.Collection => this;
#pragma warning restore CA1033 

        /// <summary>
        /// Creates a deep copy of the <see cref="ReferenceDataFilterResult{TItem}"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="ReferenceDataFilterResult{TItem}"/>.</returns>
        public override object Clone()
        {
            var clone = new ReferenceDataFilterResult<TItem> { ETag = ETag };
            foreach (TItem item in this)
            {
                clone.Add((TItem)item.Clone());
            }

            return clone;
        }
    }
     
    /// <summary>
    /// Represents a filter for a reference data collection.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ReferenceDataFilter : EntityBase
    {
        private IEnumerable<string>? _codes;
        private string? _text;

        /// <summary>
        /// Gets or sets the list of codes.
        /// </summary>
        [JsonProperty("codes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<string>? Codes
        {
            get => _codes;
            set => SetValue(ref _codes, value, false, false, nameof(Codes));
        }

        /// <summary>
        /// Gets or sets the text (including wildcards).
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Text
        {
            get => _text;
            set => SetValue(ref _text, value, false, StringTrim.End, StringTransform.EmptyToNull, nameof(Text));
        }

        #region ICleanUp

        /// <summary>
        /// Performs a clean-up of the <see cref="ReferenceDataFilter"/> resetting property values as appropriate to ensure a basic level of data consistency.
        /// </summary>
        public override void CleanUp()
        {
            base.CleanUp();
            Text = Cleaner.Clean(Text, StringTrim.End, StringTransform.EmptyToNull);
        }

        /// <summary>
        /// Indicates whether considered initial; i.e. all properties have their initial value.
        /// </summary>
        /// <returns><c>true</c> indicates is initial; otherwise, <c>false</c>.</returns>
        public override bool IsInitial
        {
            get
            {
                return Cleaner.IsInitial(Codes)
                    && Cleaner.IsInitial(Text);
            }
        }

        #endregion

        #region ICopyFrom

        /// <summary>
        /// Performs a copy from another <see cref="ReferenceDataFilter"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="ReferenceDataFilter"/> to copy from.</param>
        public override void CopyFrom(object from)
        {
            var fval = ValidateCopyFromType<ReferenceDataFilter>(from);
            CopyFrom((ReferenceDataFilter)fval);
        }

        /// <summary>
        /// Performs a copy from another <see cref="ReferenceDataFilter"/> updating this instance.
        /// </summary>
        /// <param name="from">The <see cref="ReferenceDataFilter"/> to copy from.</param>
        public void CopyFrom(ReferenceDataFilter from)
        {
            Check.NotNull(from, nameof(from));
            CopyFrom((EntityBase)from);
            Codes = from.Codes;
            Text = from.Text;
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Creates a deep copy of the <see cref="ReferenceDataFilter"/>.
        /// </summary>
        /// <returns>A deep copy of the <see cref="ReferenceDataFilter"/>.</returns>
        public override object Clone()
        {
            var clone = new ReferenceDataFilter();
            clone.CopyFrom(this);
            return clone;
        }

        #endregion
    }
}