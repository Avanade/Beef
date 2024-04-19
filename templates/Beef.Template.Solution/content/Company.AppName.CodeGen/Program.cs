namespace Company.AppName.CodeGen;

/// <summary>
/// Represents the <b>code generation</b> program (capability).
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
#if (!implement_cosmos)
#if (implement_none)
    public static Task<int> Main(string[] args) => Beef.CodeGen.CodeGenConsole.Create("Company", "AppName").Supports(entity: true, refData: false).RunAsync(args);
#else
    public static Task<int> Main(string[] args) => Beef.CodeGen.CodeGenConsole.Create("Company", "AppName").Supports(entity: true, refData: true).RunAsync(args);
#endif
#endif
#if (implement_cosmos)
    public static Task<int> Main(string[] args) => Beef.CodeGen.CodeGenConsole.Create("Company", "AppName").Supports(entity: true, refData: true, dataModel: true).RunAsync(args);
#endif
}