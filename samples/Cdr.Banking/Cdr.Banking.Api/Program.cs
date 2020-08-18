using Beef.AspNetCore.WebApi;
using Microsoft.AspNetCore.Hosting;

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
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary>
        /// Creates the <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder CreateHostBuilder(string[] args) => WebApiStartup.CreateWebHost<Startup>(args, "Banking");
    }
}