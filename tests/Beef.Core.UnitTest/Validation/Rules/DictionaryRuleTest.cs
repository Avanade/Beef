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
    public class DictionaryRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate()
        {
            var v1 = await new Dictionary<string, string> { { "k1", "v1" } }.Validate("Dict").Dictionary(2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Dict must have at least 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict", v1.Messages[0].Property);

            v1 = await new Dictionary<string, string> { { "k1", "v1" } }.Validate("Dict").Dictionary(1).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" }, { "k3", "v3" } }.Validate("Dict").Dictionary(maxCount: 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Dict must not exceed 2 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict", v1.Messages[0].Property);

            v1 = await new Dictionary<string, string> { { "k1", "v1" }, { "k2", "v2" }, { "k3", "v3" } }.Validate("Dict").Dictionary(maxCount: 3).RunAsync();
            Assert.IsFalse(v1.HasError);

            //v1 = await ((int[])null).Validate().Collection(1).RunAsync();
            v1 = await ((Dictionary<string, string>)null).Validate().Collection(1).RunAsync();
            Assert.IsFalse(v1.HasError);

            //v1 = await new int[0].Validate().Collection(1).RunAsync();
            v1 = await new Dictionary<string, string> { }.Validate("Dict").Dictionary(1).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Dict must have at least 1 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Value()
        {
            var iv = Validator.Create<TestItem>().HasProperty(x => x.Code, p => p.Mandatory());

            var v1 = await new Dictionary<string, TestItem>().Validate("Dict").Dictionary(item: DictionaryRuleItem.Create<string, TestItem>(value: iv)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, TestItem> { { "k1", new TestItem() } }.Validate("Dict").Dictionary(item: DictionaryRuleItem.Create<string, TestItem>(value: iv)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Code is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict[k1].Code", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Null_Value()
        {
            var v1 = await new Dictionary<string, TestItem> { { "k1", new TestItem() } }.Validate("Dict").Dictionary().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, TestItem> { { "k1", null } }.Validate("Dict").Dictionary().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Dict contains one or more values that are not specified.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict", v1.Messages[0].Property);

            v1 = await new Dictionary<string, TestItem> { { "k1", null } }.Validate("Dict").Dictionary(allowNullValues: true).RunAsync();
            Assert.IsFalse(v1.HasError);
        }

        [Test]
        public async Task Validate_Ints()
        {
            var v1 = await new Dictionary<string, int> { { "k1", 1 }, { "k2", 2 }, { "k3", 3 }, { "k4", 4 } }.Validate("Dict").Dictionary(maxCount: 4).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, int> { { "k1", 1 }, { "k2", 2 }, { "k3", 3 }, { "k4", 4} }.Validate("Dict").Dictionary(maxCount: 3).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Dict must not exceed 3 item(s).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_Key()
        {
            var kv = Validator.CreateCommon<string>(r => r.Text("Key").Mandatory().String(2));

            var v1 = await new Dictionary<string, int> { { "k1", 1 }, { "k2", 2 } }.Validate("Dict").Dictionary(item: DictionaryRuleItem.Create<string, int>(kv)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, int> { { "k1", 1 }, { "k2x", 2 } }.Validate("Dict").Dictionary(item: DictionaryRuleItem.Create<string, int>(kv)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Key must not exceed 2 characters in length.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict[k2x]", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_KeyAndValue()
        {
            var kv = Validator.CreateCommon<string>(r => r.Text("Key").Mandatory().String(2));
            var vv = Validator.CreateCommon<int>(r => r.Mandatory().CompareValue(CompareOperator.LessThanEqual, 10));

            var v1 = await new Dictionary<string, int> { { "k1", 1 }, { "k2", 2 } }.Validate("Dict").Dictionary(item: DictionaryRuleItem.Create<string, int>(kv, vv)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, int> { { "k1", 11 }, { "k2x", 2 } }.Validate("Dict").Dictionary(item: DictionaryRuleItem.Create<string, int>(kv, vv)).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(2, v1.Messages.Count);
            Assert.AreEqual("Value must be less than or equal to 10.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Dict[k1]", v1.Messages[0].Property);
            Assert.AreEqual("Key must not exceed 2 characters in length.", v1.Messages[1].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[1].Type);
            Assert.AreEqual("Dict[k2x]", v1.Messages[1].Property);
        }
    }
}