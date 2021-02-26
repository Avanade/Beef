using Beef.Data.Cosmos;
using Beef.Data.Database;
using Beef.Data.Database.Cdc;
using Beef.Data.EntityFrameworkCore;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.DataSvc;
using Beef.Demo.Cdc.Data;
using Beef.Demo.Cdc.Services;
using Beef.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Beef.Demo.Cdc
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host
    /// </summary>
    class Program
    {
        // Command lines args to override versus config file: dotnet run --ContactCdc:IntervalSeconds 60
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .ConfigureHostConfiguration(c => c.AddEnvironmentVariables(prefix: "BeefCdc"))
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddBeefExecutionContext()
                            .AddBeefRequestCache()
                            .AddBeefBusinessServices();

                    services.AddBeefDatabaseServices(() => new Business.Data.Database(hostContext.Configuration.GetConnectionString("BeefDemo")))
                            .AddBeefEntityFrameworkServices<EfDbContext, EfDb>()
                            .AddBeefCosmosDbServices<CosmosDb>(hostContext.Configuration.GetSection("CosmosDb"));

                    services.AddGeneratedReferenceDataManagerServices()
                            .AddGeneratedReferenceDataDataSvcServices()
                            .AddGeneratedReferenceDataDataServices();

                    services.AddGeneratedManagerServices()
                            .AddGeneratedValidationServices()
                            .AddGeneratedDataSvcServices()
                            .AddGeneratedDataServices();

                    services.AddSingleton<IGuidIdentifierGenerator, GuidIdentifierGenerator>()
                            .AddSingleton<IStringIdentifierGenerator, StringIdentifierGenerator>();

                    services.AddScoped<Common.Agents.IDemoWebApiAgentArgs>(_ => new Common.Agents.DemoWebApiAgentArgs(new System.Net.Http.HttpClient() { BaseAddress = new Uri("https://something.doesnt.matter") }));
                    services.AddScoped<Common.Agents.IPersonAgent, Common.Agents.PersonAgent>();

                    services.AddBeefLoggerEventPublisher();
                    services.AddGeneratedCdcDataServices();

                    services.AddCdcHostedService<PostsCdcBackgroundService>(hostContext.Configuration);
                    services.AddCdcHostedService<ContactCdcBackgroundService>(hostContext.Configuration);
                    services.AddCdcHostedService<PersonCdcBackgroundService>(hostContext.Configuration);
                    services.AddCdcHostedService<Person2CdcBackgroundService>(hostContext.Configuration);
                });
    }
}