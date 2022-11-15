using Beef.Database.Core.SqlServer;
using System.Threading.Tasks;

namespace Beef.Demo.Database
{
    public class Program
    {
        static Task<int> Main(string[] args) => SqlServerMigrationConsole
            .Create("Data Source=.;Initial Catalog=Beef.Test;Integrated Security=True;TrustServerCertificate=true", "Beef", "Demo")
            .Configure(c => c.Args.AddSchemaOrder("Sec", "Ref", "Demo"))
            .RunAsync(args);
    }
}