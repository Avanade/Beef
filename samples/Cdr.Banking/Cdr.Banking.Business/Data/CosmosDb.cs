using Beef.Data.Cosmos;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Represents the <b>Test</b> DocumentDb/CosmosDb client.
    /// </summary>
    public class CosmosDb : CosmosDb<CosmosDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDb"/> class.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        public CosmosDb(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = null) : base(client, databaseId, createDatabaseIfNotExists, throughput)
        {
            // Apply an authorization filter to all operations to ensure only the valid data is available based on the users context - only allow access to Accounts within list defined on ExecutionContext.
            SetAuthorizeFilter<Model.Account>("Account", (q) => ((IQueryable<Model.Account>)q).Where(am => ExecutionContext.Current.Accounts.Contains(am.Id!)));
        }
    }
}