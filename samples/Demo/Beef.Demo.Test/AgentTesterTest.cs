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
        public void OneTimeSetUp() => Assert.That(ApiTester.SetUp.SetUp(), Is.True);

        [Test, TestSetUp("USER_NAME")]
        public void A010_UserDefault()
        {
            Assert.Multiple(() =>
            {
                // Make sure the user is set up from the TestSetUp attribute.
                Assert.That(ExecutionContext.HasCurrent, Is.True);
                Assert.That(ExecutionContext.Current.UserName, Is.EqualTo("USER_NAME"));
            });

            // Execute test without passing in a user.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .Run(a =>
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.That(ExecutionContext.HasCurrent, Is.True);
                            Assert.That(ExecutionContext.Current.UserName, Is.EqualTo("USER_NAME"));
                        });
                        return a.GetAsync(1.ToGuid());
                    });

            Assert.Multiple(() =>
            {
                // Mare sure the user has not changed.
                Assert.That(ExecutionContext.HasCurrent, Is.True);
                Assert.That(ExecutionContext.Current.UserName, Is.EqualTo("USER_NAME"));
            });
        }

        [Test, TestSetUp("USER_NAME")]
        public void A020_UserOverride()
        {
            Assert.Multiple(() =>
            {
                // Make sure the user is set up from the TestSetUp attribute.
                Assert.That(ExecutionContext.HasCurrent, Is.True);
                Assert.That(ExecutionContext.Current.UserName, Is.EqualTo("USER_NAME"));
            });

            // Execute test overriding the user.
            AgentTester.Test<PersonAgent, Person>("ANOTHER_USER")
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .Run(a =>
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(ExecutionContext.HasCurrent, Is.True);
                        Assert.That(ExecutionContext.Current.UserName, Is.EqualTo("ANOTHER_USER"));
                    });
                    return a.GetAsync(1.ToGuid());
                });

            Assert.Multiple(() =>
            {
                // Make sure that the user has changed back.
                Assert.That(ExecutionContext.HasCurrent, Is.True);
                Assert.That(ExecutionContext.Current.UserName, Is.EqualTo("USER_NAME"));
            });
        }
    }
}