// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class CompareValueRuleTest
    {
        [Test]
        public void Validate()
        {
            var v1 = 1.Validate().CompareValue(CompareOperator.Equal, 1).Run();
            Assert.IsFalse(v1.HasError);

            v1 = 1.Validate().CompareValue(CompareOperator.Equal, 2).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be equal to 2.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = 1.Validate().CompareValue(CompareOperator.GreaterThan, 0).Run();
            Assert.IsFalse(v1.HasError);

            v1 = 1.Validate().CompareValue(CompareOperator.GreaterThan, 2, "Two").Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be greater than Two.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public void Validate_Nullable()
        {
            int? v = 1;
            var v1 = v.Validate().CompareValue(CompareOperator.Equal, 1).Run();
            Assert.IsFalse(v1.HasError);

            v1 = v.Validate().CompareValue(CompareOperator.Equal, 2).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be equal to 2.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
