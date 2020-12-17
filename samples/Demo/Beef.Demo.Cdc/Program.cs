using Beef.Data.Database;
using Beef.Demo.Cdc.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beef.Demo.Cdc
{
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
                    services.AddBeefExecutionContext();
                    services.AddBeefDatabaseServices(() => new Database("Data Source=.;Initial Catalog=Beef.Demo;Integrated Security=True"));
                    services.AddBeefNullEventPublisher();
                    services.AddScoped<PeopleCdcData>();
                    services.AddHostedService<Worker>();
                });
    }
}