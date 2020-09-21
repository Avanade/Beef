using Beef.Database.Core;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Reflection;
using System.Threading.Tasks;
using My.Hr.Api;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;

namespace My.Hr.Test
{
    [SetUpFixture]
    public class FixtureSetUp
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.DefaultEnvironmentVariablePrefix = "Hr";
            TestSetUp.SetDefaultLocalReferenceData<IReferenceData, ReferenceDataAgentProvider, IReferenceDataAgent, ReferenceDataAgent>();
            TestSetUp.DefaultExpectNoEvents = true;
            var config = AgentTester.BuildConfiguration<Startup>();

            TestSetUp.RegisterSetUp(async (count, _) =>
            {
                return await DatabaseExecutor.RunAsync(new DatabaseExecutorArgs(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, config["ConnectionStrings:Database"],
                    typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) { UseBeefDbo = true } ).ConfigureAwait(false) == 0;
            });
        }
    }
}