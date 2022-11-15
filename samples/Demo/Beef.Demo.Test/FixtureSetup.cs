using Beef.Database.Core;
using Beef.Database.Core.SqlServer;
using Beef.Demo.Api;
using Beef.Demo.Business;
using DbEx;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Reflection;
using UnitTestEx;
using UnitTestEx.NUnit;

namespace Beef.Demo.Test
{
    [SetUpFixture]
    public class FixtureSetUp
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Default.RegisterSetUp(async (count, _, ct) =>
            {
                using var test = ApiTester.Create<Startup>();
                var settings = test.Services.GetRequiredService<DemoSettings>();

                var args = new MigrationArgs(
                    count == 0 ? MigrationCommand.ResetAndDatabase : MigrationCommand.ResetAndData, settings.DatabaseConnectionString,
                    typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly(), typeof(Abc.Database.Scripts).Assembly)
                { UseBeefSchema = true }.AddSchemaOrder("Sec", "Ref", "Test", "Demo");

                return await new SqlServerMigration(args).MigrateAsync(ct).ConfigureAwait(false);
            });
        }
    }
}