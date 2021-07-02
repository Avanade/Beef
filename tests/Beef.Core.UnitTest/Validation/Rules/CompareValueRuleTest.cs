// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class CompareValueRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate()
        {
            var v1 = await 1.Validate().CompareValue(CompareOperator.Equal, 1).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 1.Validate().CompareValue(CompareOperator.Equal, 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be equal to 2.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 1.Validate().CompareValue(CompareOperator.GreaterThan, 0).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 1.Validate().CompareValue(CompareOperator.GreaterThan, 2, "Two").RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be greater than Two.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Nullable()
        {
            int? v = 1;
            var v1 = await v.Validate().CompareValue(CompareOperator.Equal, 1).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await v.Validate().CompareValue(CompareOperator.Equal, 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be equal to 2.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
