// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Core.UnitTest.Validation.Entities;
using Beef.Entities;
using Beef.Validation;
using NUnit.Framework;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class CommonValidatorTest
    {
        private static readonly CommonValidator<string> _cv = CommonValidator<string>.Create(v => v.String(5).Must(x => x.Value != "XXXXX"));
        private static readonly CommonValidator<int?> _cv2 = CommonValidator<int?>.Create(v => v.CompareValue(CompareOperator.NotEqual, 1));

        [Test]
        public void Validate()
        {
            var r = _cv.Validate("XXXXXX");
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Value must not exceed 5 characters in length.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value", r.Messages[0].Property);

            r = _cv.Validate("XXXXX", "Name");
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Name is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Name", r.Messages[0].Property);

            r = _cv.Validate("XXX", "Name");
            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasError);
        }

        [Test]
        public void Common()
        {
            var r = Validator<TestData>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory().Common(_cv))
                .Validate(new TestData { Text = "XXXXXX" });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Text must not exceed 5 characters in length.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = Validator<TestData>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory().Common(_cv))
                .Validate(new TestData { Text = "XXXXX" });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Text is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = Validator<TestData>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory().Common(_cv))
                .Validate(new TestData { Text = "XXX" });

            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public void Validate_Nullable()
        {
            int? v = 1;
            var r = _cv2.Validate(v);
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Value must not be equal to 1.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public void Common_Nullable()
        {
            var r = Validator<TestData>.Create()
                .HasProperty(x => x.CountB, p => p.Mandatory().Common(_cv2))
                .Validate(new TestData { CountB = 1 });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Count B must not be equal to 1.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("CountB", r.Messages[0].Property);
        }
    }
}
