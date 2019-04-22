// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Business
{
    /// <summary>
    /// Wraps a <b>Data Service invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    public class DataSvcInvoker : BusinessInvokerBase<DataSvcInvoker> { }

    /// <summary>
    /// Wraps a <b>Data Service invoke</b> enabling standard <b>business tier</b> functionality to be added to all invocations.
    /// </summary>
    public class DataSvcInvoker<TResult> : BusinessInvokerBase<DataSvcInvoker<TResult>, TResult> { }
}