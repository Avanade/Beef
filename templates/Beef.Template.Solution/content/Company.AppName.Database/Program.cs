using Beef.Database;
using Beef.Database.SqlServer;
using System.Threading.Tasks;

namespace Company.AppName.Database;

/// <summary>
/// Represents the <b>database migration</b> program capability.
/// </summary>
public class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
    public static Task<int> Main(string[] args) => SqlServerMigrationConsole
        .Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True;TrustServerCertificate=true", "Company", "AppName")
        .Configure(c => ConfigureMigrationArgs(c.Args))
        .RunAsync(args);

    /// <summary>
    /// Configure the <see cref="MigrationArgs"/> where applicable.
    /// </summary>
    /// <param name="args">The <see cref="MigrationArgs"/>.</param>
    /// <returns>The <see cref="MigrationArgs"/>.</returns>
    /// <remarks>This is also invoked from the tests to ensure consistency of execution.</remarks>
    public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args) => args.AddAssembly<Program>().UseBeefSchema();
}