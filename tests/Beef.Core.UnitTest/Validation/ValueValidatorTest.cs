// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Core.UnitTest.Validation.Entities;
using Beef.Entities;
using Beef.Validation;
using NUnit.Framework;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class ValueValidatorTest
    {
        [Test]
        public void Run_ErrorWithException()
        {
            Assert.Throws<ValidationException>(() => new ValueValidator<TestData, int>(x => x.CountA, 0).Mandatory().Run(true));
        }

        [Test]
        public void Run_ErrorWithResult()
        {
            var r = new ValueValidator<TestData, int>(x => x.CountA, 0).Mandatory().Run();
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Count A is required.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("CountA", r.Messages[0].Property);
        }

        [Test]
        public void Run_NoError()
        {
            var r = new ValueValidator<TestData, int>(x => x.CountA, 1).Mandatory().Run();
            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasError);
        }

        [Test]
        public void Run_Common_Error()
        {
            var cv = CommonValidator<int>.Create(v => v.Mandatory());

            var r = new ValueValidator<TestData, int>(x => x.CountA, 0).Common(cv).Run();
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Count A is required.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("CountA", r.Messages[0].Property);
        }
    }
}
