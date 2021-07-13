// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using Beef.Validation;
using System.Collections.Generic;
using Beef.Entities;
using static Beef.Core.UnitTest.Validation.ValidatorTest;
using Beef.Validation.Rules;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class CollectionRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate_Errors()
        {
            var v1 = await new int[] { 1 }.Validate().Collection(2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must have at least 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await new int[] { 1 }.Validate().Collection(1).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new int[] { 1, 2, 3 }.Validate().Collection(maxCount: 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not exceed 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await new int[] { 1, 2 }.Validate().Collection(maxCount: 2).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await ((int[])null).Validate().Collection(1).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new int[0].Validate().Collection(1).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must have at least 1 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await new int[] { 1, 2, 3 }.Validate().Collection().RunAsync();
            Assert.IsFalse(v1.HasError);
        }

        [Test]
        public async Task Validate_MinCount()
        {
            var v1 = await new List<int> { 1 }.Validate().Collection(2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must have at least 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Item()
        {
            var iv = Validator.Create<TestItem>().HasProperty(x => x.Code, p => p.Mandatory());

            var v1 = await new TestItem[0].Validate().Collection(item: CollectionRuleItem.Create(iv)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new TestItem[] { new TestItem() }.Validate().Collection(item: CollectionRuleItem.Create(iv)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Code is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value[0].Code", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_ItemInt()
        {
            var iv = Validator.CreateCommon<int>(r => r.Text("Number").CompareValue(CompareOperator.LessThanEqual, 5));

            var v1 = await new int[] { 1, 2, 3, 4, 5 }.Validate().Collection(item: CollectionRuleItem.Create(iv)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new int[] { 6, 2, 3, 4, 5 }.Validate().Collection(item: CollectionRuleItem.Create(iv)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Number must be less than or equal to 5.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value[0]", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Item_Null()
        {
            var v1 = await new TestItem[] { new TestItem() }.Validate().Collection().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new TestItem[] { null }.Validate().Collection().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value contains one or more items that are not specified.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await new TestItem[] { null }.Validate().Collection(allowNullItems: true).RunAsync();
            Assert.IsFalse(v1.HasError);
        }

        [Test]
        public async Task Validate_Item_Duplicates()
        {
            var iv = Validator.Create<TestItem>().HasProperty(x => x.Code, p => p.Mandatory());

            var v1 = await new TestItem[0].Validate().Collection(item: CollectionRuleItem.Create(iv).DuplicateCheck(x => x.Code)).RunAsync();
            Assert.IsFalse(v1.HasError);

            var tis = new TestItem[] { new TestItem { Code = "ABC", Text = "Abc" }, new TestItem { Code = "DEF", Text = "Def" }, new TestItem { Code = "GHI", Text = "Ghi" } };

            v1 = await tis.Validate().Collection(item:  CollectionRuleItem.Create(iv).DuplicateCheck(x => x.Code)).RunAsync();
            Assert.IsFalse(v1.HasError);

            tis[2].Code = "ABC";
            v1 = await tis.Validate().Collection(item: CollectionRuleItem.Create(iv).DuplicateCheck(x => x.Code)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value contains duplicates; Code value 'ABC' specified more than once.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Ints()
        {
            var v1 = await new int[] { 1, 2, 3, 4 }.Validate(name: "Array").Collection(maxCount: 5).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new int[] { 1, 2, 3, 4 }.Validate(name: "Array").Collection(maxCount: 3).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Array must not exceed 3 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Array", v1.Messages[0].Property);
        }
    }
}