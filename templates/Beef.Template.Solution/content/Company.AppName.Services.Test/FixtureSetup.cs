namespace Company.AppName.Services.Test;

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
            using var test = FunctionTester.Create<Startup>();
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
                using var test = FunctionTester.Create<Startup>();
                var cosmosDb = test.Services.GetRequiredService<ICosmos>();

                await cosmosDb.Database.Client.CreateDatabaseIfNotExistsAsync(cosmosDb.Database.Id, cancellationToken: ct).ConfigureAwait(false);

                var ac = await cosmosDb.Database.ReplaceOrCreateContainerAsync(new AzCosmos.ContainerProperties
                {
                    Id = cosmosDb.Persons.Container.Id,
                    PartitionKeyPath = "/_partitionKey"
                }, 400, cancellationToken: ct).ConfigureAwait(false);

                var rdc = await cosmosDb.Database.ReplaceOrCreateContainerAsync(new AzCosmos.ContainerProperties
                {
                    Id = "RefData",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                }, 400, cancellationToken: ct).ConfigureAwait(false);

                var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("Person.yaml");
                await cosmosDb.Persons.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);

                jdr = JsonDataReader.ParseYaml<FixtureSetUp>("RefData.yaml", new JsonDataReaderArgs(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
                await cosmosDb.ImportValueBatchAsync("RefData", jdr, test.Services.GetRequiredService<CoreEx.RefData.ReferenceDataOrchestrator>().GetAllTypes(), cancellationToken: ct).ConfigureAwait(false);
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