// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Entities;
using System.Text.RegularExpressions;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class StringRuleTest
    {
        [Test]
        public void Validate_MinLength()
        {
            var v1 = "Abc".Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = "Ab".Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = "A".Validate().String(2, 5).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must be at least 2 characters in length.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = string.Empty.Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = ((string)null).Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);
        }

        [Test]
        public void Validate_MaxLength()
        {
            var v1 = "Abc".Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = "Abcde".Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = "Abcdef".Validate().String(2, 5).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not exceed 5 characters in length.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = string.Empty.Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = ((string)null).Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);
        }

        [Test]
        public void Validate_Regex()
        {
            var r = new Regex("[a-zA-Z]$");
            var v1 = "Abc".Validate().String(r).Run();
            Assert.IsFalse(v1.HasError);

            v1 = "123".Validate().String(r).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value is invalid.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = string.Empty.Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);

            v1 = ((string)null).Validate().String(2, 5).Run();
            Assert.IsFalse(v1.HasError);
        }
    }
}
