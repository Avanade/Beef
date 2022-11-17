using Beef.Database;
using Beef.Database.SqlServer;
using System.Threading.Tasks;

namespace My.Hr.Database;

/// <summary>
/// Represents the <b>database utilities</b> program (capability).
/// </summary>
public class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
    static Task<int> Main(string[] args) => SqlServerMigrationConsole
        .Create("Data Source =.; Initial Catalog = My.Hr; Integrated Security = True; TrustServerCertificate = true", "My", "Hr")
        .Configure(c => ConfigureMigrationArgs(c.Args))
        .RunAsync(args);

    /// <summary>
    /// Configure the <see cref="MigrationArgs"/>.
    /// </summary>
    /// <param name="args">The <see cref="MigrationArgs"/>.</param>
    /// <returns>The <see cref="MigrationArgs"/>.</returns>
    public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args) => args.AddAssembly<Program>().UseBeefSchema();
}