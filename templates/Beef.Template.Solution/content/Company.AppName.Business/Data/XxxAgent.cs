namespace Company.AppName.Business.Data;

/// <summary>
/// Defines the <b>Xxx</b> <see cref="IHttpAgentArgs"/>.
/// </summary>
public interface IXxxAgentArgs : IHttpAgentArgs { }

/// <summary>
/// Provides the <b>Xxx</b> <see cref="IHttpAgentArgs"/>.
/// </summary>
public class XxxAgentArgs : HttpAgentArgs, IXxxAgentArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XxxAgentArgs"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
    /// <param name="beforeRequest">The optional <see cref="WebApiAgentArgs.BeforeRequest"/> action.</param>
    /// <param name="beforeRequestAsync">The optional <see cref="WebApiAgentArgs.BeforeRequestAsync"/> asynchronous function.</param>
    /// <param name="afterResponse">The optional <see cref="AfterResponse"/> action.</param>
    /// <param name="afterResponseAsync">The optional <see cref="AfterResponseAsync"/> asynchronous function.</param>
    public XxxAgentArgs(HttpClient httpClient, Action<HttpRequestMessage>? beforeRequest = null, Func<HttpRequestMessage, Task>? beforeRequestAsync = null, Action<IHttpAgentResult>? afterResponse = null, Func<IHttpAgentResult, Task>? afterResponseAsync = null)
        : base(httpClient, beforeRequest, beforeRequestAsync, afterResponse, afterResponseAsync) { }
}

/// <summary>
/// Defines the <b>Xxx</b> <see cref="IHttpAgentArgs"/>.
/// </summary>
public interface IXxxAgent : IHttpAgent { }

/// <summary>
/// Provides the <b>Xxx</b> <see cref="HttpAgentBase"/>.
/// </summary>
public class XxxAgent : HttpAgentBase, IXxxAgent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XxxAgent"/> class.
    /// </summary>
    /// <param name="args">The <see cref="IXxxAgentArgs"/>.</param>
    public XxxAgent(IXxxAgentArgs args) : base(args) { }
}