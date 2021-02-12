// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Wraps an <see cref="EventDataSubscriberHost"/> <b>invoke</b> enabling standard functionality to be added to all invocations.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class EventDataSubscriberHostInvoker : InvokerBase<EventData> { }
}