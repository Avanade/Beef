using Beef.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using My.Hr.Api;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;

namespace My.Hr.Test.Apis
{
    [TestFixture, NonParallelizable]
    public class PerformanceReviewTest
    {
        #region Get

        [Test, TestSetUp]
        public void A110_Get_NotFound()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void A110_Get()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new PerformanceReview
                {
                    Id = 2.ToGuid(),
                    EmployeeId = 1.ToGuid(),
                    Date = new DateTime(2016, 11, 12),
                    Outcome = "EE",
                    Reviewer = "r.Browne@org.com",
                    Notes = "They are awesome!"
                })
                .Run(a => a.GetAsync(2.ToGuid()));
        }

        #endregion

        #region GetByEmployeeId

        [Test, TestSetUp]
        public void A210_GetByEmployeeId_NotFound()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PerformanceReviewAgent, PerformanceReviewCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByEmployeeIdAsync(4.ToGuid())).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp]
        public void A220_GetByEmployeeId()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PerformanceReviewAgent, PerformanceReviewCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByEmployeeIdAsync(2.ToGuid())).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Work quality low.", "Work quality below standard." }, v.Result.Select(x => x.Notes).ToArray());
        }

        [Test, TestSetUp]
        public void A220_GetByEmployeeId_Last()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PerformanceReviewAgent, PerformanceReviewCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByEmployeeIdAsync(2.ToGuid(), PagingArgs.CreateSkipAndTake(0, 1))).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "Work quality low." }, v.Result.Select(x => x.Notes).ToArray());
        }

        #endregion

        #region Create

        [Test, TestSetUp]
        public void B110_Create()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = new PerformanceReview
            {
                Date = new DateTime(2020, 06, 15),
                Outcome = "ME",
                Notes = "Solid performance :-)",
                Reviewer = "the.boss@org.com",
            };

            // Create value.
            v = agentTester.Test<PerformanceReviewAgent, PerformanceReview>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue(_ => v, "EmployeeId")
                .ExpectEvent("My.Hr.PerformanceReview", "Created")
                .Run(a => a.CreateAsync(v, 3.ToGuid())).Value!;

            Assert.AreEqual(3.ToGuid(), v.EmployeeId);

            // Check the value was created properly.
            agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(v.Id));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void C110_Update_NotFound()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var v = agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid())).Value!;

            // Try updating with an invalid identifier.
            agentTester.Test<PerformanceReviewAgent, PerformanceReview>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void C120_Update_Concurrency()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var id = 3.ToGuid();
            var v = agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            agentTester.Test<PerformanceReviewAgent, PerformanceReview>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = TestSetUp.ConcurrencyErrorETag;
            agentTester.Test<PerformanceReviewAgent, PerformanceReview>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id));
        }

        [Test, TestSetUp]
        public void C130_Update()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var id = 3.ToGuid();
            var v = agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.Notes += "X";

            // Update the value.
            v = agentTester.Test<PerformanceReviewAgent, PerformanceReview>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"My.Hr.PerformanceReview", "Updated")
                .Run(a => a.UpdateAsync(v, id)).Value!;

            // Check the value was updated properly.
            agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void E110_Delete()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var id = 3.ToGuid();

            // Get an existing value.
            agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id));

            // Delete value.
            agentTester.Test<PerformanceReviewAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"My.Hr.PerformanceReview", "Deleted")
                .Run(a => a.DeleteAsync(id));

            // Check value no longer exists.
            agentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(id));

            // Delete again (should still be successful as a Delete is idempotent); note there should be no corresponding event as nothing actually happened.
            agentTester.Test<PerformanceReviewAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(id));
        }

        #endregion
    }
}