using Beef.Data.Database;
using Beef.Demo.Cdc.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                    services.AddBeefExecutionContext();
                    services.AddBeefDatabaseServices(() => new Database("Data Source=.;Initial Catalog=Beef.Demo;Integrated Security=True"));
                    services.AddBeefNullEventPublisher();
                    services.AddScoped<PostsCdcData>();
                    services.AddHostedService<PostsCdcBackgroundService>();
                });
    }
}