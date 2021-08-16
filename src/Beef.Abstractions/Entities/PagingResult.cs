// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Represents the resulting paging response including <see cref="TotalCount"/> and <see cref="TotalPages"/> where applicable for the subsequent query.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class PagingResult : PagingArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagingResult"/> class from a <see cref="PagingArgs"/> and optional total count.
        /// </summary>
        /// <param name="pagingArgs">The <see cref="PagingArgs"/>.</param>
        /// <param name="totalCount">The total record count where applicable.</param>
        /// <remarks>Where the <paramref name="pagingArgs"/> and <paramref name="totalCount"/> are both provided the <see cref="TotalPages"/> will be automatically created.</remarks>
        public PagingResult(PagingArgs pagingArgs, long? totalCount = null)
        {
            Check.NotNull(pagingArgs, nameof(pagingArgs));

            Skip = pagingArgs.Skip;
            Take = pagingArgs.Take;
            Page = pagingArgs.Page;
            IsGetCount = pagingArgs.IsGetCount;
            TotalCount = (totalCount.HasValue && totalCount.Value < 0) ? null : totalCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingResult"/> class from a <see cref="PagingResult"/> (copies values).
        /// </summary>
        /// <param name="pagingResult">The <see cref="PagingResult"/>.</param>
        public PagingResult(PagingResult pagingResult) : this(pagingResult, pagingResult?.TotalCount) { }

        /// <summary>
        /// Gets or sets the total count of the elements in the sequence (a <c>null</c> value indicates that the total count is unknown).
        /// </summary>
        public long? TotalCount { get; set; }

        /// <summary>
        /// Gets the calculated total pages for all elements in the sequence (needs <see cref="TotalCount"/>, <see cref="PagingArgs.Take"/> values as well as not being an <see cref="PagingArgs.IsSkipTake"/>).
        /// </summary>
        public long? TotalPages => !IsSkipTake && TotalCount.HasValue ? (long)System.Math.Ceiling(TotalCount.Value / (double)Take) : (long?)null;
    }
}