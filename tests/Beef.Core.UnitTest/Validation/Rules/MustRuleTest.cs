// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class MustRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate_Value()
        {
            var v1 = await 123.Validate().Must(x => true).RunAsync();
            Assert.IsFalse(v1.HasError);
            
            v1 = await 123.Validate().Must(x => false).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is invalid.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await 123.Validate().Must(() => true).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await 123.Validate().Must(() => false).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is invalid.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
