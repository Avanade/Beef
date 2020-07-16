// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Grpc.Core;

namespace Beef.Grpc
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TInvoker, TParam}"/> enabling standard functionality to be added to all <see cref="GrpcAgentBase{TClient}"/> invocations.
    /// </summary>
    public class GrpcAgentInvoker<TClient> : InvokerBase<GrpcAgentInvoker<TClient>, GrpcAgentBase<TClient>?> where TClient : ClientBase<TClient> { }
}