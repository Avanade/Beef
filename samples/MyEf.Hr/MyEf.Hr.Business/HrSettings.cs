namespace MyEf.Hr.Business
{
    /// <summary>
    /// Provides the <see cref="IConfiguration"/> settings.
    /// </summary>
    public class HrSettings : SettingsBase
    {
        /// <summary>
        /// Gets the setting prefixes in order of precedence.
        /// </summary>
        public static string[] Prefixes { get; } = { "Hr/", "Common/" };

        /// <summary>
        /// Initializes a new instance of the <see cref="HrSettings"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        public HrSettings(IConfiguration configuration) : base(configuration, Prefixes) => ValidationArgs.DefaultUseJsonNames = true;

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string DatabaseConnectionString => GetRequiredValue<string>("ConnectionStrings__Database");

        /// <summary>
        /// Gets the Azure service bus connection string.
        /// </summary>
        public string ServiceBusConnectionString => GetRequiredValue<string>("ConnectionStrings__ServiceBus");

        /// <summary>
        /// Gets the Azure storage connection string.
        /// </summary>
        public string StorageConnectionString => GetRequiredValue<string>("ConnectionStrings__Storage");
    }
}