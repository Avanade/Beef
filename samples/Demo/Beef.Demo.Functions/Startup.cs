﻿using Beef.Caching.Policy;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.DataSvc;
using Beef.Entities;
using Beef.Events;
using Beef.Events.Subscribe;
using Beef.Events.Subscribe.EventHubs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                            .AddBeefCachePolicyManager(config.GetSection("BeefCaching").Get<CachePolicyConfig>())
                            .AddBeefBusinessServices();

            // Add event subscriber host with auto-discovered subscribers and set the audit writer.
            builder.Services.AddBeefEventHubSubscriberHost(EventSubscriberHostArgs.Create<Startup>(), additional: (sp, ehsh) => ehsh.UseAuditWriter(new EventHubsAzureStorageRepository(config.GetWebJobsConnectionString(ConnectionStringNames.Storage)));

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
            builder.Services.AddBeefEventHubEventPublisher(config.GetValue<string>("EventHubConnectionString"));

            // Add logging.
            builder.Services.AddLogging();
        }
    }
}