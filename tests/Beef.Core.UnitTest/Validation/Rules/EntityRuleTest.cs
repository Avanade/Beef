using Beef.Entities;
using Beef.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Beef.Core.UnitTest.Validation.ValidatorTest;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class EntityRuleTest
    {
        private static readonly IValidator _tiv = Validator.Create<TestItem>().HasProperty(x => x.Code, p => p.Mandatory());
        private static readonly IValidator _tev = Validator.Create<TestEntity>().HasProperty(x => x.Item, p => p.Entity(_tiv));

        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate()
        {
            var te = new TestEntity { Item = new TestItem() };
            var v1 = await te.Validate().Entity(_tev).RunAsync();

            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Code is required.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value.Item.Code", v1.Messages[0].Property);
        }
    }
}