// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Grpc.Core;

namespace Beef.Grpc
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TParam}"/> enabling standard functionality to be added to all <see cref="GrpcAgentBase{TClient}"/> invocations.
    /// </summary>
    public class GrpcAgentInvoker : InvokerBase<ClientBase>
    {
        /// <summary>
        /// Gets the current configured instance (see <see cref="ExecutionContext.ServiceProvider"/>).
        /// </summary>
        public static GrpcAgentInvoker Current => GetCurrentInstance<GrpcAgentInvoker>(false) ?? new GrpcAgentInvoker();
    }
}