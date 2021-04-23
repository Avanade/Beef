using Beef.CodeGen;
using System.Threading.Tasks;

namespace Xyz.Legacy.CdcCodeGen
{
    class Program
    {
        // Uses the connection string defined in Environment Variable: Xyz_Legacy_ConnectionString
        // To run execute command line: dotnet run database
        static Task<int> Main(string[] args) => CodeGenConsoleWrapper
            // Code generation configuration as follows:
            // - Create - creates the code-generator instance and sets the `Company` and `AppName` parameters.
            // - Supports - turns off the default `entity` support, and turns on `database` code-gen support only.
            // - DatabaseScript - configures the code-gen to use the `DatabaseCdcDacpac.xml` that drives the code-gen templates to be used; this is a CDC-only script designed for DACPAC output.
            .Create("Xyz", "Legacy")
            .Supports(entity: false, database: true)
            .DatabaseScript("DatabaseCdcDacpac.xml")
            .RunAsync(args);
    }
}