using Beef.Database.Core;
using System.Threading.Tasks;

namespace Company.AppName.Database;

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
#if (implement_database)
    public static Task<int> Main(string[] args) => DatabaseConsole.Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True;TrustServerCertificate=true", "Company", "AppName", useBeefDbo: true).RunAsync(args);
#endif
#if (implement_entityframework)
    public static Task<int> Main(string[] args) => DatabaseConsole.Create("Data Source=.;Initial Catalog=Company.AppName;Integrated Security=True;TrustServerCertificate=true", "Company", "AppName", useBeefDbo: true).RunAsync(args);
#endif
}