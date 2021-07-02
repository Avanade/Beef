// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides Web API constants.
    /// </summary>
    public static class WebApiConsts
    {
        /// <summary>
        /// Gets the header name for the exception error type value.
        /// </summary>
        public const string ErrorTypeHeaderName = "x-error-type";

        /// <summary>
        /// Gets the header name for the exception error code value.
        /// </summary>
        public const string ErrorCodeHeaderName = "x-error-code";

        /// <summary>
        /// Gets the header name for the <see cref="PagingResult"/> <see cref="PagingArgs.Page"/>.
        /// </summary>
        public const string PagingPageNumberHeaderName = "x-paging-page-number";

        /// <summary>
        /// Gets the header name for the <see cref="PagingResult"/> <see cref="PagingArgs.Take"/>.
        /// </summary>
        public const string PagingPageSizeHeaderName = "x-paging-page-size";

        /// <summary>
        /// Gets the header name for the <see cref="PagingResult"/> <see cref="PagingArgs.Skip"/>.
        /// </summary>
        public const string PagingSkipHeaderName = "x-paging-skip";

        /// <summary>
        /// Gets the header name for the <see cref="PagingResult"/> <see cref="PagingArgs.Take"/>.
        /// </summary>
        public const string PagingTakeHeaderName = "x-paging-take";

        /// <summary>
        /// Gets the header name for the <see cref="PagingResult"/> <see cref="PagingResult.TotalCount"/>.
        /// </summary>
        public const string PagingTotalCountHeaderName = "x-paging-total-count";

        /// <summary>
        /// Gets the header name for the <see cref="PagingResult"/> <see cref="PagingResult.TotalPages"/>.
        /// </summary>
        public const string PagingTotalPagesHeaderName = "x-paging-total-pages";

        /// <summary>
        /// Gets the header name for the messages.
        /// </summary>
        public const string MessagesHeaderName = "x-messages";

        /// <summary>
        /// Gets the header name for the <see cref="ExecutionContext.CorrelationId"/>.
        /// </summary>
        public const string CorrelationIdHeaderName = "x-correlation-id";
    }
}