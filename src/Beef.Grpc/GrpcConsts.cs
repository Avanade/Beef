// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Net;

namespace Beef.Grpc
{
    /// <summary>
    /// Provides <b>gRPC</b> constants.
    /// </summary>
    public static class GrpcConsts
    {
        /// <summary>
        /// Gets the header name for the <see cref="HttpStatusCode"/> value.
        /// </summary>
        public const string HttpStatusCodeHeaderName = "x-http-status-code";

        /// <summary>
        /// Gets the header name for the exception error type value.
        /// </summary>
        public const string ErrorTypeHeaderName = "x-error-type";

        /// <summary>
        /// Gets the header name for the exception error code value.
        /// </summary>
        public const string ErrorCodeHeaderName = "x-error-code";

        /// <summary>
        /// Gets the header name for the <see cref="MessageItemCollection"/> value.
        /// </summary>
        public const string MessagesHeaderName = "x-messages-bin";
    }
}