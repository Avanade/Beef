using Cdr.Banking.Api;
using Cdr.Banking.Business.Data;
using CoreEx.Cosmos;
using CoreEx.Cosmos.Batch;
using CoreEx.Json.Data;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.NUnit;
using Cosmos = Microsoft.Azure.Cosmos;

namespace Cdr.Banking.Test
{
    [SetUpFixture]
    public class FixtureSetUp
    {
        private ICosmos? _cosmosDb;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Default.RegisterSetUp(async (count, _, ct) =>
            {
                // Setup and load cosmos once only.
                if (count == 0)
                {
                    using var test = ApiTester.Create<Startup>();
                    _cosmosDb = test.Services.GetRequiredService<ICosmos>();

                    await _cosmosDb.Database.Client.CreateDatabaseIfNotExistsAsync(_cosmosDb.Database.Id, cancellationToken: ct).ConfigureAwait(false);

                    var ac = await _cosmosDb.Database.ReplaceOrCreateContainerAsync(new Cosmos.ContainerProperties
                    {
                        Id = _cosmosDb.Accounts.Container.Id,
                        PartitionKeyPath = "/_partitionKey"
                    }, cancellationToken: ct).ConfigureAwait(false);

                    var tc = await _cosmosDb.Database.ReplaceOrCreateContainerAsync(new Cosmos.ContainerProperties
                    {
                        Id = _cosmosDb.Transactions.Container.Id,
                        PartitionKeyPath = "/accountId"
                    }, cancellationToken: ct).ConfigureAwait(false);

                    var rdc = await _cosmosDb.Database.ReplaceOrCreateContainerAsync(new Cosmos.ContainerProperties
                    {
                        Id = "RefData",
                        PartitionKeyPath = "/_partitionKey",
                        UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                    }, cancellationToken: ct).ConfigureAwait(false);

                    var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("Data.yaml");
                    await _cosmosDb.Accounts.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);
                    await _cosmosDb.Transactions.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);

                    jdr = JsonDataReader.ParseYaml<FixtureSetUp>("RefData.yaml", new JsonDataReaderArgs(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
                    await _cosmosDb.ImportValueBatchAsync("RefData", jdr, test.Services.GetRequiredService<CoreEx.RefData.ReferenceDataOrchestrator>().GetAllTypes(), cancellationToken: ct).ConfigureAwait(false);
                }

                return true;
            });

            // TODO: Passing the username as an http header for all requests; this would be replaced with OAuth integration, etc.
            TestSetUp.Default.OnBeforeHttpRequestMessageSendAsync = (req, userName, _) =>
            {
                req.Headers.Add("cdr-user", userName);
                return Task.CompletedTask;
            };

            // Set "page" and "page-size" as the supported paging query string parameters as defined by the CDR specification.
            CoreEx.Http.HttpConsts.PagingArgsPageQueryStringName = "page";
            CoreEx.Http.HttpConsts.PagingArgsSizeQueryStringName = "page-size";
        }
    }
}