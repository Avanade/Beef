namespace Company.AppName.Business;

/// <summary>
/// Provides the <see cref="IConfiguration"/> settings.
/// </summary>
public class AppNameSettings : SettingsBase
{
    /// <summary>
    /// Gets the setting prefixes in order of precedence.
    /// </summary>
    public static string[] Prefixes { get; } = { "AppName/", "Common/" };

    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameSettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public AppNameSettings(IConfiguration configuration) : base(configuration, Prefixes) => ValidationArgs.DefaultUseJsonNames = true;
#if (implement_database || implement_entityframework)

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetValue<string>("ConnectionStrings__Database");
#endif
#if (implement_database)

    /// <summary>
    /// Gets the CosmosDB connection string.
    /// </summary>
    public string CosmosConnectionString => GetRequiredValue<string>();

    /// <summary>
    /// Gtes the CosmosDB database identifier.
    /// </summary>
    public string CosmosDatabaseId => GetRequiredValue<string>();
#endif
}