// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Business
{
    /// <summary>
    /// Wraps a <b>Data invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class DataInvoker : BusinessInvokerBase
    {
        private static DataInvoker? _default;

        /// <summary>
        /// Gets the current configured instance (see <see cref="ExecutionContext.ServiceProvider"/>).
        /// </summary>
        public static DataInvoker Current => ExecutionContext.GetService<DataInvoker>() ?? (_default ??= new DataInvoker());
    }
}