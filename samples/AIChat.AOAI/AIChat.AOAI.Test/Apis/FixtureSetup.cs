namespace AIChat.AOAI.Test.Apis;

[SetUpFixture]
public class FixtureSetUp
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.EnableExpectedEvents();
        TestSetUp.Default.ExpectNoEvents();

        TestSetUp.Default.RegisterSetUp(async (count, _, ct) =>
        {
            // Setup and load cosmos once only.
            if (count == 0)
            {
                using var test = ApiTester.Create<Startup>();
                using var scope = test.Services.CreateScope();
                var cosmosDb = scope.ServiceProvider.GetRequiredService<AOAICosmosDb>();

                // Create the Cosmos Db (where not exists).
                await cosmosDb.Database.Client.CreateDatabaseIfNotExistsAsync(cosmosDb.Database.Id, cancellationToken: ct).ConfigureAwait(false);

                // Create 'Person' container.
                var cdp = cosmosDb.Database.DefineContainer(cosmosDb.Chats.Container.Id, "/_partitionKey")
                    .WithIndexingPolicy()
                       .WithCompositeIndex()
                           .Path("/lastName", AzCosmos.CompositePathSortOrder.Ascending)
                           .Path("/firstName", AzCosmos.CompositePathSortOrder.Ascending)
                           .Attach()
                    .Attach()
                    .Build();

                var ac = await cosmosDb.Database.ReplaceOrCreateContainerAsync(cdp, cancellationToken: ct).ConfigureAwait(false);

                // Create 'RefData' container.
                var cdr = cosmosDb.Database.DefineContainer("RefData", "/_partitionKey")
                    .WithUniqueKey()
                        .Path("/type")
                        .Path("/value/code")
                        .Attach()
                    .Build();

                var rdc = await cosmosDb.Database.ReplaceOrCreateContainerAsync(cdr, cancellationToken: ct).ConfigureAwait(false);

                // Import the data.
                //var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("Person.yaml");
                //await cosmosDb.Chats.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);

                var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("RefData.yaml", new JsonDataReaderArgs(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
                await cosmosDb.ImportValueBatchAsync("RefData", jdr, ReferenceDataOrchestrator.GetAllTypesInNamespace<Business.Data.Model.Role>(), cancellationToken: ct).ConfigureAwait(false);
            }

            return true;
        });
    }
}