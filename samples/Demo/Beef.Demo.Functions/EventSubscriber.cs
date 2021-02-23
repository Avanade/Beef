using Beef.Events.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using EventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Demo.Functions
{
    public class EventSubscriber
    {
        private readonly EventHubSubscriberHost _subscriber;

        public EventSubscriber(EventHubSubscriberHost subscriber) => _subscriber = Check.NotNull(subscriber, nameof(subscriber));

        // To enable resiliency the poison invoker is used with storage to manage (within Startup.cs); the other requirement is the max retry of -1 (infinite) below - the underlying Skip will allow.
        // We also invoke the UseLogger() to get the correct logger instance into the subscriber, etc. for correct output logging.
        [FunctionName("EventSubscriber")]
        [ExponentialBackoffRetry(-1, "00:00:05", "00:00:30")] // https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-error-pages?tabs=csharp
        public async Task Run([EventHubTrigger("testhub", Connection = "EventHubConnectionString")] EventHubs.EventData @event, ILogger logger, PartitionContext partitionContext) 
            => await _subscriber.UseLogger(logger).ReceiveAsync(partitionContext, @event);
    }
}