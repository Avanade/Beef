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
            TestSetUp.RegisterSetUp((count, data) =>
            {
                return DatabaseExecutor.Run(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, 
                    AgentTester.Configuration["ConnectionStrings:BeefDemo"],
                    typeof(DatabaseExecutor).Assembly, typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) == 0;
            });

            AgentTester.StartupTestServer<Startup>();
        }
    }
}