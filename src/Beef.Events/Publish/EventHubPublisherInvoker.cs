// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using EventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.Publish
{
    /// <summary>
    /// The publisher (<see cref="EventHubs.EventHubClient.SendAsync(EventHubs.EventData, string)"/>) invoker. 
    /// </summary>
    public class EventHubPublisherInvoker : InvokerBase<object> { }
}