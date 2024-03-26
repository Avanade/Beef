namespace Company.AppName.Business.Data;

/// <summary>
/// Provides the <b>Xxx</b> <see cref="TypedHttpClientCore{TSelf}"/>.
/// </summary>
public class XxxAgent(HttpClient client, IMapper mapper, IJsonSerializer jsonSerializer, CoreEx.ExecutionContext executionContext) : TypedMappedHttpClientCore<XxxAgent>(client, mapper, jsonSerializer, executionContext) { };