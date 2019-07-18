using Beef.Data.Cosmos;
using Microsoft.Azure.Cosmos;

namespace Beef.Demo.Business.Data
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
        public CosmosDb(CosmosClient client, string databaseId) : base(client, databaseId) { }
    }
}
