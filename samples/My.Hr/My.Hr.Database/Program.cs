using Beef.Database.Core;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace My.Hr.Database
{
    /// <summary>
    /// Represents the <b>database utilities</b> program (capability).
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main startup.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        /// <returns>The status code whereby zero indicates success.</returns>
        static Task<int> Main(string[] args)
        {

            // read configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            // User secrets can only be used in Development - need to add a check to verify env is Dev
            // Docker container running tye is not considered Development environment - there's no access to user secrets
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            System.Console.WriteLine("Environment: {0}", env);

            IConfiguration config = builder.Build();

            // first try to get connection 
            var connectionString = config.GetConnectionString("sqlserver:MyHr") ?? "Data Source =.; Initial Catalog = My.Hr; Integrated Security = True; TrustServerCertificate = true";
            System.Console.WriteLine(connectionString);

            return DatabaseConsole.Create(connectionString, "My", "Hr", useBeefDbo: true).RunAsync(args);
        }
    }
}