// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.RefData
{
    /// <summary>
    /// Represents the filter for a reference data collection.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ReferenceDataFilter : EntityBase
    {
        private List<string> _codes;
        private string _text;

        /// <summary>
        /// Gets the validator to ensure the wildcards are considered valid.
        /// </summary>
        public static Validator<ReferenceDataFilter> Validator { get; } = Validator<ReferenceDataFilter>.Create().HasProperty(x => x.Text, p => p.Wildcard());

        /// <summary>
        /// Validates the <paramref name="codes"/> and <paramref name="text"/> then applies as a filter to the reference data <paramref name="coll"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="System.Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="System.Type"/>.</typeparam>
        /// <param name="coll">The reference data collection.</param>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>The filtered collection.</returns>
        public static TColl ApplyFilter<TColl, TItem>(TColl coll, List<string> codes = null, string text = null) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            return ApplyFilter<TColl, TItem>(coll, new ReferenceDataFilter { Codes = codes, Text = text });
        }

        /// <summary>
        /// Validates the <paramref name="filter"/> then applies as a filter to the reference data <paramref name="coll"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="System.Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="System.Type"/>.</typeparam>
        /// <param name="coll">The reference data collection.</param>
        /// <param name="filter">The <see cref="ReferenceDataFilter"/>.</param>
        /// <returns>The filtered collection.</returns>
        public static TColl ApplyFilter<TColl, TItem>(TColl coll, ReferenceDataFilter filter) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            if (filter == null || filter.IsInitial)
                return coll;

            // Validate the arguments.
            Validator.Validate(filter).ThrowOnError();

            // Apply the filter.
            var result = new TColl();
            result.AddRange(coll.WhereWhen(x => filter.Codes.Contains(x.Code, StringComparer.OrdinalIgnoreCase), filter.Codes != null && filter.Codes.Count > 0).WhereWildcard(x => x.Text, filter.Text));
            result.GenerateETag();
            return result;
        }

        /// <summary>
        /// Gets or sets the list of codes.
        /// </summary>
        [JsonProperty("codes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Codes
        {
            get => _codes;
            set => SetValue(ref _codes, value, false, false, nameof(Codes));
        }

        /// <summary>
        /// Gets or sets the text (including wildcards).
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Text
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