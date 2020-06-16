using Beef.Caching.Policy;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Events.WebJobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;

[assembly: WebJobsStartup(typeof(Beef.Demo.Functions.Startup))]

namespace Beef.Demo.Functions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var config = builder.GetConfiguration<Startup>(environmentVariablePrefix: "Beef_");

            // Load the cache policies.
            CachePolicyManager.SetFromCachePolicyConfig(config.GetSection("BeefCaching").Get<CachePolicyConfig>());
            CachePolicyManager.StartFlushTimer(CachePolicyManager.TenMinutes, CachePolicyManager.FiveMinutes);

            // Register the ReferenceData provider.
            RefData.ReferenceDataManager.Register(new ReferenceDataProvider());

            // Register the database.
            Database.Register(() => new Database(config.GetConnectionString("BeefDemo")));

            // Register the DocumentDb/CosmosDb client.
            CosmosDb.Register(() =>
            {
                var cs = config.GetSection("CosmosDb");
                return new CosmosDb(new Microsoft.Azure.Cosmos.CosmosClient(cs.GetValue<string>("EndPoint"), cs.GetValue<string>("AuthKey")), cs.GetValue<string>("Database"));
            });
        }
    }
}