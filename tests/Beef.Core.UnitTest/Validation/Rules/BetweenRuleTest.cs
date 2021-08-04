// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class BetweenRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate()
        {
            var v1 = await 1.Validate().Between(1, 10).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 5.Validate().Between(1, 10).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 1.Validate().Between(2, 10).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between 2 and 10.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 10.Validate().Between(1, 10).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 11.Validate().Between(1, 10, "One", "Ten").RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between One and Ten.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 2.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 5.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 1.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between 1 and 10 (exclusive).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 9.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 10.Validate().Between(1, 10, "One", "Ten", exclusiveBetween: true).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between One and Ten (exclusive).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Nullable()
        {
            int? v = null;
            var v1 = await v.Validate().Between(1, 10).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between 1 and 10.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v = 1;
            v1 = await v.Validate().Between(1, 10).RunAsync();
            Assert.IsFalse(v1.HasError);

            v = 5;
            v1 = await v.Validate().Between(1, 10).RunAsync();
            Assert.IsFalse(v1.HasError);

            v = 1;
            v1 = await v.Validate().Between(2, 10).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between 2 and 10.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v = 10;
            v1 = await v.Validate().Between(1, 10).RunAsync();
            Assert.IsFalse(v1.HasError);

            v = 11;
            v1 = await v.Validate().Between(1, 10, "One", "Ten").RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between One and Ten.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v = 2;
            v1 = await v.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v = 5;
            v1 = await v.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v = 1;
            v1 = await v.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between 1 and 10 (exclusive).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v = 9;
            v1 = await v.Validate().Between(1, 10, exclusiveBetween: true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v = 10;
            v1 = await v.Validate().Between(1, 10, "One", "Ten", exclusiveBetween: true).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be between One and Ten (exclusive).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
