using System.Threading.Tasks;
using Beef.Events.Subscribe;
using Beef.Events.Triggers;
using EventHubs = Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Beef.Demo.Functions
{
    public static class EventSubscriber
    {
        [FunctionName("EventSubscriber")]
        public static async Task Run([ResilientEventHubTrigger] EventHubs.EventData @event, ILogger log)
        {
            await EventHubSubscriberHost.Create(log).ReceiveAsync(@event);
        }
    }
}