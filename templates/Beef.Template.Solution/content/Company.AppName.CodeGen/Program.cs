using Beef.CodeGen;

namespace Company.AppName.CodeGen
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
        public static int Main(string[] args)
        {
            return CodeGenConsoleWrapper.Create("Company", "AppName").Supports(entity: true, refData: true).Run(args);
        }
    }
}