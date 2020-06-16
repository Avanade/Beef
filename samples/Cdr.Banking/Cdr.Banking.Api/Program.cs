using Beef.AspNetCore.WebApi;
//using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.FileProviders;

namespace Cdr.Banking.Api
{
    /// <summary>
    /// The <b>WebAPI</b> host.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main startup.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        public static void Main(string[] args) => WebApiStartup.BuildWebHost<Startup>(args, "Banking").Run();

        ///// <summary>
        ///// Builds the <see cref="IWebHost"/>.
        ///// </summary>
        ///// <param name="args">The startup arguments.</param>
        ///// <returns>The <see cref="IWebHost"/> instance.</returns>
        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //       .ConfigureAppConfiguration((hostingContext, config) => WebApiStartup.ConfigurationBuilder<Startup>(config, hostingContext.HostingEnvironment, "Banking"))
        //       .UseStartup<Startup>()
        //       .Build();

        ///// <summary>
        ///// Builds the configuration probing.
        ///// </summary>
        //private static void ConfigBuilder(IConfigurationBuilder configurationBuilder, IWebHostEnvironment hostingEnvironment) =>
        //    configurationBuilder.AddJsonFile(new EmbeddedFileProvider(typeof(Program).Assembly), $"webapisettings.json", true, false)
        //        .AddJsonFile(new EmbeddedFileProvider(typeof(Program).Assembly), $"webapisettings.{hostingEnvironment.EnvironmentName}.json", true, false)
        //        .AddJsonFile("appsettings.json", true, true)
        //        .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
        //        .AddEnvironmentVariables("Banking");
    }
}