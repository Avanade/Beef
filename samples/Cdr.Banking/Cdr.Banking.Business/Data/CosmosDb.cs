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
    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDb"/> class.
    /// </summary>
    public CosmosDb(Mac.Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : base(database, mapper, invoker)
    {
        // Apply the authorization filter to all containers to ensure only the valid data is available based on the users context; i.e.only allow access to Accounts within list defined on ExecutionContext.
        Container("Account").UseAuthorizeFilter<Model.Account>(q => q.Where(x => ExecutionContext.Current.Accounts.Contains(x.Id!)));
        Container("Transaction").UseAuthorizeFilter<Model.Transaction>(q => q.Where(x => ExecutionContext.Current.Accounts.Contains(x.AccountId!)));
    }

    /// <summary>
    /// Exposes <see cref="Account"/> entity from <b>Account</b> container.
    /// </summary>
    public CosmosDbContainer<Account, Model.Account> Accounts => Container<Account, Model.Account>("Account");

    /// <summary>
    /// Exposes <see cref="AccountDetail"/> entity from <b>Account</b> container.
    /// </summary>
    public CosmosDbContainer<AccountDetail, Model.Account> AccountDetails => Container<AccountDetail, Model.Account>("Account");

    /// <summary>
    /// Exposes <see cref="AccountDetail"/> entity from <b>Account</b> container.
    /// </summary>
    public CosmosDbContainer<Transaction, Model.Transaction> Transactions => Container<Transaction, Model.Transaction>("Transaction");
}