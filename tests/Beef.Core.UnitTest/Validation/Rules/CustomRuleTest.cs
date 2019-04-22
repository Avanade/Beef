// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class CustomRuleTest
    {
        [Test]
        public void Validate()
        {
            var v1 = "Abc".Validate().Custom(x => x.CreateErrorMessage("Test")).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Test", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
