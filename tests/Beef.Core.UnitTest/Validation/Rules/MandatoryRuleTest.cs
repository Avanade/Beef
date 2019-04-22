// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using Beef.Entities;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class MandatoryRuleTest
    {
        [Test]
        public void Validate_String()
        {
            var v1 = "XXX".Validate().Mandatory().Run();
            Assert.IsFalse(v1.HasError);

            v1 = ((string)null).Validate().Mandatory().Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = (string.Empty).Validate().Mandatory().Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public void Validate_Int32()
        {
            var v1 = (123).Validate().Mandatory().Run();
            Assert.IsFalse(v1.HasError);

            v1 = (0).Validate().Mandatory().Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            var v2 = ((int?)123).Validate().Mandatory().Run();
            Assert.IsFalse(v2.HasError);

            v2 = ((int?)0).Validate().Mandatory().Run();
            Assert.IsFalse(v2.HasError);

            v2 = ((int?)null).Validate().Mandatory().Run();
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
        public void Validate_Entity()
        {
            Foo foo = new Foo();
            var v1 = foo.Validate().Mandatory().Run();
            Assert.IsFalse(v1.HasError);

            foo = null;
            v1 = foo.Validate().Mandatory().Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
