using Beef.Database.Core;
using Beef.Demo.Api;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Reflection;

namespace Beef.Demo.Test
{
    [SetUpFixture]
    public class FixtureSetUp
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.RegisterSetUp(async (count, data) =>
            {
                if (count == 0)
                    await DatabaseExecutor.RunAsync(DatabaseExecutorCommand.Drop, AgentTester.Configuration["ConnectionStrings:BeefDemo"]);

                return await DatabaseExecutor.RunAsync(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, 
                    AgentTester.Configuration["ConnectionStrings:BeefDemo"],
                    typeof(DatabaseExecutor).Assembly, typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly(), typeof(Beef.Demo.Abc.Database.Scripts).Assembly).ConfigureAwait(false) == 0;
            });

            AgentTester.StartupTestServer<Startup>(environmentVariablesPrefix: "Beef_");
        }
    }
}