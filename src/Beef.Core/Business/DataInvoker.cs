// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Business
{
    /// <summary>
    /// Wraps a <b>Data invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    public class DataInvoker : BusinessInvokerBase<DataInvoker> { }

    /// <summary>
    /// Wraps a <b>Data invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    public class DataInvoker<TResult> : BusinessInvokerBase<DataInvoker<TResult>, TResult> { }
}