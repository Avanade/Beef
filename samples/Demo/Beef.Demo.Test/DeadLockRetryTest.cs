using Beef.Data.Database;
using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class DeadLockRetryTest
    {
        private AgentTesterServer<Startup> _agentTester;

        [OneTimeSetUp]
        public void OneTimeSetUp() { TestSetUp.Reset(); _agentTester = AgentTester.CreateServer<Startup>(); }

        [OneTimeTearDown]
        public void OneTimeTearDown() => _agentTester.Dispose();

        [Test, TestSetUp]
        public void A010_DatabaseDeadlock_Retry()
        {
            int count = 0;
            SqlRetryDatabaseInvoker.ExceptionRetry += (s, e) => { count++; Console.WriteLine(e.Exception.ToString()); };

            var p1 = _agentTester.Test<PersonAgent, PersonDetail>().ExpectStatusCode(HttpStatusCode.OK).Run(a => a.GetDetailAsync(1.ToGuid())).Value;
            var p2 = _agentTester.Test<PersonAgent, PersonDetail>().ExpectStatusCode(HttpStatusCode.OK).Run(a => a.GetDetailAsync(2.ToGuid())).Value;

            var db = new Beef.Demo.Business.Data.Database(AgentTester.BuildConfiguration<Startup>()["ConnectionStrings:BeefDemo"]);

            var task1 = Task.Run(async () =>
            {
                await db.SqlStatement(
                    @"begin transaction
            select * from Demo.WorkHistory with (tablock, holdlock)
            waitfor delay '00:00:02'
            select * from Demo.Person with (tablock, holdlock)
            waitfor delay '00:00:04'
            commit transaction"
                    ).NonQueryAsync();
            });

            var task2 = Task.Run(() =>
            {
                Thread.Sleep(500);
                p1.FirstName += "X";
                var r1 = _agentTester.Test<PersonAgent, PersonDetail>().Run(a => a.UpdateDetailAsync(p1, p1.Id));
                Console.WriteLine($"Person {p1.Id} update status code: {r1.StatusCode}");
            });

            var task3 = Task.Run(() =>
            {
                Thread.Sleep(750);
                p2.FirstName += "X";
                var r2 = _agentTester.Test<PersonAgent, PersonDetail>().Run(a => a.UpdateDetailAsync(p2, p2.Id));
                Console.WriteLine($"Person {p2.Id} update status code: {r2.StatusCode}");
            });

            Task.WaitAll(task1, task2, task3);

            if (count == 0)
                Assert.Inconclusive("Unable to cause the required database deadlock; therefore, the retry logic was not exercised for this test.");
        }
    }
}