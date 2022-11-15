using Beef.Database.Core.SqlServer;
using System.Threading.Tasks;

namespace Company.AppName.Database;

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
    public static Task<int> Main(string[] args) => SqlServerMigrationConsole.Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True;TrustServerCertificate=true", "Company", "AppName", useBeefSchema: true).RunAsync(args);
}