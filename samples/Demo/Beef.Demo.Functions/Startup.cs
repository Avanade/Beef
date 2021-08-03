using Azure.Messaging.EventHubs.Producer;
using Beef.Caching.Policy;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.DataSvc;
using Beef.Entities;
using Beef.Events;
using Beef.Events.EventHubs;
using Beef.Events.ServiceBus;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Cosmos = Microsoft.Azure.Cosmos;

[assembly: FunctionsStartup(typeof(Beef.Demo.Functions.Startup))]

namespace Beef.Demo.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = builder.GetBeefConfiguration<Startup>("Beef_");

            // Add the core beef services.
            builder.Services.AddBeefExecutionContext()
                            .AddBeefSystemTime()
                            .AddBeefRequestCache()
                            .AddBeefCachePolicyManager(config.GetSection("BeefCaching").Get<CachePolicyConfig>(), new System.TimeSpan(0, 0, 30), new System.TimeSpan(0, 0, 30), useCachePolicyManagerTimer: true)
                            .AddBeefBusinessServices();

            // Add event subscriber host with auto-discovered subscribers and set the audit writer to use azure storage; plus use the poison event orchestrator/invoker.
            var ehasr = new EventHubAzureStorageRepository(config.GetConnectionString("AzureStorage"));
            builder.Services.AddBeefEventHubConsumerHost(
                EventSubscriberHostArgs.Create<Startup>().UseAuditWriter(ehasr).UseMaxAttempts(10), additional: (_, ehsh) => ehsh.UseInvoker(new EventHubConsumerHostPoisonInvoker(ehasr)));

            var sbasr = new ServiceBusAzureStorageRepository(config.GetConnectionString("AzureStorage"));
            builder.Services.AddBeefServiceBusReceiverHost(
                EventSubscriberHostArgs.Create<Startup>().UseAuditWriter(sbasr).UseMaxAttempts(10), additional: (_, ehsh) => ehsh.UseInvoker(new ServiceBusReceiverHostPoisonInvoker(sbasr)));

            // Add the data sources as singletons for dependency injection requirements.
            var ccs = config.GetSection("CosmosDb");
            builder.Services.AddScoped<Data.Database.IDatabase>(_ => new Database(config.GetConnectionString("BeefDemo")))
                            .AddDbContext<EfDbContext>()
                            .AddScoped<Data.EntityFrameworkCore.IEfDb, EfDb>()
                            .AddSingleton<Data.Cosmos.ICosmosDb>(_ => new CosmosDb(new Cosmos.CosmosClient(ccs.GetValue<string>("EndPoint"), ccs.GetValue<string>("AuthKey")), ccs.GetValue<string>("Database")));

            // Add the generated reference data services for dependency injection requirements.
            builder.Services.AddGeneratedReferenceDataManagerServices()
                            .AddGeneratedReferenceDataDataSvcServices()
                            .AddGeneratedReferenceDataDataServices();

            // Add the generated entity services for dependency injection requirements.
            builder.Services.AddGeneratedManagerServices()
                            .AddGeneratedValidationServices()
                            .AddGeneratedDataSvcServices()
                            .AddGeneratedDataServices();

            // Add identifier generator services.
            builder.Services.AddSingleton<IGuidIdentifierGenerator, GuidIdentifierGenerator>()
                            .AddSingleton<IStringIdentifierGenerator, StringIdentifierGenerator>();

            // Add event publishing.
            builder.Services.AddBeefEventHubEventProducer(new EventHubProducerClient(config.GetValue<string>("EventHubConnectionString")));

            // Add the AutoMapper profiles.
            builder.Services.AddAutoMapper(Mapper.AutoMapperProfile.Assembly, typeof(ContactData).Assembly);

            // Add logging.
            builder.Services.AddLogging();
        }
    }
}