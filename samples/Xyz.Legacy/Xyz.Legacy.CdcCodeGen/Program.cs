using Beef.CodeGen;
using System.Threading.Tasks;

namespace Xyz.Legacy.CdcCodeGen
{
    class Program
    {
        // To run execute command line: dotnet run database
        static Task<int> Main(string[] args) => CodeGenConsole
            // Code generation configuration as follows:
            // - Create - creates the code-generator instance and sets the `Company` and `AppName` parameters.
            // - Supports - turns off the default `entity` support, and turns on `database` code-gen support only.
            // - DatabaseScript - configures the code-gen to use the `DatabaseCdcDacpac.xml` that drives the code-gen templates to be used; this is a CDC-only script designed for DACPAC output.
            // - DatabaseConnectionString - defaults the database connection string; will be overridden by command line arguments '-cs|--connectionString' or environment variable: Xyz_Legacy_ConnectionString
            .Create("Xyz", "Legacy")
            .Supports(entity: false, database: true)
            .DatabaseScript("DatabaseCdcDacpac.yaml")
            .DatabaseConnectionString("Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True")
            .RunAsync(args);
    }
}