using Beef.Database.Core;
using System;

namespace Beef.Demo.Database
{
    public class Program
    {
        static int Main(string[] args)
        {
            return DatabaseConsoleWrapper.Create("Data Source=.;Initial Catalog=Beef.Demo;Integrated Security=True", "Beef", "Demo").Run(args);
        }
    }
}
