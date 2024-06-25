// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Validation;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.RefData
{
    /// <summary>
    /// Represents a filterer for a reference data collection.
    /// </summary>
    public static class ReferenceDataFilterer
    {
        /// <summary>
        /// Gets the validator to ensure the wildcards are considered valid.
        /// </summary>
        public static Validator<ReferenceDataFilter> Validator { get; } = Validation.Validator.Create<ReferenceDataFilter>().HasProperty(x => x.Text, p => p.Wildcard());

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
        public static Task<ReferenceDataFilterResult<TItem>> ApplyFilterAsync<TColl, TItem>(TColl coll, IEnumerable<string>? codes = null, string? text = null, bool includeInactive = false) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            return ApplyFilterAsync<TColl, TItem>(coll, new ReferenceDataFilter { Codes = codes?.Where(x => !string.IsNullOrEmpty(x)).AsEnumerable(), Text = text }, includeInactive);
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
        public static Task<ReferenceDataFilterResult<TItem>> ApplyFilterAsync<TColl, TItem>(TColl coll, StringValues codes = default, string? text = null, bool includeInactive = false) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            var list = new List<string>();
            foreach (var c in codes)
            {
                list.AddRange(c.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }

            return ApplyFilterAsync<TColl, TItem>(coll, new ReferenceDataFilter { Codes = list, Text = text }, includeInactive);
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
        public static async Task<ReferenceDataFilterResult<TItem>> ApplyFilterAsync<TColl, TItem>(TColl coll, ReferenceDataFilter filter, bool includeInactive = false) where TColl : ReferenceDataCollectionBase<TItem>, new() where TItem : ReferenceDataBase, new()
        {
            Check.NotNull(coll, nameof(coll));
            Check.NotNull(filter, nameof(filter));
            if (filter.Codes is null || !filter.Codes.Any() && string.IsNullOrEmpty(filter.Text) && !includeInactive)
                return new ReferenceDataFilterResult<TItem>(coll.ActiveList) { ETag = coll.ETag };

            // Validate the arguments.
            (await Validator.ValidateAsync(filter).ConfigureAwait(false)).ThrowOnError();

            // Apply the filter.
            var items = includeInactive ? coll.AllList : coll.ActiveList; 
            var list = items
                .WhereWhen(x => filter.Codes.Contains(x.Code, StringComparer.OrdinalIgnoreCase), filter.Codes != null && filter.Codes.FirstOrDefault() != null)
                .WhereWildcard(x => x.Text, filter.Text);

            return new ReferenceDataFilterResult<TItem>(list) { ETag = ETagGenerator.Generate(list) };
        }
    }
}