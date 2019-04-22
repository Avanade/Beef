// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Wraps a <b>Web API Controller invoke</b> enabling standard functionality to be added to all invocations.
    /// </summary>
    [DebuggerStepThrough()]
    public class WebApiControllerInvoker : InvokerBase<WebApiControllerInvoker, ControllerBase> { }

    /// <summary>
    /// Wraps a <b>Web API Controller invoke</b> enabling standard functionality to be added to all invocations.
    /// </summary>
    [DebuggerStepThrough()]
    public class WebApiControllerInvoker<TResult> : InvokerBase<WebApiControllerInvoker<TResult>, ControllerBase, TResult> { }
}