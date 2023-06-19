/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace MyEf.Hr.Api.Controllers;

/// <summary>
/// Provides the <see cref="PerformanceReview"/> Web API functionality.
/// </summary>
[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
public partial class PerformanceReviewController : ControllerBase
{
    private readonly WebApi _webApi;
    private readonly IPerformanceReviewManager _manager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewController"/> class.
    /// </summary>
    /// <param name="webApi">The <see cref="WebApi"/>.</param>
    /// <param name="manager">The <see cref="IPerformanceReviewManager"/>.</param>
    public PerformanceReviewController(WebApi webApi, IPerformanceReviewManager manager)
        { _webApi = webApi.ThrowIfNull(); _manager = manager.ThrowIfNull(); PerformanceReviewControllerCtor(); }

    partial void PerformanceReviewControllerCtor(); // Enables additional functionality to be added to the constructor.

    /// <summary>
    /// Gets the specified <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    /// <returns>The selected <see cref="PerformanceReview"/> where found.</returns>
    [HttpGet("reviews/{id}")]
    [ProducesResponseType(typeof(Common.Entities.PerformanceReview), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public Task<IActionResult> Get(Guid id)
        => _webApi.GetWithResultAsync<PerformanceReview?>(Request, p => _manager.GetAsync(id));

    /// <summary>
    /// Gets the <see cref="PerformanceReviewCollectionResult"/> that contains the items that match the selection criteria.
    /// </summary>
    /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
    /// <returns>The <see cref="PerformanceReviewCollection"/></returns>
    [HttpGet("employees/{employeeId}/reviews")]
    [Paging]
    [ProducesResponseType(typeof(Common.Entities.PerformanceReviewCollection), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<IActionResult> GetByEmployeeId(Guid employeeId)
        => _webApi.GetWithResultAsync<PerformanceReviewCollectionResult>(Request, p => _manager.GetByEmployeeIdAsync(employeeId, p.RequestOptions.Paging), alternateStatusCode: HttpStatusCode.NoContent);

    /// <summary>
    /// Creates a new <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="employeeId">The <see cref="Employee.Id"/>.</param>
    /// <returns>The created <see cref="PerformanceReview"/>.</returns>
    [HttpPost("employees/{employeeId}/reviews")]
    [AcceptsBody(typeof(Common.Entities.PerformanceReview))]
    [ProducesResponseType(typeof(Common.Entities.PerformanceReview), (int)HttpStatusCode.Created)]
    public Task<IActionResult> Create(Guid employeeId)
        => _webApi.PostWithResultAsync<PerformanceReview, PerformanceReview>(Request, p => _manager.CreateAsync(p.Value!, employeeId), statusCode: HttpStatusCode.Created, locationUri: r => new Uri($"/reviews/{r.Id}", UriKind.Relative));

    /// <summary>
    /// Updates an existing <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    /// <returns>The updated <see cref="PerformanceReview"/>.</returns>
    [HttpPut("reviews/{id}")]
    [AcceptsBody(typeof(Common.Entities.PerformanceReview))]
    [ProducesResponseType(typeof(Common.Entities.PerformanceReview), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Update(Guid id)
        => _webApi.PutWithResultAsync<PerformanceReview, PerformanceReview>(Request, p => _manager.UpdateAsync(p.Value!, id));

    /// <summary>
    /// Patches an existing <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    /// <returns>The patched <see cref="PerformanceReview"/>.</returns>
    [HttpPatch("reviews/{id}")]
    [AcceptsBody(typeof(Common.Entities.PerformanceReview), HttpConsts.MergePatchMediaTypeName)]
    [ProducesResponseType(typeof(Common.Entities.PerformanceReview), (int)HttpStatusCode.OK)]
    public Task<IActionResult> Patch(Guid id)
        => _webApi.PatchWithResultAsync<PerformanceReview>(Request, get: _ => _manager.GetAsync(id), put: p => _manager.UpdateAsync(p.Value!, id));

    /// <summary>
    /// Deletes the specified <see cref="PerformanceReview"/>.
    /// </summary>
    /// <param name="id">The <see cref="Employee"/> identifier.</param>
    [HttpDelete("reviews/{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<IActionResult> Delete(Guid id)
        => _webApi.DeleteWithResultAsync(Request, p => _manager.DeleteAsync(id));
}

#pragma warning restore
#nullable restore