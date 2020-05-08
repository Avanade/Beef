// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;

namespace Beef.Grpc
{
    /// <summary>
    /// Wrapper for the <see cref="MessageItemCollection"/> providing a <see cref="Messages"/> property.
    /// </summary>
    public class GrpcMessages
    {
        /// <summary>
        /// Gets of sets the <see cref="MessageItemCollection"/>.
        /// </summary>
#pragma warning disable CA2227 // Collection properties should be read only; used as a DTO therefore get/set is valid.
        public MessageItemCollection? Messages { get; set; }
#pragma warning restore CA2227
    }
}
