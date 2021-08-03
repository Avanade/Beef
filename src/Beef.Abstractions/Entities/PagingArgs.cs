// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Entities
{
    /// <summary>
    /// Represents either position-based paging being (<see cref="Page"/> and <see cref="Size"/>), or <see cref="Skip"/> and <see cref="Take"/>. The <see cref="DefaultTake"/> 
    /// and <see cref="MaxTake"/> (and <see cref="DefaultIsGetCount"/>) are system-wide settings to encourage page-size consistency, as well as limit the maximum value possible. 
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class PagingArgs
    {
        private static long _defaultTake = 100;
        private static long _maxTake = 1000;

        /// <summary>
        /// Gets or sets the default <see cref="Take"/> size (defaults to 100).
        /// </summary>
        public static long DefaultTake
        {
            get { return _defaultTake; }

            set
            {
                if (value > 0)
                    _defaultTake = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum <see cref="Take"/> size allowed (defaults to 1000).
        /// </summary>
        public static long MaxTake
        {
            get { return _maxTake; }

            set
            {
                if (value > 0)
                    _maxTake = value;
            }
        }

        /// <summary>
        /// Gets or sets the default <see cref="IsGetCount"/> (defaults to <c>false</c>).
        /// </summary>
        public static bool DefaultIsGetCount { get; set; }

        /// <summary>
        /// Creates a <see cref="PagingArgs"/> for a specified page number and size.
        /// </summary>
        /// <param name="page">The <see cref="Page"/> number.</param>
        /// <param name="size">The page <see cref="Size"/> (defaults to <see cref="DefaultTake"/>).</param>
        /// <param name="isGetCount">Indicates whether to get the total count (see <see cref="PagingResult.TotalCount"/>) when performing the underlying query (defaults to <see cref="DefaultIsGetCount"/> where <c>null</c>).</param>
        /// <returns>The <see cref="PagingArgs"/>.</returns>
        public static PagingArgs CreatePageAndSize(long page, long? size = null, bool? isGetCount = null)
        {
            var pa = new PagingArgs
            {
                Page = page < 0 ? 1 : page,
                Take = !size.HasValue || size.Value < 1 ? DefaultTake : (size.Value > MaxTake ? MaxTake : size.Value),
                IsGetCount = isGetCount == null ? DefaultIsGetCount : isGetCount.Value
            };

            pa.Skip = (pa.Page.Value - 1) * pa.Size;
            return pa;
        }

        /// <summary>
        /// Creates a <see cref="PagingArgs"/> for a specified skip and take.
        /// </summary>
        /// <param name="skip">The <see cref="Skip"/> value.</param>
        /// <param name="take">The <see cref="Take"/> value (defaults to <see cref="DefaultTake"/>).</param>
        /// <param name="isGetCount">Indicates whether to get the total count (see <see cref="PagingResult.TotalCount"/>) when performing the underlying query (defaults to <see cref="DefaultIsGetCount"/> where <c>null</c>).</param>
        /// <returns>The <see cref="PagingArgs"/>.</returns>
        public static PagingArgs CreateSkipAndTake(long skip, long? take = null, bool? isGetCount = null)
        {
            return new PagingArgs
            {
                Skip = skip < 0 ? 0 : skip,
                Take = !take.HasValue || take.Value < 1 ? DefaultTake : (take.Value > MaxTake ? MaxTake : take.Value),
                IsGetCount = isGetCount == null ? DefaultIsGetCount : isGetCount.Value
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingArgs"/> class with default <see cref="Skip"/> and <see cref="Take"/>.
        /// </summary>
        public PagingArgs()
        {
            Skip = 0;
            Take = DefaultTake;
            IsGetCount = DefaultIsGetCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingArgs"/> class copying the values from <paramref name="pagingArgs"/>.
        /// </summary>
        /// <param name="pagingArgs">The <see cref="PagingArgs"/> to copy from.</param>
        public PagingArgs(PagingArgs pagingArgs)
        {
            if (pagingArgs == null)
                throw new ArgumentNullException(nameof(pagingArgs));

            Skip = pagingArgs.Skip;
            Take = pagingArgs.Take;
            Page = pagingArgs.Page;
            IsGetCount = pagingArgs.IsGetCount;
        }

        /// <summary>
        /// Gets page number for the elements in a sequence to select (see <see cref="CreatePageAndSize(long, long?, bool?)"/>).
        /// </summary>
        public long? Page { get; internal protected set; }

        /// <summary>
        /// Indicates whether the paging was created with a <see cref="Skip"/> and <see cref="Take"/> (see <see cref="CreateSkipAndTake(long, long?, bool?)"/>); versus <see cref="Page"/> and <see cref="Size"/> (see <see cref="CreatePageAndSize(long, long?, bool?)"/>).
        /// </summary>
        public bool IsSkipTake => Page == null;

        /// <summary>
        /// Gets the page size (see <see cref="Take"/>).
        /// </summary>
        public long Size => Take;

        /// <summary>
        /// Gets the specified number of elements in a sequence to bypass.
        /// </summary>
        public long Skip { get; internal protected set; }

        /// <summary>
        /// Gets the specified number of contiguous elements from the start of a sequence.
        /// </summary>
        public long Take { get; internal protected set; }

        /// <summary>
        /// Overrides/updates the <see cref="Skip"/> value.
        /// </summary>
        /// <param name="skip">The new skip value.</param>
        /// <returns>The <see cref="PagingArgs"/> instance to support fluent-style method chaining.</returns>
        public PagingArgs OverrideSkip(long skip)
        {
            if (skip == Skip)
                return this;

            Skip = skip < 0 ? 0 : skip;
            return this;
        }

        /// <summary>
        /// Overrides/updates the <see cref="Take"/> value bypassing the <see cref="MaxTake"/> checking.
        /// </summary>
        /// <param name="take">The new take value.</param>
        /// <returns>The <see cref="PagingArgs"/> instance to support fluent-style method chaining.</returns>
        public PagingArgs OverrideTake(long take)
        {
            Take = take < 0 ? 0 : take;
            return this;
        }

        /// <summary>
        /// Indicates whether to get the total count (see <see cref="PagingResult.TotalCount"/>) when performing the underlying query (defaults to <c>false</c>).
        /// </summary>
        public bool IsGetCount { get; set; } = false;
    }
}