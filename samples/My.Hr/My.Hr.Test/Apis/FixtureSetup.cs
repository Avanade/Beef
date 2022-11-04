using Beef.Database.Core;
using Microsoft.Extensions.DependencyInjection;
using My.Hr.Api;
using My.Hr.Business;
using NUnit.Framework;
using System.Reflection;
using UnitTestEx;
using UnitTestEx.NUnit;

namespace My.Hr.Test.Apis
{
    [SetUpFixture]
    public class FixtureSetUp
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Default.ExpectedEventsEnabled = true;
            TestSetUp.Default.ExpectNoEvents = true;

            TestSetUp.Default.RegisterSetUp(async (count, _, __) =>
            {
                using var test = ApiTester.Create<Startup>();
                var settings = test.Services.GetRequiredService<HrSettings>();

                return await DatabaseExecutor.RunAsync(new DatabaseExecutorArgs(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, settings.DatabaseConnectionString,
                    typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) { UseBeefDbo = true }).ConfigureAwait(false) == 0;
            });
        }
    }
}