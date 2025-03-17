namespace AIChat.AOAI.Business;

/// <summary>
/// Provides the <see cref="IConfiguration"/> settings.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/>.</param>
public class AOAISettings(IConfiguration configuration) : SettingsBase(configuration, ["AOAI/", "Common/"])
{

    /// <summary>
    /// Gets the CosmosDB connection string.
    /// </summary>
    public string CosmosConnectionString => GetRequiredValue<string>();

    /// <summary>
    /// Gtes the CosmosDB database identifier.
    /// </summary>
    public string CosmosDatabaseId => GetRequiredValue<string>();
}