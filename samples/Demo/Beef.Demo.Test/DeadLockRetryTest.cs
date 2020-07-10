using Beef.Data.Database;
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
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset();
        }

        [Test, TestSetUp]
        public void A010_DatabaseDeadlock_Retry()
        {
            int count = 0;
            SqlRetryDatabaseInvoker.ExceptionRetry += (s, e) => { count++; Console.WriteLine(e.Exception.ToString()); };

            var p1 = AgentTester.Create<PersonAgent, PersonDetail>().ExpectStatusCode(HttpStatusCode.OK).Run((a) => a.Agent.GetDetailAsync(1.ToGuid())).Value;
            var p2 = AgentTester.Create<PersonAgent, PersonDetail>().ExpectStatusCode(HttpStatusCode.OK).Run((a) => a.Agent.GetDetailAsync(2.ToGuid())).Value;

            //            var task1 = Task.Run(async () =>
            //            {
            //                await Beef.Demo.Business.Data.Database.Default.SqlStatement(
            //                    @"begin transaction
            //select * from Demo.WorkHistory with (tablock, holdlock)
            //waitfor delay '00:00:02'
            //select * from Demo.Person with (tablock, holdlock)
            //waitfor delay '00:00:04'
            //commit transaction"
            //                    ).NonQueryAsync();
            //            });
            Assert.Fail("fix this");

            var task2 = Task.Run(() =>
            {
                Thread.Sleep(500);
                p1.FirstName += "X";
                var r1 = AgentTester.Create<PersonAgent, PersonDetail>().Run((a) => a.Agent.UpdateDetailAsync(p1, p1.Id));
                Console.WriteLine($"Person {p1.Id} update status code: {r1.StatusCode}");
            });

            var task3 = Task.Run(() =>
            {
                Thread.Sleep(750);
                p2.FirstName += "X";
                var r2 = AgentTester.Create<PersonAgent, PersonDetail>().Run((a) => a.Agent.UpdateDetailAsync(p2, p2.Id));
                Console.WriteLine($"Person {p2.Id} update status code: {r2.StatusCode}");
            });

            //Task.WaitAll(task1, task2, task3);

            if (count == 0)
                Assert.Inconclusive("Unable to cause the required database deadlock; therefore, the retry logic was not exercised for this test.");
        }
    }
}