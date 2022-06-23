//using Beef.AspNetCore.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace My.Hr.Api
{
    /// <summary>
    /// The <b>Web API</b> host/program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main startup.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        //public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();
        public static void Main(string[] args) => Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .ConfigureAppConfiguration(c => c.AddEnvironmentVariables("Hr_"))
            .Build().Run();

        /// <summary>
        /// Creates the <see cref="IWebHostBuilder"/> using the <i>Beef</i> <see cref="WebApiStartup"/> capability to create the host with the underlying configuration probing.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        //public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebApiStartup.CreateWebHost<Startup>(args, "Hr");
    }
}