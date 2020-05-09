// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Grpc
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TInvoker, TParam}"/> enabling standard functionality to be added to all <see cref="GrpcServiceBase"/> invocations.
    /// </summary>
    public class GrpcInvoker : InvokerBase<GrpcInvoker, GrpcServiceBase?> { }
}