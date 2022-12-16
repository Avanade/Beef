namespace MyEf.Hr.Test.Apis;

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
            var args = Database.Program.ConfigureMigrationArgs(new MigrationArgs(count == 0 ? MigrationCommand.ResetAndDatabase : MigrationCommand.ResetAndData, settings.DatabaseConnectionString)).AddAssembly<FixtureSetUp>();
            var (Success, Output) = await new SqlServerMigration(args).MigrateAndLogAsync(ct).ConfigureAwait(false);
            if (!Success)
                Assert.Fail(Output);

            return Success;
        });
    }
}