/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Cdr.Banking.Api.Controllers
{
    /// <summary>
    /// Provides the <see cref="Account"/> Web API functionality.
    /// </summary>
    [Route("api/v1/banking/accounts")]
    [Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
    public partial class AccountController : ControllerBase
    {
        private readonly WebApi _webApi;
        private readonly IAccountManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="webApi">The <see cref="WebApi"/>.</param>
        /// <param name="manager">The <see cref="IAccountManager"/>.</param>
        public AccountController(WebApi webApi, IAccountManager manager)
            { _webApi = webApi ?? throw new ArgumentNullException(nameof(webApi)); _manager = manager ?? throw new ArgumentNullException(nameof(manager)); AccountControllerCtor(); }

        partial void AccountControllerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Get all accounts.
        /// </summary>
        /// <param name="productCategory">The Product Category (see <see cref="RefDataNamespace.ProductCategory"/>).</param>
        /// <param name="openStatus">The Open Status (see <see cref="RefDataNamespace.OpenStatus"/>).</param>
        /// <param name="isOwned">Indicates whether Is Owned.</param>
        /// <returns>The <see cref="AccountCollection"/></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(AccountCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public Task<IActionResult> GetAccounts([FromQuery(Name="product-category")] string? productCategory = default, [FromQuery(Name="open-status")] string? openStatus = default, [FromQuery(Name="is-owned")] bool? isOwned = default)
        {
            var args = new AccountArgs { ProductCategorySid = productCategory, OpenStatusSid = openStatus, IsOwned = isOwned };
            return _webApi.GetAsync<AccountCollectionResult>(Request, p => _manager.GetAccountsAsync(args, p.RequestOptions.Paging), alternateStatusCode: HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Get <see cref="AccountDetail"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="AccountDetail"/> where found.</returns>
        [HttpGet("{accountId}")]
        [ProducesResponseType(typeof(Common.Entities.AccountDetail), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Task<IActionResult> GetDetail(string? accountId) =>
            _webApi.GetAsync<AccountDetail?>(Request, p => _manager.GetDetailAsync(accountId));

        /// <summary>
        /// Get <see cref="Account"/> <see cref="Balance"/>.
        /// </summary>
        /// <param name="accountId">The <see cref="Account"/> identifier.</param>
        /// <returns>The selected <see cref="Balance"/> where found.</returns>
        [HttpGet("{accountId}/balance")]
        [ProducesResponseType(typeof(Common.Entities.Balance), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Task<IActionResult> GetBalance(string? accountId) =>
            _webApi.GetAsync<Balance?>(Request, p => _manager.GetBalanceAsync(accountId));
    }
}

#pragma warning restore
#nullable restore