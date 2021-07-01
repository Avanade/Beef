// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class ExistsRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate_Value()
        {
            var v1 = await 123.Validate().Exists(x => true).RunAsync();
            Assert.IsFalse(v1.HasError);
            
            v1 = await 123.Validate().Exists(x => false).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 123.Validate().Exists(true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 123.Validate().Exists(false).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 123.Validate().Exists(_ => Task.FromResult(true)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 123.Validate().Exists(_ => Task.FromResult(false)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 123.Validate().Exists(x => Task.FromResult(new object())).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 123.Validate().Exists(x => Task.FromResult((object)null)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 123.Validate().AgentExists(x => Task.FromResult(new Beef.WebApi.WebApiAgentResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)))).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 123.Validate().AgentExists(x => Task.FromResult(new Beef.WebApi.WebApiAgentResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound)))).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
