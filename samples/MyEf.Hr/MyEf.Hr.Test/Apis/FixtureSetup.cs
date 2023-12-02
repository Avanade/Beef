namespace MyEf.Hr.Test.Apis;

[SetUpFixture]
public class FixtureSetUp
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.EnableExpectedEvents();
        TestSetUp.Default.ExpectNoEvents();

        TestSetUp.Default.RegisterAutoSetUp(async (count, _, ct) =>
        {
            using var test = ApiTester.Create<Startup>();
            var settings = test.Services.GetRequiredService<HrSettings>();
            var args = Database.Program.ConfigureMigrationArgs(new MigrationArgs(count == 0 ? MigrationCommand.ResetAndDatabase : MigrationCommand.ResetAndData, settings.DatabaseConnectionString)).AddAssembly<FixtureSetUp>();
            return await new SqlServerMigration(args).MigrateAndLogAsync(ct).ConfigureAwait(false);
        });
    }
}