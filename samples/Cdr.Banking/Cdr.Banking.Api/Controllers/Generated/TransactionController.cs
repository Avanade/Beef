/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Api.Controllers;

/// <summary>
/// Provides the <see cref="Transaction"/> Web API functionality.
/// </summary>
[Produces(System.Net.Mime.MediaTypeNames.Application.Json)]
public partial class TransactionController : ControllerBase
{
    private readonly WebApi _webApi;
    private readonly ITransactionManager _manager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionController"/> class.
    /// </summary>
    /// <param name="webApi">The <see cref="WebApi"/>.</param>
    /// <param name="manager">The <see cref="ITransactionManager"/>.</param>
    public TransactionController(WebApi webApi, ITransactionManager manager)
        { _webApi = webApi.ThrowIfNull(); _manager = manager.ThrowIfNull(); TransactionControllerCtor(); }

    partial void TransactionControllerCtor(); // Enables additional functionality to be added to the constructor.

    /// <summary>
    /// Get transaction for account.
    /// </summary>
    /// <param name="accountId">The Account Id.</param>
    /// <param name="fromDate">The From (oldest time).</param>
    /// <param name="toDate">The To (newest time).</param>
    /// <param name="minAmount">The Min Amount.</param>
    /// <param name="maxAmount">The Max Amount.</param>
    /// <param name="text">The Text.</param>
    /// <returns>The <see cref="TransactionCollection"/></returns>
    [HttpGet("api/v1/banking/accounts/{accountId}/transactions")]
    [Paging]
    [ProducesResponseType(typeof(Common.Entities.TransactionCollection), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public Task<IActionResult> GetTransactions([FromRoute] string? accountId, [FromQuery(Name="oldest-time")] DateTime? fromDate = default, [FromQuery(Name="newest-time")] DateTime? toDate = default, [FromQuery(Name="min-amount")] decimal? minAmount = default, [FromQuery(Name="max-amount")] decimal? maxAmount = default, [FromQuery(Name="text")] string? text = default)
    {
        var args = new TransactionArgs { FromDate = fromDate, ToDate = toDate, MinAmount = minAmount, MaxAmount = maxAmount, Text = text };
        return _webApi.GetWithResultAsync<TransactionCollectionResult>(Request, p => _manager.GetTransactionsAsync(accountId, args, p.RequestOptions.Paging), alternateStatusCode: HttpStatusCode.NoContent);
    }
}