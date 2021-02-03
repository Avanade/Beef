using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.DataSvc;
using Beef.Demo.Cdc.Data;
using Beef.Demo.Cdc.Services;
using Beef.Entities;
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
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddBeefExecutionContext()
                            .AddBeefRequestCache()
                            .AddBeefBusinessServices();

                    services.AddBeefDatabaseServices(() => new Business.Data.Database("Data Source=.;Initial Catalog=Beef.Demo;Integrated Security=True"))
                            .AddBeefEntityFrameworkServices<EfDbContext, EfDb>();

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

                    services.AddHostedService<PostsCdcBackgroundService>();
                    services.AddHostedService<ContactCdcBackgroundService>();
                    services.AddHostedService<PersonCdcBackgroundService>();
                    services.AddHostedService<Person2CdcBackgroundService>();
                });
    }
}