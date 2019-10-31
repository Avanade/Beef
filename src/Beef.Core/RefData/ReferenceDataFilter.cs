// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Validation;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// The underlying <see cref="ReferenceDataFilter"/> result/collection.
    /// </summary>
    /// <typeparam name="TItem">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
    public class ReferenceDataFilterResult<TItem> : EntityBaseCollection<TItem>, IReferenceDataFilterResult where TItem : ReferenceDataBase
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
        public string ETag { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="ReferenceDataBase"/> collection.
        /// </summary>
        IEnumerable<ReferenceDataBase> IReferenceDataFilterResult.Collection => this;

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
        private IEnumerable<string> _codes;
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
        /// <param name="includeInactive">Indicates whether to include inactive (<see cref="ReferenceDataBase.IsActive"/> equal <c>false</c>) entries.</param>
        /// <returns>The filtered collection and corresponding ETag.</returns>
        public static ReferenceDataFilterResult<TItem> ApplyFilter<TColl, TItem>(TColl coll, IEnumerable<string> codes = null, string text = null, bool includeInactive = false) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            return ApplyFilter<TColl, TItem>(coll, new ReferenceDataFilter { Codes = codes?.Where(x => !string.IsNullOrEmpty(x)).AsEnumerable(), Text = text }, includeInactive);
        }

        /// <summary>
        /// Validates the <paramref name="codes"/> and <paramref name="text"/> then applies as a filter to the reference data <paramref name="coll"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="System.Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="System.Type"/>.</typeparam>
        /// <param name="coll">The reference data collection.</param>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <param name="includeInactive">Indicates whether to include inactive (<see cref="ReferenceDataBase.IsActive"/> equal <c>false</c>) entries.</param>
        /// <returns>The filtered collection and corresponding ETag.</returns>
        public static ReferenceDataFilterResult<TItem> ApplyFilter<TColl, TItem>(TColl coll, StringValues codes = default, string text = null, bool includeInactive = false) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            var list = new List<string>();
            foreach (var c in codes)
            {
                list.AddRange(c.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }

            return ApplyFilter<TColl, TItem>(coll, new ReferenceDataFilter { Codes = list, Text = text }, includeInactive);
        }

        /// <summary>
        /// Validates the <paramref name="filter"/> then applies as a filter to the reference data <paramref name="coll"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="System.Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="System.Type"/>.</typeparam>
        /// <param name="coll">The reference data collection.</param>
        /// <param name="filter">The <see cref="ReferenceDataFilter"/>.</param>
        /// <param name="includeInactive">Indicates whether to include inactive (<see cref="ReferenceDataBase.IsActive"/> equal <c>false</c>) entries.</param>
        /// <returns>The filtered collection and corresponding ETag.</returns>
        public static ReferenceDataFilterResult<TItem> ApplyFilter<TColl, TItem>(TColl coll, ReferenceDataFilter filter, bool includeInactive = false) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            Check.NotNull(filter, nameof(filter));
            if (!filter.Codes.Any() && string.IsNullOrEmpty(filter.Text) && !includeInactive)
                return new ReferenceDataFilterResult<TItem>(coll.ActiveList) { ETag = coll.ETag };

            // Validate the arguments.
            Validator.Validate(filter).ThrowOnError();

            // Apply the filter.
            var items = includeInactive ? coll.AllList : coll.ActiveList; 
            var list = items
                .WhereWhen(x => filter.Codes.Contains(x.Code, StringComparer.OrdinalIgnoreCase), filter.Codes != null && filter.Codes.FirstOrDefault() != null)
                .WhereWildcard(x => x.Text, filter.Text);

            return new ReferenceDataFilterResult<TItem>(list) { ETag = GenerateETag(list) };
        }

        /// <summary>
        /// Generates an ETag as an <see cref="System.Security.Cryptography.SHA1"/> hash of the collection contents.
        /// </summary>
        private static string GenerateETag(IEnumerable<ReferenceDataBase> items)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var buf = System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(items));
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Gets or sets the list of codes.
        /// </summary>
        [JsonProperty("codes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IEnumerable<string> Codes
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