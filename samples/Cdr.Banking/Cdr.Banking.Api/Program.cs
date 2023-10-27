namespace Cdr.Banking.Api;

/// <summary>
/// The <b>WebAPI</b> host.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    public static void Main(string[] args) => Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
        .ConfigureAppConfiguration(c => c.AddEnvironmentVariables("Cdr_").AddCommandLine(args))
        .Build().Run();
}