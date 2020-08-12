using Beef.Database.Core;
using System.Threading.Tasks;

namespace Beef.Demo.Database
{
    public class Program
    {
        static Task<int> Main(string[] args)
        {
            var outDir = $".{System.IO.Path.DirectorySeparatorChar}..";
            return DatabaseConsoleWrapper.Create("Data Source=.;Initial Catalog=Beef.Demo;Integrated Security=True", "Beef", "Demo", outDir).RunAsync(args);
        }
    }
}