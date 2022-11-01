namespace Beef.Demo.Business
{
    /// <summary>
    /// Provides the <see cref="IConfiguration"/> settings.
    /// </summary>
    public class DemoSettings : SettingsBase
    {
        /// <summary>
        /// Gets the setting prefixes in order of precedence.
        /// </summary>
        public static string[] Prefixes { get; } = { "Demo/", "Common/" };

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoSettings"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        public DemoSettings(IConfiguration configuration) : base(configuration, Prefixes) { }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings:Database");

        /// <summary>
        /// Gets the Cosmos DB connection string.
        /// </summary>
        public string CosmosConnectionString => GetRequiredValue<string>("Cosmos__Connection");

        /// <summary>
        /// Gets the Cosmos DB database id.
        /// </summary>
        public string CosmosDatabaseId => GetRequiredValue<string>("Cosmos__Database");

        public string ZippoAgentUrl => GetRequiredValue<string>("ZippoAgentUrl");
    }
}
