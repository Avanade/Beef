// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class NumericRuleTest
    {
        [Test]
        public void Validate_AllowNegatives()
        {
            var v1 = (123f).Validate().Numeric().Run();
            Assert.IsFalse(v1.HasError);

            v1 = (-123f).Validate().Numeric().Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not be negative.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = (-123f).Validate().Numeric(true).Run();
            Assert.IsFalse(v1.HasError);

            var v2 = (123d).Validate().Numeric().Run();
            Assert.IsFalse(v2.HasError);

            v2 = (-123d).Validate().Numeric().Run();
            Assert.IsTrue(v2.HasError);
            Assert.AreEqual(1, v2.Messages.Count);
            Assert.AreEqual("Value must not be negative.", v2.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v2.Messages[0].Type);
            Assert.AreEqual("Value", v2.Messages[0].Property);

            v2 = (-123d).Validate().Numeric(true).Run();
            Assert.IsFalse(v2.HasError);
        }
    }
}
