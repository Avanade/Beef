using Beef.Events.Subscribe;
using Beef.Events.Triggers;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;
using EventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Demo.Functions
{
    public class EventSubscriber
    {
        private readonly EventHubSubscriberHost _subscriber;

        public EventSubscriber(EventHubSubscriberHost subscriber)
        {
            _subscriber = Check.NotNull(subscriber, nameof(subscriber));
        }

        [FunctionName("EventSubscriber")]
        public async Task Run([ResilientEventHubTrigger("BeefEventHub")] EventHubs.EventData @event) => await _subscriber.ReceiveAsync(@event);
    }
}