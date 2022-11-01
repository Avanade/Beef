using Beef.Database.Core;
using Beef.Demo.Api;
using Beef.Demo.Business;
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
            TestSetUp.Default.RegisterSetUp(async (count, _, __) =>
            {
                using var test = ApiTester.Create<Startup>();
                var settings = test.Services.GetRequiredService<DemoSettings>();

                var args = new DatabaseExecutorArgs(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, settings.DatabaseConnectionString,
                    typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly(), typeof(Beef.Demo.Abc.Database.Scripts).Assembly)
                { UseBeefDbo = true }.AddSchemaOrder("Sec", "Ref", "Test", "Demo");

                return await DatabaseExecutor.RunAsync(args).ConfigureAwait(false) == 0;
            });
        }
    }
}