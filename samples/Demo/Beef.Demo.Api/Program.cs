using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Beef.Demo.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
               .ConfigureAppConfiguration((hostingContext, config) => ConfigBuilder(config, hostingContext.HostingEnvironment))
               .UseStartup<Startup>()
               .Build();

        private static void ConfigBuilder(IConfigurationBuilder configurationBuilder, IWebHostEnvironment hostingEnvironment) =>
            configurationBuilder.AddJsonFile(new EmbeddedFileProvider(typeof(Program).Assembly), $"webapisettings.json", true, false)
                .AddJsonFile(new EmbeddedFileProvider(typeof(Program).Assembly), $"webapisettings.{hostingEnvironment.EnvironmentName}.json", true, false)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables("Beef_");
    }
}