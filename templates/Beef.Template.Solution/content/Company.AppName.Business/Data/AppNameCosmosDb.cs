﻿using Beef.Data.Cosmos;
using Microsoft.Azure.Cosmos;

namespace Company.AppName.Business.Data
{
    /// <summary>
    /// Represents the <b>Test</b> DocumentDb/CosmosDb client.
    /// </summary>
    public class AppNameCosmosDb : CosmosDb<AppNameCosmosDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppNameCosmosDb"/> class.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        public AppNameCosmosDb(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = null) : base(client, databaseId, createDatabaseIfNotExists, throughput) { }
    }
}
