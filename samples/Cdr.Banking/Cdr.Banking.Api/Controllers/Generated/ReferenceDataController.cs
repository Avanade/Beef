/*
 * This file is automatically generated; any changes will be lost. 
 */
 
#nullable enable
#pragma warning disable

using CommonRefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Api.Controllers
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> Web API functionality.
    /// </summary>
    public partial class ReferenceDataController : ControllerBase
    {
        private readonly ReferenceDataContentWebApi _webApi;
        private readonly ReferenceDataOrchestrator _orchestrator;

        public ReferenceDataController(ReferenceDataContentWebApi webApi, ReferenceDataOrchestrator orchestrator)
        {
            _webApi = webApi ?? throw new ArgumentNullException(nameof(webApi));
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        /// <summary> 
        /// Gets all of the <see cref="RefDataNamespace.OpenStatus"/> reference data items that match the specified criteria.
        /// </summary>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>A RefDataNamespace.OpenStatus collection.</returns>
        [HttpGet()]
        [Route("api/v1/ref/openStatuses")]
        [ProducesResponseType(typeof(IEnumerable<CommonRefDataNamespace.OpenStatus>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> OpenStatusGetAll([FromQuery] IEnumerable<string>? codes = default, string? text = default)
            => _webApi.GetAsync(Request, p => _orchestrator.GetWithFilterAsync<RefDataNamespace.OpenStatus>(codes, text, p.RequestOptions.IncludeInactive));

        /// <summary> 
        /// Gets all of the <see cref="RefDataNamespace.ProductCategory"/> reference data items that match the specified criteria.
        /// </summary>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>A RefDataNamespace.ProductCategory collection.</returns>
        [HttpGet()]
        [Route("api/v1/ref/productCategories")]
        [ProducesResponseType(typeof(IEnumerable<CommonRefDataNamespace.ProductCategory>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> ProductCategoryGetAll([FromQuery] IEnumerable<string>? codes = default, string? text = default)
            => _webApi.GetAsync(Request, p => _orchestrator.GetWithFilterAsync<RefDataNamespace.ProductCategory>(codes, text, p.RequestOptions.IncludeInactive));

        /// <summary> 
        /// Gets all of the <see cref="RefDataNamespace.AccountUType"/> reference data items that match the specified criteria.
        /// </summary>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>A RefDataNamespace.AccountUType collection.</returns>
        [HttpGet()]
        [Route("api/v1/ref/accountUTypes")]
        [ProducesResponseType(typeof(IEnumerable<CommonRefDataNamespace.AccountUType>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> AccountUTypeGetAll([FromQuery] IEnumerable<string>? codes = default, string? text = default)
            => _webApi.GetAsync(Request, p => _orchestrator.GetWithFilterAsync<RefDataNamespace.AccountUType>(codes, text, p.RequestOptions.IncludeInactive));

        /// <summary> 
        /// Gets all of the <see cref="RefDataNamespace.MaturityInstructions"/> reference data items that match the specified criteria.
        /// </summary>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>A RefDataNamespace.MaturityInstructions collection.</returns>
        [HttpGet()]
        [Route("api/v1/ref/maturityInstructions")]
        [ProducesResponseType(typeof(IEnumerable<CommonRefDataNamespace.MaturityInstructions>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> MaturityInstructionsGetAll([FromQuery] IEnumerable<string>? codes = default, string? text = default)
            => _webApi.GetAsync(Request, p => _orchestrator.GetWithFilterAsync<RefDataNamespace.MaturityInstructions>(codes, text, p.RequestOptions.IncludeInactive));

        /// <summary> 
        /// Gets all of the <see cref="RefDataNamespace.TransactionType"/> reference data items that match the specified criteria.
        /// </summary>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>A RefDataNamespace.TransactionType collection.</returns>
        [HttpGet()]
        [Route("api/v1/ref/transactionTypes")]
        [ProducesResponseType(typeof(IEnumerable<CommonRefDataNamespace.TransactionType>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> TransactionTypeGetAll([FromQuery] IEnumerable<string>? codes = default, string? text = default)
            => _webApi.GetAsync(Request, p => _orchestrator.GetWithFilterAsync<RefDataNamespace.TransactionType>(codes, text, p.RequestOptions.IncludeInactive));

        /// <summary> 
        /// Gets all of the <see cref="RefDataNamespace.TransactionStatus"/> reference data items that match the specified criteria.
        /// </summary>
        /// <param name="codes">The reference data code list.</param>
        /// <param name="text">The reference data text (including wildcards).</param>
        /// <returns>A RefDataNamespace.TransactionStatus collection.</returns>
        [HttpGet()]
        [Route("api/v1/ref/transactionStatuses")]
        [ProducesResponseType(typeof(IEnumerable<CommonRefDataNamespace.TransactionStatus>), (int)HttpStatusCode.OK)]
        public Task<IActionResult> TransactionStatusGetAll([FromQuery] IEnumerable<string>? codes = default, string? text = default)
            => _webApi.GetAsync(Request, p => _orchestrator.GetWithFilterAsync<RefDataNamespace.TransactionStatus>(codes, text, p.RequestOptions.IncludeInactive));

        /// <summary>
        /// Gets the reference data entries for the specified entities and codes from the query string; e.g: api/v1/ref?entity=codeX,codeY&amp;entity2=codeZ&amp;entity3
        /// </summary>
        /// <returns>A <see cref="ReferenceDataMultiCollection"/>.</returns>
        [HttpGet()]
        [Route("api/v1/ref")]
        [ProducesResponseType(typeof(IEnumerable<CoreEx.RefData.ReferenceDataMultiItem>), (int)HttpStatusCode.OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> GetNamed() => _webApi.GetAsync(Request, p => _orchestrator.GetNamedAsync(p.RequestOptions));
    }
}

#pragma warning restore
#nullable restore