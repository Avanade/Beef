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

            var v1 = await new Dictionary<string, TestItem>().Validate("Dict").Dictionary(item: DictionaryRuleValue.Create<string, TestItem>(iv)).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await new Dictionary<string, TestItem> { { "k1", new TestItem() } }.Validate("Dict").Dictionary(item: DictionaryRuleValue.Create<string, TestItem>(iv)).RunAsync();
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

            v1 = await new Dictionary<string, TestItem> { { "k1", null } }.Validate("Dict").Dictionary(allowNullItems: true).RunAsync();
            Assert.IsFalse(v1.HasError);
        }
    }
}