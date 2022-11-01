using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using CoreEx;
using NUnit.Framework;
using System;
using System.Net;
using UnitTestEx;
using UnitTestEx.Expectations;

namespace Beef.Demo.Test
{
    [TestFixture]
    public class AgentTesterTest : UsingAgentTesterServer<Startup>
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => base.ApiTester.SetUp.SetUp();

        [Test, TestSetUp("USER_NAME")]
        public void A010_UserDefault()
        {
            // Make sure the user is set up from the TestSetUp attribute.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.UserName);

            // Execute test without passing in a user.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .Run(a =>
                    {
                        Assert.IsTrue(ExecutionContext.HasCurrent);
                        Assert.AreEqual("USER_NAME", ExecutionContext.Current.UserName);
                        return a.GetAsync(1.ToGuid());
                    });

            // Mare sure the user has not changed.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.UserName);
        }

        [Test, TestSetUp("USER_NAME")]
        public void A020_UserOverride()
        {
            // Make sure the user is set up from the TestSetUp attribute.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.UserName);

            // Execute test overriding the user.
            AgentTester.Test<PersonAgent, Person>("ANOTHER_USER")
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .Run(a =>
                {
                    Assert.IsTrue(ExecutionContext.HasCurrent);
                    Assert.AreEqual("ANOTHER_USER", ExecutionContext.Current.UserName);
                    return a.GetAsync(1.ToGuid());
                });

            // Make sure that the user has changed back.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.UserName);
        }
    }
}