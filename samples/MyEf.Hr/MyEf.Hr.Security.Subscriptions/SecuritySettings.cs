namespace MyEf.Hr.Security.Subscriptions;

/// <summary>
/// Provides the <see cref="IConfiguration"/> settings.
/// </summary>
public class SecuritySettings : SettingsBase
{
    /// <summary>
    /// Gets the setting prefixes in order of precedence.
    /// </summary>
    public static string[] Prefixes { get; } = { "Security/", "Common/" };

    /// <summary>
    /// Initializes a new instance of the <see cref="SecuritySettings"/> class.
    /// </summary>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    public SecuritySettings(IConfiguration configuration) : base(configuration, Prefixes) => ValidationArgs.DefaultUseJsonNames = true;

    /// <summary>
    /// Gets the OKTA API base URI.
    /// </summary>
    public string OktaHttpClientBaseUri => GetRequiredValue<string>("OktaHttpClient__BaseUri");
}