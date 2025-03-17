namespace AIChat.AOAI.CodeGen;

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
    public static Task<int> Main(string[] args) => Beef.CodeGen.CodeGenConsole.Create("AIChat", "AOAI").Supports(entity: true, refData: true, dataModel: true).RunAsync(args);
}