// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef
{
    /// <summary>
    /// Provides the core capabilities for the <see cref="InvokerBase{TParam}"/> and enables the <see cref="GetCurrentInstance{TInvoker}(bool)"/>.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class Invoker
    {
        /// <summary>
        /// Gets the current instance (using the <see cref="ExecutionContext.GetService{T}(bool)"/>).
        /// </summary>
        /// <typeparam name="TInvoker">The invoker <see cref="Type"/>.</typeparam>
        /// <param name="throwExceptionOnNull">Indicates whether to throw an <see cref="InvalidOperationException"/> where the underlying <see cref="IServiceProvider.GetService(Type)"/> returns <c>null</c>.</param>
        /// <returns>The <typeparamref name="TInvoker"/> instance.</returns>
        protected static TInvoker GetCurrentInstance<TInvoker>(bool throwExceptionOnNull = true) where TInvoker : Invoker => ExecutionContext.GetService<TInvoker>(throwExceptionOnNull)!;
    }
}