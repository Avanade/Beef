using Beef.Events.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Beef.Demo.Functions
{
    public class ServiceBusSubscriber
    {
        private readonly ServiceBusReceiverHost _subscriber;

        public ServiceBusSubscriber(ServiceBusReceiverHost subscriber) => _subscriber = Check.NotNull(subscriber, nameof(subscriber));

        // To enable resiliency the poison invoker is used with storage to manage (within Startup.cs); the other requirement is the max retry of -1 (infinite) below - the underlying Skip will allow.
        // We also invoke the UseLogger() to get the correct logger instance into the subscriber, etc. for correct output logging.
        [FunctionName("ServiceBusSubscriber")]
        [ExponentialBackoffRetry(-1, "00:00:05", "00:00:30")] // https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-error-pages?tabs=csharp
        public async Task Run([ServiceBusTrigger("default", Connection = "ServiceBusConnectionString")] Message message, ILogger logger) 
            => await _subscriber.UseLogger(logger).ReceiveAsync(new ServiceBusData("BeefServiceBus", "default", message));
    }
}