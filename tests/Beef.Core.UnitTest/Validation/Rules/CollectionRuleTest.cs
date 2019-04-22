// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using System.Collections.Generic;
using Beef.Entities;
using static Beef.Core.UnitTest.Validation.ValidatorTest;
using Beef.Validation.Rules;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class CollectionRuleTest
    {
        [Test]
        public void Validate()
        {
            var v1 = new int[] { 1 }.Validate().Collection(2).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must have at least 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = new int[] { 1 }.Validate().Collection(1).Run();
            Assert.IsFalse(v1.HasError);

            v1 = new int[] { 1, 2, 3 }.Validate().Collection(maxCount: 2).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not exceed 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = new int[] { 1, 2 }.Validate().Collection(maxCount: 2).Run();
            Assert.IsFalse(v1.HasError);

            v1 = ((int[])null).Validate().Collection(1).Run();
            Assert.IsFalse(v1.HasError);

            v1 = new int[0].Validate().Collection(1).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must have at least 1 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = new int[] { 1, 2, 3 }.Validate().Collection().Run();
            Assert.IsFalse(v1.HasError);
        }

        [Test]
        public void Validate2()
        {
            var v1 = new List<int> { 1 }.Validate().Collection(2).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must have at least 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public void Validate_Item()
        {
            var iv = Validator<TestItem>.Create().HasProperty(x => x.Code, p => p.Mandatory());

            var v1 = new TestItem[0].Validate().Collection(item: new CollectionRuleItem<TestItem>(iv)).Run();
            Assert.IsFalse(v1.HasError);

            v1 = new TestItem[] { new TestItem() }.Validate().Collection(item: new CollectionRuleItem<TestItem>(iv)).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Code is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value[0].Code", v1.Messages[0].Property);
        }

        [Test]
        public void Validate_Item_Duplicates()
        {
            var iv = Validator<TestItem>.Create().HasProperty(x => x.Code, p => p.Mandatory());

            var v1 = new TestItem[0].Validate().Collection(item: new CollectionRuleItem<TestItem>(iv).DuplicateCheck(x => x.Code)).Run();
            Assert.IsFalse(v1.HasError);

            var tis = new TestItem[] { new TestItem { Code = "ABC", Text = "Abc" }, new TestItem { Code = "DEF", Text = "Def" }, new TestItem { Code = "GHI", Text = "Ghi" } };

            v1 = tis.Validate().Collection(item: new CollectionRuleItem<TestItem>(iv).DuplicateCheck(x => x.Code)).Run();
            Assert.IsFalse(v1.HasError);

            tis[2].Code = "ABC";
            v1 = tis.Validate().Collection(item: new CollectionRuleItem<TestItem>(iv).DuplicateCheck(x => x.Code)).Run();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value contains duplicates; Code value 'ABC' specified more than once.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }
    }
}
