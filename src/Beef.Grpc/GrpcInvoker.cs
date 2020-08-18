// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Grpc
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TParam}"/> enabling standard functionality to be added to all <see cref="GrpcServiceBase"/> invocations.
    /// </summary>
    public class GrpcInvoker : InvokerBase<GrpcServiceBase?>
    {
        /// <summary>
        /// Gets the current configured instance (see <see cref="ExecutionContext.ServiceProvider"/>).
        /// </summary>
        public static GrpcInvoker Current => GetCurrentInstance<GrpcInvoker>(false) ?? new GrpcInvoker();
    }
}