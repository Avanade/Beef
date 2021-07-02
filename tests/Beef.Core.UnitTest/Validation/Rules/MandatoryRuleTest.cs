// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using Beef.Entities;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class MandatoryRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate_String()
        {
            var v1 = await "XXX".Validate().Mandatory().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await ((string)null).Validate().Mandatory().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await (string.Empty).Validate().Mandatory().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Int32()
        {
            var v1 = await (123).Validate().Mandatory().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (0).Validate().Mandatory().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            var v2 = await ((int?)123).Validate().Mandatory().RunAsync();
            Assert.IsFalse(v2.HasError);

            v2 = await ((int?)0).Validate().Mandatory().RunAsync();
            Assert.IsFalse(v2.HasError);

            v2 = await ((int?)null).Validate().Mandatory().RunAsync();
            Assert.IsTrue(v2.HasError);
            Assert.AreEqual(1, v2.Messages.Count);
            Assert.AreEqual("Value is required.", v2.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v2.Messages[0].Type);
            Assert.AreEqual("Value", v2.Messages[0].Property);
        }

        public class Foo
        {
            public string Bar { get; set; }
        }

        [Test]
        public async Task Validate_Entity()
        {
            Foo foo = new Foo();
            var v1 = await foo.Validate().Mandatory().RunAsync();
            Assert.IsFalse(v1.HasError);

            foo = null;
            v1 = await foo.Validate().Mandatory().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
