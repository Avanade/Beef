using Microsoft.AspNetCore;
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
        public static void Main(string[] args) => WebApiStartup.BuildWebHost<Startup>(args, "Banking").Run();
    }
}