using Beef;
using Beef.Events;
using Beef.Events.ServiceBus;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Xyz.Legacy.CdcReceiver.Startup))] // Required to ensure the Dependency Injection (DI) Startup is executed.

namespace Xyz.Legacy.CdcReceiver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Services configuration as follows:
            // - .AddBeefExecutionContext - enables the `ExecutionContext` required internally by _Beef_.
            // - .AddBeefServiceBusReceiverHost - adds the capability to encapsulate the receive orchestration Azure Service Bus messages.
            builder.Services.AddBeefExecutionContext();
            builder.Services.AddBeefServiceBusReceiverHost(EventSubscriberHostArgs.Create<Startup>());
        }
    }
}