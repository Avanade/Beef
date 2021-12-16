using Beef.Database.Core;
using System.Threading.Tasks;

namespace Beef.Demo.Database
{
    public class Program
    {
        static Task<int> Main(string[] args) => DatabaseConsole
            .Create("Data Source=.;Initial Catalog=Beef.Test;Integrated Security=True;TrustServerCertificate=true", "Beef", "Demo")
            .RunAsync(args);
    }
}