// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Grpc
{
    /// <summary>
    /// Provides <b>gRPC</b> constants.
    /// </summary>
    public static class GrpcConsts
    {
        /// <summary>
        /// Gets the header name for the exception error type value.
        /// </summary>
        public const string ErrorTypeHeaderName = "x-error-type";

        /// <summary>
        /// Gets the header name for the exception error code value.
        /// </summary>
        public const string ErrorCodeHeaderName = "x-error-code";
    }
}