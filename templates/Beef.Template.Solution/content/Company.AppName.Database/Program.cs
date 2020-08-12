﻿using Beef.Database.Core;
using System.Threading.Tasks;

namespace Company.AppName.Database
{
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
        static Task<int> Main(string[] args)
        {
            var outDir = $".{System.IO.Path.DirectorySeparatorChar}..";

#if (implement_database)
            return DatabaseConsoleWrapper.Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True", "Company", "AppName", useBeefDbo: true, outDir: outDir).RunAsync(args);
#endif
#if (implement_entityframework)
            return DatabaseConsoleWrapper.Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True", "Company", "AppName", useBeefDbo: true, outDir: outDir).RunAsync(args);
#endif
        }
    }
}