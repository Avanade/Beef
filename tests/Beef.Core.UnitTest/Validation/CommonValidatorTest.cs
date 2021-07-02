// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Core.UnitTest.Validation.Entities;
using Beef.Entities;
using Beef.Validation;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class CommonValidatorTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        private static readonly CommonValidator<string> _cv = CommonValidator.Create<string>(v => v.String(5).Must(x => x.Value != "XXXXX"));
        private static readonly CommonValidator<int?> _cv2 = CommonValidator.Create<int?>(v => v.CompareValue(CompareOperator.NotEqual, 1));

        [Test]
        public async Task Validate()
        {
            var r = await _cv.ValidateAsync("XXXXXX");
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Value must not exceed 5 characters in length.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value", r.Messages[0].Property);

            r = await _cv.ValidateAsync("XXXXX", "Name");
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Name is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Name", r.Messages[0].Property);

            r = await _cv.ValidateAsync("XXX", "Name");
            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasError);
        }

        [Test]
        public async Task Common()
        {
            var r = await Validator.Create<TestData>()
                .HasProperty(x => x.Text, p => p.Mandatory().Common(_cv))
                .ValidateAsync(new TestData { Text = "XXXXXX" });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Text must not exceed 5 characters in length.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = await Validator.Create<TestData>()
                .HasProperty(x => x.Text, p => p.Mandatory().Common(_cv))
                .ValidateAsync(new TestData { Text = "XXXXX" });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Text is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = await Validator.Create<TestData>()
                .HasProperty(x => x.Text, p => p.Mandatory().Common(_cv))
                .ValidateAsync(new TestData { Text = "XXX" });

            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Validate_Nullable()
        {
            int? v = 1;
            var r = await _cv2.ValidateAsync(v);
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasError);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Value must not be equal to 1.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public async Task Common_Nullable()
        {
            var r = await Validator.Create<TestData>()
                .HasProperty(x => x.CountB, p => p.Mandatory().Common(_cv2))
                .ValidateAsync(new TestData { CountB = 1 });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Count B must not be equal to 1.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("CountB", r.Messages[0].Property);
        }
    }
}