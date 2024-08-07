/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Api.Controllers;

/// <summary>
/// Provides the <b>Config</b> Web API functionality.
/// </summary>
[Consumes(System.Net.Mime.MediaTypeNames.Application.Json)]
[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
public partial class ConfigController : ControllerBase
{
    private readonly WebApi _webApi;
    private readonly IConfigManager _manager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigController"/> class.
    /// </summary>
    /// <param name="webApi">The <see cref="WebApi"/>.</param>
    /// <param name="manager">The <see cref="IConfigManager"/>.</param>
    public ConfigController(WebApi webApi, IConfigManager manager)
        { _webApi = webApi.ThrowIfNull(); _manager = manager.ThrowIfNull(); ConfigControllerCtor(); }

    partial void ConfigControllerCtor(); // Enables additional functionality to be added to the constructor.

    /// <summary>
    /// Get Env Vars.
    /// </summary>
    /// <returns>A resultant <c>System.Collections.IDictionary</c>.</returns>
    [HttpPost("api/v1/envvars", Name="Config_GetEnvVars")]
    [ProducesResponseType(typeof(System.Collections.IDictionary), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<IActionResult> GetEnvVars()
        => _webApi.PostAsync<System.Collections.IDictionary>(Request, p => _manager.GetEnvVarsAsync(), alternateStatusCode: HttpStatusCode.NoContent, operationType: CoreEx.OperationType.Unspecified);
}

#pragma warning restore
#nullable restore