using Beef;
using Beef.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using My.Hr.Api;
using My.Hr.Business.Validation;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace My.Hr.Test
{
    [TestFixture, NonParallelizable]
    public class PerformanceReviewTest : UsingAgentTesterServer<Startup>
    {
        #region Get

        [Test, TestSetUp]
        public void A110_Get_NotFound()
        {
            AgentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void A110_Get()
        {
            AgentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new PerformanceReview
                {
                    Id = 2.ToGuid(),
                    EmployeeId = 1.ToGuid(),
                    Date = new DateTime(2016, 11, 12),
                    OutcomeSid = "EE",
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
            var v = AgentTester.Test<PerformanceReviewAgent, PerformanceReviewCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByEmployeeIdAsync(4.ToGuid())).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp]
        public void A220_GetByEmployeeId()
        {
            var v = AgentTester.Test<PerformanceReviewAgent, PerformanceReviewCollectionResult>()
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
            var v = AgentTester.Test<PerformanceReviewAgent, PerformanceReviewCollectionResult>()
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
            var v = new PerformanceReview
            {
                Date = new DateTime(2020, 06, 15),
                OutcomeSid = "ME",
                Notes = "Solid performance :-)",
                Reviewer = "the.boss@org.com",
            };

            // Create value.
            v = AgentTester.Test<PerformanceReviewAgent, PerformanceReview>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue(_ => v, "EmployeeId")
                .ExpectEvent("My.Hr.PerformanceReview.*", "Created")
                .Run(a => a.CreateAsync(v, 3.ToGuid())).Value!;

            Assert.AreEqual(3.ToGuid(), v.EmployeeId);

            // Check the value was created properly.
            AgentTester.Test<PerformanceReviewAgent, PerformanceReview?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(v.Id));
        }

        #endregion
    }
}