using Cdr.Banking.Business.Entities;
using CoreEx.Cosmos;
using CoreEx.Mapping;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Represents the CosmosDb/DocumentDb client.
    /// </summary>
    public interface ICosmos : CoreEx.Cosmos.ICosmosDb
    {
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

    /// <summary>
    /// Represents the CosmosDb/DocumentDb client.
    /// </summary>
    public class CosmosDb : CoreEx.Cosmos.CosmosDb, ICosmos
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDb"/> class.
        /// </summary>
        public CosmosDb(Database database, IMapper mapper, CosmosDbInvoker? invoker = null) : base(database, mapper, invoker)
        {
            // Apply an authorization filter to all operations to ensure only the valid data is available based on the users context; i.e. only allow access to Accounts within list defined on ExecutionContext.
            UseAuthorizeFilter<Model.Account>("Account", (q) => ((IQueryable<Model.Account>)q).Where(x => ExecutionContext.Current.Accounts.Contains(x.Id!)));
            UseAuthorizeFilter<Model.Account>("Transaction", (q) => ((IQueryable<Model.Transaction>)q).Where(x => ExecutionContext.Current.Accounts.Contains(x.AccountId!)));
        }
    }
}