namespace My.Hr.Test.Apis;

[SetUpFixture]
public class FixtureSetUp
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.ExpectedEventsEnabled = true;
        TestSetUp.Default.ExpectNoEvents = true;

        TestSetUp.Default.RegisterSetUp(async (count, _, ct) =>
        {
            using var test = ApiTester.Create<Startup>();
            var settings = test.Services.GetRequiredService<HrSettings>();

            return await new SqlServerMigration(new MigrationArgs(
                count == 0 ? MigrationCommand.ResetAndDatabase : MigrationCommand.ResetAndData, settings.DatabaseConnectionString,
                typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) { UseBeefSchema = true }).MigrateAsync(ct).ConfigureAwait(false);
        });
    }
}