using Beef.Database.Core;
using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
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
            var config = AgentTester.BuildConfiguration<Startup>("Beef");
            TestSetUp.SetDefaultLocalReferenceData<IReferenceData, ReferenceDataAgentProvider, IReferenceDataAgent, ReferenceDataAgent>();
            TestSetUp.RegisterSetUp(async (count, data) =>
            {
                var args = new DatabaseExecutorArgs(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, config["ConnectionStrings:BeefDemo"],
                    typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly(), typeof(Beef.Demo.Abc.Database.Scripts).Assembly)
                { UseBeefDbo = true, RefDataSchemaName = "Ref" }.AddSchemaOrder("Sec", "Ref", "Test", "Demo");

                return await DatabaseExecutor.RunAsync(args).ConfigureAwait(false) == 0;
            });
        }
    }
}