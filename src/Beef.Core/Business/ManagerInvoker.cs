// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Business
{
    /// <summary>
    /// Wraps a <b>Manager invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class ManagerInvoker : BusinessInvokerBase
    {
        /// <summary>
        /// Gets the current configured instance (see <see cref="ExecutionContext.ServiceProvider"/>).
        /// </summary>
        public static ManagerInvoker Current => GetCurrentInstance<ManagerInvoker>(false) ?? new ManagerInvoker();
    }
}