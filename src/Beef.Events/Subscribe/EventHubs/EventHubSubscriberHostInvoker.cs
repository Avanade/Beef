﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Wraps an <see cref="EventHubSubscriberHost"/> <b>invoke</b> enabling standard functionality to be added to all invocations.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class EventHubSubscriberHostInvoker : InvokerBase<EventHubsData> { }
}