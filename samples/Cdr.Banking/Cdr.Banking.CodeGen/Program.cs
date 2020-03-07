using Beef.CodeGen;
using System.Threading.Tasks;

namespace Cdr.Banking.CodeGen
{
    /// <summary>
    /// Represents the <b>code generation</b> program (capability).
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main startup.
        /// </summary>
        /// <param name="args">The startup arguments.</param>
        /// <returns>The status code whereby zero indicates success.</returns>
        public static Task<int> Main(string[] args)
        {
            // Generat the entities, reference data and internal data models.
            return CodeGenConsoleWrapper.Create("Cdr", "Banking").Supports(entity: true, refData: true, dataModel: true).RunAsync(args);
        }
    }
}