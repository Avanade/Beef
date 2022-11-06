namespace Company.AppName.Test.Apis;

[SetUpFixture]
public class FixtureSetUp
{
#if (implement_database || implement_entityframework)
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.ExpectedEventsEnabled = true;
        TestSetUp.Default.ExpectNoEvents = true;

        TestSetUp.Default.RegisterSetUp(async (count, _, __) =>
        {
            using var test = ApiTester.Create<Startup>();
            var settings = test.Services.GetRequiredService<AppNameSettings>();

            return await DatabaseExecutor.RunAsync(new DatabaseExecutorArgs(
                count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, settings.DatabaseConnectionString,
                typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) { UseBeefDbo = true }).ConfigureAwait(false) == 0;
        });
    }
#endif
#if (implement_cosmos)
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        using var test = ApiTester.Create<Startup>();
        _cosmosDb = test.Services.GetRequiredService<ICosmos>();

        await _cosmosDb.Database.Client.CreateDatabaseIfNotExistsAsync(_cosmosDb.Database.Id, cancellationToken: ct).ConfigureAwait(false);

        var ac = await _cosmosDb.Database.ReplaceOrCreateContainerAsync(new Cosmos.ContainerProperties
        {
            Id = _cosmosDb.Accounts.Container.Id,
            PartitionKeyPath = "/_partitionKey"
        }, 400, cancellationToken: ct).ConfigureAwait(false);

        var tc = await _cosmosDb.Database.ReplaceOrCreateContainerAsync(new Cosmos.ContainerProperties
        {
            Id = _cosmosDb.Transactions.Container.Id,
            PartitionKeyPath = "/accountId"
        }, 400, cancellationToken: ct).ConfigureAwait(false);

        var rdc = await _cosmosDb.Database.ReplaceOrCreateContainerAsync(new Cosmos.ContainerProperties
        {
            Id = "RefData",
            PartitionKeyPath = "/_partitionKey",
            UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
        }, 400, cancellationToken: ct).ConfigureAwait(false);

        var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("Data.yaml");
        await _cosmosDb.Accounts.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);
        await _cosmosDb.Transactions.ImportBatchAsync(jdr, cancellationToken: ct).ConfigureAwait(false);

        jdr = JsonDataReader.ParseYaml<FixtureSetUp>("RefData.yaml", new JsonDataReaderArgs(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
        await _cosmosDb.ImportValueBatchAsync("RefData", jdr, test.Services.GetRequiredService<CoreEx.RefData.ReferenceDataOrchestrator>().GetAllTypes(), cancellationToken: ct).ConfigureAwait(false);

        return true;
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_cosmosDb != null && _removeAfterUse)
            await _cosmosDb.Database.DeleteAsync().ConfigureAwait(false);
    }
#endif
#if (implement_httpagent || implement_none)
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.DefaultEnvironmentVariablePrefix = "AppName";
        TestSetUp.AddWebApiAgentArgsType<IAppNameWebApiAgentArgs, AppNameWebApiAgentArgs>();
        TestSetUp.DefaultExpectNoEvents = false;
    }
#endif
}