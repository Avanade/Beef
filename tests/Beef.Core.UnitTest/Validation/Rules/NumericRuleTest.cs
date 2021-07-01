// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class NumericRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate_AllowNegatives()
        {
            var v1 = await (123f).Validate().Numeric().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (-123f).Validate().Numeric().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not be negative.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await (-123f).Validate().Numeric(true).RunAsync();
            Assert.IsFalse(v1.HasError);

            var v2 = await (123d).Validate().Numeric().RunAsync();
            Assert.IsFalse(v2.HasError);

            v2 = await (-123d).Validate().Numeric().RunAsync();
            Assert.IsTrue(v2.HasError);
            Assert.AreEqual(1, v2.Messages.Count);
            Assert.AreEqual("Value must not be negative.", v2.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v2.Messages[0].Type);
            Assert.AreEqual("Value", v2.Messages[0].Property);

            v2 = await (-123d).Validate().Numeric(true).RunAsync();
            Assert.IsFalse(v2.HasError);
        }
    }
}
