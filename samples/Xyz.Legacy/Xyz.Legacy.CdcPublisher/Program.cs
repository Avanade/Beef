using Azure.Messaging.ServiceBus;
using Beef;
using Beef.Data.Database;
using Beef.Entities;
using Beef.Events.ServiceBus;
using Xyz.Legacy.CdcPublisher.Data;
using Xyz.Legacy.CdcPublisher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xyz.Legacy.CdcPublisher
{
    class Program
    {
        static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Services configuration as follows:
                    // - .AddBeefExecutionContext - enables the `ExecutionContext` required internally by _Beef_.
                    // - .AddBeefDatabaseServices - adds the `IDatabase` instance for the CDC database.
                    // - .AddSingleton<IStringIdentifierGenerator>` - used to create Global Identifiers where required; default is a GUID.
                    // - .AddGeneratedCdcDataServices` - adds the generated CDC data services; these are the primary CDC data orchestrators.
                    // - .AddGeneratedCdcHostedServices` - adds the generated CDC hosted services; enables background-style execution.

                    services.AddBeefExecutionContext();
                    services.AddBeefDatabaseServices(() => new Database(hostContext.Configuration.GetValue<string>("CdcDb")));
                    services.AddSingleton<IStringIdentifierGenerator>(new StringIdentifierGenerator());
                    services.AddGeneratedCdcDataServices();
                    services.AddGeneratedCdcHostedServices(hostContext.Configuration);

                    // - .AddBeefLoggerEventPublisher - adds an `ILogger` implementation to output the published events versus actually sending (used for debugging only).
                    // - .AddBeefEventServiceBusSender - adds the capability to publish Azure Service Bus messages(as [CloudEvents](https://cloudevents.io/)).
                    //services.AddBeefLoggerEventPublisher();
                    var sbc = new ServiceBusClient(hostContext.Configuration.GetValue<string>("ServiceBus"));
                    services.AddBeefServiceBusSender(sbc);
                });
    }
}