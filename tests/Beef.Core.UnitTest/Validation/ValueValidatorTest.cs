// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Core.UnitTest.Validation.Entities;
using Beef.Entities;
using Beef.Validation;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class ValueValidatorTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public void Run_ErrorWithException()
        {
            Assert.ThrowsAsync<ValidationException>(async () => await new ValueValidator<TestData, int>(x => x.CountA, 0).Mandatory().RunAsync(true));
        }

        [Test]
        public async Task Run_ErrorWithResult()
        {
            var r = await new ValueValidator<TestData, int>(x => x.CountA, 0).Mandatory().RunAsync();
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Count A is required.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("CountA", r.Messages[0].Property);
        }

        [Test]
        public async Task Run_NoError()
        {
            var r = await new ValueValidator<TestData, int>(x => x.CountA, 1).Mandatory().RunAsync();
            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasError);
        }

        [Test]
        public async Task Run_Common_Error()
        {
            var cv = CommonValidator.Create<int>(v => v.Mandatory());

            var r = await new ValueValidator<TestData, int>(x => x.CountA, 0).Common(cv).RunAsync();
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Count A is required.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("CountA", r.Messages[0].Property);
        }

        [Test]
        public async Task Run_Will_Null()
        {
            string name = null;
            var r = await name.Validate().Mandatory().RunAsync();
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Value is required.", r.Messages[0].Text);
        }
    }
}