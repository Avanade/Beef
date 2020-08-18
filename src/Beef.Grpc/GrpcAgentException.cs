// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Grpc
{
    /// <summary>
    /// Represents an gRPC service agent invocation <see cref="Exception"/>.
    /// </summary>
    public class GrpcAgentException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="GrpcAgentException"/> class.
        /// </summary>
        public GrpcAgentException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentException"/> class with the specified <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public GrpcAgentException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentException"/> class with the specified <paramref name="message"/> and <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public GrpcAgentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
