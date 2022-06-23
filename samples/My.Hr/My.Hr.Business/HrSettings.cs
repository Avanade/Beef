using CoreEx.Configuration;
using Microsoft.Extensions.Configuration;

namespace My.Hr.Business
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
        public HrSettings(IConfiguration configuration) : base(configuration, Prefixes) { }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string DatabaseConnectionString => GetValue<string>("ConnectionStrings__Database");
    }
}