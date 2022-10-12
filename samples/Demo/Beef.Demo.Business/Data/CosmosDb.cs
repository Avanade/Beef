using CoreEx.Cosmos;
using Microsoft.Azure.Cosmos;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Test</b> DocumentDb/CosmosDb client.
    /// </summary>
    public interface ICosmos : CoreEx.Cosmos.ICosmosDb
    {
        public CosmosDbContainer<Robot, Model.Robot> Items => Container<Robot, Model.Robot>("Items");
    }

    /// <summary>
    /// Represents the <b>Test</b> DocumentDb/CosmosDb client.
    /// </summary>
    public class CosmosDb : CoreEx.Cosmos.CosmosDb
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDb"/> class.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        public CosmosDb(Microsoft.Azure.Cosmos.Database database, IMapper mapper, CoreEx.Cosmos.CosmosDbInvoker? invoker = null) : base(database, mapper, invoker) { }
    }
}