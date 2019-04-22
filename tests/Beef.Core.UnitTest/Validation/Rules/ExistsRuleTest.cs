// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using Beef.Entities;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class ExistsRuleTest
    {
        [Test]
        public void Validate_Value()
        {
            var v1 = 123.Validate().Exists(x => true).Run();
            Assert.IsFalse(v1.HasError);
            
            v1 = 123.Validate().Exists(x => false).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = 123.Validate().Exists(() => true).Run();
            Assert.IsFalse(v1.HasError);

            v1 = 123.Validate().Exists(() => false).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = 123.Validate().Exists(x => new object()).Run();
            Assert.IsFalse(v1.HasError);

            v1 = 123.Validate().Exists(x => null).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = 123.Validate().AgentExists(x => new Beef.WebApi.WebApiAgentResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK))).Run();
            Assert.IsFalse(v1.HasError);

            v1 = 123.Validate().AgentExists(x => new Beef.WebApi.WebApiAgentResult(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NotFound))).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is not found; a valid value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
