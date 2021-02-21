using Beef.Events.Subscribe.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using EventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Demo.Functions
{
    public class EventSubscriber
    {
        private const string _eventHubName = "testhub";
        private readonly EventHubSubscriberHost _subscriber;

        public EventSubscriber(EventHubSubscriberHost subscriber, IConfiguration config, ILogger<EventSubscriber> logger)
        {
            // To enable reseliency the poison invoker is used with storage to manage; the other requirement is the max retry of -1 (infinite) - the underlying Skip will allow.
            var asr = new EventHubsAzureStorageRepository(new EventHubsRepositoryArgs(_eventHubName, config), logger);
            var epi = new EventHubSubscriberHostPoisonInvoker(asr);
            _subscriber = Check.NotNull(subscriber, nameof(subscriber)).UseAuditWriter(asr).UseInvoker(epi);
        }

        [FunctionName("EventSubscriber")]
        [ExponentialBackoffRetry(-1, "00:00:05", "00:00:30")] // https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-error-pages?tabs=csharp
        public async Task Run([EventHubTrigger(_eventHubName, Connection = "EventHubConnectionString")] EventHubs.EventData @event, ILogger logger, PartitionContext partitionContext) => await _subscriber.ReceiveAsync(@event);
    }
}