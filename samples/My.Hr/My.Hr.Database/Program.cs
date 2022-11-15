using Beef.Database.Core.SqlServer;
using System.Threading.Tasks;

namespace My.Hr.Database;

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
    static Task<int> Main(string[] args) => SqlServerMigrationConsole.Create("Data Source =.; Initial Catalog = My.Hr; Integrated Security = True; TrustServerCertificate = true", "My", "Hr", useBeefSchema: true).RunAsync(args);
}