using Beef.Database.Core;
using System.Threading.Tasks;

namespace Beef.Demo.Database
{
    public class Program
    {
        static Task<int> Main(string[] args)
        {
            return DatabaseConsoleWrapper
                .Create("Data Source=.;Initial Catalog=Beef.Test;Integrated Security=True", "Beef", "Demo")
                .DatabaseScript("DatabaseWithCdc.yaml")
                .RunAsync(args);
        }
    }
}