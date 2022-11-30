namespace Company.AppName.Business.Data;

/// <summary>
/// Provides the <b>Xxx</b> <see cref="TypedHttpClientCore{TSelf}"/>.
/// </summary>
public class XxxAgent : TypedMappedHttpClientCore<XxxAgent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XxxAgent"/> class.
    /// </summary>
    /// <param name="args">The <see cref="IXxxAgentArgs"/>.</param>
    public XxxAgent(HttpClient client, IMapper mapper, IJsonSerializer jsonSerializer, CoreEx.ExecutionContext executionContext, SettingsBase settings, ILogger<XxxAgent> logger)
        : base(client, mapper, jsonSerializer, executionContext, settings, logger) => DefaultOptions.WithRetry();
}