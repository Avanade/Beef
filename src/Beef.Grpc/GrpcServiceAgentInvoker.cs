// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Grpc.Core;

namespace Beef.Grpc
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TInvoker, TParam}"/> enabling standard functionality to be added to all <see cref="GrpcServiceAgentBase{TClient}"/> invocations.
    /// </summary>
    public class GrpcServiceAgentInvoker<TClient> : InvokerBase<GrpcServiceAgentInvoker<TClient>, GrpcServiceAgentBase<TClient>?> where TClient : ClientBase<TClient> { }
}