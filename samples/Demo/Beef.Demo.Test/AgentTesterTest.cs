using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Net;

namespace Beef.Demo.Test
{
    [TestFixture]
    public class AgentTesterTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp("USER_NAME")]
        public void A010_UserDefault()
        {
            // Make sure the user is set up from the TestSetUp attribute.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.Username);

            // Execute test without passing in a user.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .Run(a =>
                    {
                        Assert.IsTrue(ExecutionContext.HasCurrent);
                        Assert.AreEqual("USER_NAME", ExecutionContext.Current.Username);
                        return a.GetAsync(1.ToGuid());
                    });

            // Mare sure the user has not changed.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.Username);
        }

        [Test, TestSetUp("USER_NAME")]
        public void A020_UserOverride()
        {
            // Make sure the user is set up from the TestSetUp attribute.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("USER_NAME", ExecutionContext.Current.Username);

            // Execute test overridding the user.
            AgentTester.Test<PersonAgent, Person>("ANOTHER_USER")
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .Run(a =>
                {
                    Assert.IsTrue(ExecutionContext.HasCurrent);
                    Assert.AreEqual("ANOTHER_USER", ExecutionContext.Current.Username);
                    return a.GetAsync(1.ToGuid());
                });

            // Mare sure the user has not changed.
            Assert.IsTrue(ExecutionContext.HasCurrent);
            Assert.AreEqual("ANOTHER_USER", ExecutionContext.Current.Username);
        }
    }
}