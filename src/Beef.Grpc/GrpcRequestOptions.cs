// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Grpc
{
    /// <summary>
    /// Represents additional (optional) request options for a gRPC request.
    /// </summary>
    public class GrpcRequestOptions
    {
        /// <summary>
        /// Gets or sets the entity tag that will be passed as either a <c>If-None-Match</c> header where performing a Get operation; otherwise, an <c>If-Match</c> header.
        /// </summary>
        public string? ETag { get; set; }
    }
}
