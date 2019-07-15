using Beef.Data.DocumentDb;
using Microsoft.Azure.Documents.Client;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Test</b> DocumentDb/CosmosDb client.
    /// </summary>
    public class DocDb : DocDb<DocDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocDb"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DocumentClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        public DocDb(DocumentClient client, string databaseId) : base(client, databaseId) { }
    }
}
