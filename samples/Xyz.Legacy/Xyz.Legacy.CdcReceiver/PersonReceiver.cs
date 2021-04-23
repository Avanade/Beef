using Beef;
using Beef.Events.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AzureServiceBus = Microsoft.Azure.ServiceBus;

namespace Xyz.Legacy.CdcReceiver
{
    public class PersonReceiver
    {
        private readonly ServiceBusReceiverHost _receiver;

        public PersonReceiver(ServiceBusReceiverHost receiver) => _receiver = Check.NotNull(receiver, nameof(receiver));

        [FunctionName(nameof(PersonReceiver))]
        [ExponentialBackoffRetry(10, "00:00:05", "00:00:30")]
        public async Task Run([ServiceBusTrigger("xyz.legacy.person", Connection = "ServiceBusConnectionString")] AzureServiceBus.Message message, ILogger logger)
            => await _receiver.UseLogger(logger).ReceiveAsync(_receiver.CreateServiceBusData(message, "xyz.legacy.person", "ServiceBusConnectionString"));
    }
}