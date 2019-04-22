// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Business
{

    /// <summary>
    /// Wraps a <b>Manager invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    public class ManagerInvoker : BusinessInvokerBase<ManagerInvoker> { }

    /// <summary>
    /// Wraps a <b>Manager invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    public class ManagerInvoker<TResult> : BusinessInvokerBase<ManagerInvoker<TResult>, TResult> { }
}
