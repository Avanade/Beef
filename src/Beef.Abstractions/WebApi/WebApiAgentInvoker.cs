// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.WebApi
{
    /// <summary>
    /// Wraps a <b>WebApi Agent</b> invocation enabling standard functionality to be added to all invocations. 
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class WebApiAgentInvoker : InvokerBase<object>
    {
        /// <summary>
        /// Gets the current configured instance (see <see cref="ExecutionContext.ServiceProvider"/>).
        /// </summary>
        public static WebApiAgentInvoker Current => GetCurrentInstance<WebApiAgentInvoker>(false) ?? new WebApiAgentInvoker();
    }
}