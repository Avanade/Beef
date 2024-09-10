namespace Cdr.Banking.Business.Data;

/// <summary>
/// Represents the CosmosDb/DocumentDb client.
/// </summary>
public interface ICosmos : ICosmosDb
{
    /// <summary>
    /// Exposes <see cref="Account"/> entity from <b>Account</b> container.
    /// </summary>
    CosmosDbContainer<Account, Model.Account> Accounts { get; }

    /// <summary>
    /// Exposes <see cref="AccountDetail"/> entity from <b>Account</b> container.
    /// </summary>
    CosmosDbContainer<AccountDetail, Model.Account> AccountDetails { get; }

    /// <summary>
    /// Exposes <see cref="AccountDetail"/> entity from <b>Account</b> container.
    /// </summary>
    CosmosDbContainer<Transaction, Model.Transaction> Transactions { get; }
}

/// <summary>
/// Represents the CosmosDb/DocumentDb client.
/// </summary>
public class CosmosDb : CoreEx.Cosmos.CosmosDb, ICosmos
{
    private readonly Lazy<CosmosDbContainer<Account, Model.Account>> _accounts;
    private readonly Lazy<CosmosDbContainer<AccountDetail, Model.Account>> _accountDetails;
    private readonly Lazy<CosmosDbContainer<Transaction, Model.Transaction>> _transactions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDb"/> class.
    /// </summary>
    public CosmosDb(Mac.Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : base(database, mapper, invoker)
    {
        // Apply an authorization filter to all operations to ensure only the valid data is available based on the users context; i.e. only allow access to Accounts within list defined on ExecutionContext.
        UseAuthorizeFilter<Model.Account>("Account", (q) => ((IQueryable<Model.Account>)q).Where(x => ExecutionContext.Current.Accounts.Contains(x.Id!)));
        UseAuthorizeFilter<Model.Account>("Transaction", (q) => ((IQueryable<Model.Transaction>)q).Where(x => ExecutionContext.Current.Accounts.Contains(x.AccountId!)));

        // Lazy create the containers.
        _accounts = new(() => Container<Account, Model.Account>("Account"));
        _accountDetails = new(() => Container<AccountDetail, Model.Account>("Account"));
        _transactions = new(() => Container<Transaction, Model.Transaction>("Transaction"));
    }

    /// <summary>
    /// Exposes <see cref="Account"/> entity from <b>Account</b> container.
    /// </summary>
    public CosmosDbContainer<Account, Model.Account> Accounts => _accounts.Value;

    /// <summary>
    /// Exposes <see cref="AccountDetail"/> entity from <b>Account</b> container.
    /// </summary>
    public CosmosDbContainer<AccountDetail, Model.Account> AccountDetails => _accountDetails.Value;

    /// <summary>
    /// Exposes <see cref="AccountDetail"/> entity from <b>Account</b> container.
    /// </summary>
    public CosmosDbContainer<Transaction, Model.Transaction> Transactions => _transactions.Value;
}