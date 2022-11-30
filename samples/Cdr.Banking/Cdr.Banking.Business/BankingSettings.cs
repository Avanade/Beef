using CoreEx.Configuration;
using Microsoft.Extensions.Configuration;

namespace Cdr.Banking.Business
{
    /// <summary>
    /// Provides the <see cref="IConfiguration"/> settings.
    /// </summary>
    public class BankingSettings : SettingsBase
    {
        /// <summary>
        /// Gets the setting prefixes in order of precedence.
        /// </summary>
        public static string[] Prefixes { get; } = { "Banking/", "Common/" };

        /// <summary>
        /// Initializes a new instance of the <see cref="BankingSettings"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        public BankingSettings(IConfiguration configuration) : base(configuration, Prefixes) { }

        /// <summary>
        /// Gets the CosmosDB connection string.
        /// </summary>
        public string CosmosConnectionString => GetRequiredValue<string>();

        /// <summary>
        /// Gets the CosmosDB database identifier.
        /// </summary>
        public string CosmosDatabaseId => GetRequiredValue<string>();
    }
}