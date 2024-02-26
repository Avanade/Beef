using Beef.Database;
#if (implement_database || implement_sqlserver)
using Beef.Database.SqlServer;
#endif
#if (implement_mysql)
using Beef.Database.MySql;
#endif
#if (implement_postgres)
using Beef.Database.Postgres;
#endif
using System.Threading.Tasks;

namespace Company.AppName.Database;

/// <summary>
/// Represents the <b>database migration</b> program (capability).
/// </summary>
public class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
#if (implement_database || implement_sqlserver)
    public static Task<int> Main(string[] args) => SqlServerMigrationConsole
        .Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True;TrustServerCertificate=true", "Company", "AppName")
#endif
#if (implement_mysql)
    public static Task<int> Main(string[] args) => MySqlMigrationConsole
        .Create("Server=localhost; Port=3306; Database=Company.AppName; Uid=dbuser; Pwd=dbpassword;", "Company", "AppName")
#endif
#if (implement_postgres)
    public static Task<int> Main(string[] args) => PostgresMigrationConsole
        .Create("Server=localhost; Database=Company.AppName; Username=postgres; Password=dbpassword;", "Company", "AppName")
#endif
        .Configure(c => ConfigureMigrationArgs(c.Args))
        .RunAsync(args);

    /// <summary>
    /// Configure the <see cref="MigrationArgs"/> where applicable.
    /// </summary>
    /// <param name="args">The <see cref="MigrationArgs"/>.</param>
    /// <returns>The <see cref="MigrationArgs"/>.</returns>
    /// <remarks>This is also invoked from the unit tests to ensure consistency of execution.</remarks>
#if (implement_database)
    public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args) => args.AddAssembly<Program>().UseBeefSchema();
#endif
#if (implement_sqlserver || implement_mysql || implement_postgres)
    public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args) => args.AddAssembly<Program>();
#endif
}