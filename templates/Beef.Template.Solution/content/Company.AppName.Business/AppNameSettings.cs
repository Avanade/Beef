namespace Company.AppName.Business;

/// <summary>
/// Provides the <see cref="IConfiguration"/> settings.
/// </summary>
/// <param name="configuration">The <see cref="IConfiguration"/>.</param>
public class AppNameSettings(IConfiguration configuration) : SettingsBase(configuration, ["AppName/", "Common/"])
{
#if (implement_database || implement_entityframework)

    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    public string DatabaseConnectionString => GetValue<string>("ConnectionStrings__Database");
#endif
#if (implement_cosmos)

    /// <summary>
    /// Gets the CosmosDB connection string.
    /// </summary>
    public string CosmosConnectionString => GetRequiredValue<string>();

    /// <summary>
    /// Gtes the CosmosDB database identifier.
    /// </summary>
    public string CosmosDatabaseId => GetRequiredValue<string>();
#endif
#if (implement_httpagent)

    /// <summary>
    /// Gets the XxxAgent URL.
    /// </summary>
    public string XxxAgentUrl => GetRequiredValue<string>();
#endif
#if (implement_services)

    /// <summary>
    /// Gets the Azure Service Bus connection string.
    /// </summary>
    public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");
#if (implement_database || implement_sqlserver)

    /// <summary>
    /// Gets the Azure Blob Storage connection string.
    /// </summary>
    public string StorageConnectionString => GetRequiredValue<string>("ConnectionStrings__Storage");
#endif
#endif
}