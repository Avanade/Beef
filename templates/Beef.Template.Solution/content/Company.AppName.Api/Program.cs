using Beef.AspNetCore.WebApi;
using Microsoft.AspNetCore.Hosting;

namespace Company.AppName.Api
{
    /// <summary>
    /// The <b>Web API</b> host/program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main startup using the <i>Beef</i> <see cref="WebApiStartup"/> capability to build the host and underlying configuration probing.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        public static void Main(string[] args) => WebApiStartup.BuildWebHost<Startup>(args, "AppName").Run();
    }
}