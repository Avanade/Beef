// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using Beef.Entities;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class DuplicateRuleTest
    {
        [Test]
        public void Validate_Value()
        {
            var v1 = 123.Validate().Duplicate(x => false).Run();
            Assert.IsFalse(v1.HasError);
            
            v1 = 123.Validate().Duplicate(x => true).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value already exists and would result in a duplicate.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = 123.Validate().Duplicate(() => false).Run();
            Assert.IsFalse(v1.HasError);

            v1 = 123.Validate().Duplicate(() => true).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value already exists and would result in a duplicate.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
