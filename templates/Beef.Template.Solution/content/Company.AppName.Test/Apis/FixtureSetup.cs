namespace Company.AppName.Test.Apis;

[SetUpFixture]
public class FixtureSetUp
{
#if (implement_database || implement_entityframework)
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.EnableExpectedEvents();
        TestSetUp.Default.ExpectNoEvents();

        TestSetUp.Default.RegisterAutoSetUp(async (count, _, ct) =>
        {
            using var test = ApiTester.Create<Startup>();
            var settings = test.Services.GetRequiredService<AppNameSettings>();
            var args = Database.Program.ConfigureMigrationArgs(new MigrationArgs(count == 0 ? MigrationCommand.ResetAndDatabase : MigrationCommand.ResetAndData, settings.DatabaseConnectionString)).AddAssembly<FixtureSetUp>();
#if (implement_database || implement_sqlserver)
            return await new SqlServerMigration(args).MigrateAndLogAsync(ct).ConfigureAwait(false);
#endif
#if (implement_mysql)
            return await new MySqlMigration(args).MigrateAndLogAsync(ct).ConfigureAwait(false);
#endif
#if (implement_postgres)
            return await new PostgresMigration(args).MigrateAndLogAsync(ct).ConfigureAwait(false);
#endif
        });
    }
#endif
#if (implement_cosmos)
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
                var cosmosDb = test.Services.GetRequiredService<AppNameCosmosDb>();

                // Create the Cosmos Db (where not exists).
                await cosmosDb.Database.Client.CreateDatabaseIfNotExistsAsync(cosmosDb.Database.Id, cancellationToken: ct).ConfigureAwait(false);

                // Create 'Person' container.
                var cdp = cosmosDb.Database.DefineContainer(cosmosDb.Persons.Container.Id, "/_partitionKey")
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
                var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("Person.yaml");
                await cosmosDb.Persons.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);

                jdr = JsonDataReader.ParseYaml<FixtureSetUp>("RefData.yaml", new JsonDataReaderArgs(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
                await cosmosDb.ImportValueBatchAsync("RefData", jdr, ReferenceDataOrchestrator.GetAllTypesInNamespace<Business.Data.Model.Gender>(), cancellationToken: ct).ConfigureAwait(false);
            }

            return true;
        });
    }
#endif
#if (implement_httpagent || implement_none)
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.EnableExpectedEvents();
        TestSetUp.Default.ExpectNoEvents();
    }
#endif
}