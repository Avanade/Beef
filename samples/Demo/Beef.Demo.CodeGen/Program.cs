using Beef.CodeGen;
using System.Threading.Tasks;

namespace Beef.Demo.CodeGen
{
    public class Program
    {
        static Task<int> Main(string[] args) => CodeGenConsole
            .Create("Beef", "Demo")
            .Supports(entity: true, refData: true, dataModel: true)
            .EntityScript("TestScript.yaml")
            .RunAsync(args);
    }
}