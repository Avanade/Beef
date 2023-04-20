/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Api.Controllers
{
    /// <summary>
    /// Provides the <see cref="Employee"/> Web API functionality.
    /// </summary>
    [Route("employees")]
    [Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
    public partial class EmployeeController : ControllerBase
    {
        private readonly WebApi _webApi;
        private readonly IEmployeeManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeController"/> class.
        /// </summary>
        /// <param name="webApi">The <see cref="WebApi"/>.</param>
        /// <param name="manager">The <see cref="IEmployeeManager"/>.</param>
        public EmployeeController(WebApi webApi, IEmployeeManager manager)
            { _webApi = webApi ?? throw new ArgumentNullException(nameof(webApi)); _manager = manager ?? throw new ArgumentNullException(nameof(manager)); EmployeeControllerCtor(); }

        partial void EmployeeControllerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Common.Entities.Employee), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Task<IActionResult> Get(Guid id) =>
            _webApi.GetAsync<Employee?>(Request, p => _manager.GetAsync(id));

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <returns>The created <see cref="Employee"/>.</returns>
        [HttpPost("")]
        [AcceptsBody(typeof(Common.Entities.Employee))]
        [ProducesResponseType(typeof(Common.Entities.Employee), (int)HttpStatusCode.Created)]
        public Task<IActionResult> Create() =>
            _webApi.PostAsync<Employee, Employee>(Request, p => _manager.CreateAsync(p.Value!), statusCode: HttpStatusCode.Created, locationUri: r => new Uri($"/employees/{r.Id}", UriKind.Relative));

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        [HttpPut("{id}")]
        [AcceptsBody(typeof(Common.Entities.Employee))]
        [ProducesResponseType(typeof(Common.Entities.Employee), (int)HttpStatusCode.OK)]
        public Task<IActionResult> Update(Guid id) =>
            _webApi.PutAsync<Employee, Employee>(Request, p => _manager.UpdateAsync(p.Value!, id));

        /// <summary>
        /// Patches an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The patched <see cref="Employee"/>.</returns>
        [HttpPatch("{id}")]
        [AcceptsBody(typeof(Common.Entities.Employee), HttpConsts.MergePatchMediaTypeName)]
        [ProducesResponseType(typeof(Common.Entities.Employee), (int)HttpStatusCode.OK)]
        public Task<IActionResult> Patch(Guid id) =>
            _webApi.PatchAsync<Employee>(Request, get: _ => _manager.GetAsync(id), put: p => _manager.UpdateAsync(p.Value!, id));

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<IActionResult> Delete(Guid id) =>
            _webApi.DeleteAsync(Request, p => _manager.DeleteAsync(id));

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="firstName">The First Name.</param>
        /// <param name="lastName">The Last Name.</param>
        /// <param name="genders">The Genders (see <see cref="RefDataNamespace.Gender"/>).</param>
        /// <param name="startFrom">The Start From.</param>
        /// <param name="startTo">The Start To.</param>
        /// <param name="isIncludeTerminated">Indicates whether Is Include Terminated.</param>
        /// <returns>The <see cref="EmployeeBaseCollection"/></returns>
        [HttpGet("")]
        [Paging]
        [ProducesResponseType(typeof(Common.Entities.EmployeeBaseCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<IActionResult> GetByArgs(string? firstName = default, string? lastName = default, List<string>? genders = default, DateTime? startFrom = default, DateTime? startTo = default, [FromQuery(Name="includeTerminated")] bool? isIncludeTerminated = default)
        {
            var args = new EmployeeArgs { FirstName = firstName, LastName = lastName, GendersSids = genders, StartFrom = startFrom, StartTo = startTo, IsIncludeTerminated = isIncludeTerminated };
            return _webApi.GetAsync<EmployeeBaseCollectionResult>(Request, p => _manager.GetByArgsAsync(args, p.RequestOptions.Paging), alternateStatusCode: HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Terminates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        [HttpPost("{id}/terminate")]
        [AcceptsBody(typeof(Common.Entities.TerminationDetail))]
        [ProducesResponseType(typeof(Common.Entities.Employee), (int)HttpStatusCode.OK)]
        public Task<IActionResult> Terminate(Guid id) =>
            _webApi.PostAsync<TerminationDetail, Employee>(Request, p => _manager.TerminateAsync(p.Value!, id), operationType: CoreEx.OperationType.Update);
    }
}

#pragma warning restore
#nullable restore