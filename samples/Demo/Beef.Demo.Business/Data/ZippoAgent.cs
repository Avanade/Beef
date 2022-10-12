using CoreEx.Http.Extended;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <b>zippopotam</b> <see cref="TypedHttpClientCore{TSelf}"/>.
    /// </summary>
    public class ZippoAgent : TypedMappedHttpClientCore<ZippoAgent>
    {
        public ZippoAgent(HttpClient client, IMapper mapper, IJsonSerializer jsonSerializer, CoreEx.ExecutionContext executionContext, SettingsBase settings, ILogger<ZippoAgent> logger)
            : base(client, mapper, jsonSerializer, executionContext, settings, logger)
        {
            DefaultOptions.WithRetry();
        }
    }
}