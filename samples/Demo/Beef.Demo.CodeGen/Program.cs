using Beef.CodeGen;

namespace Beef.Demo.CodeGen
{
    class Program
    {
        static int Main(string[] args)
        {
            return CodeGenConsoleWrapper.Create("Beef", "Demo").Supports(entity: true, database: false, refData: true).Run(args);
        }
    }
}