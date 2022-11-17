using Beef.Database;
using Beef.Database.SqlServer;
using System.Threading.Tasks;

namespace Beef.Demo.Database
{
    public class Program
    {
        static Task<int> Main(string[] args) => SqlServerMigrationConsole
            .Create("Data Source=.;Initial Catalog=Beef.Test;Integrated Security=True;TrustServerCertificate=true", "Beef", "Demo")
            .Configure(c => ConfigureMigrationArgs(c.Args))
            .RunAsync(args);

        /// <summary>
        /// Configure the <see cref="MigrationArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="MigrationArgs"/>.</param>
        /// <returns>The <see cref="MigrationArgs"/>.</returns>
        public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args)
        {
            args.AddAssembly<Program>();
            args.AddSchemaOrder("Sec", "Ref", "Demo");
            args.UseBeefSchema();
            return args;
        }
    }
}