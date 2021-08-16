// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;
using Beef.Entities;
using Beef.RefData;
using Beef.Validation;
using NUnit.Framework;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class ReferenceDataValidatorTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        public class Gender : ReferenceDataBaseInt32
        {
            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        public class GenderValidator : ReferenceDataValidatorBase<Gender, GenderValidator> { }

        [Test]
        public async Task Validate_Null()
        {
            var r = await (new ReferenceDataValidator<Gender>()).ValidateAsync(null);
            Assert.IsNotNull(r);
            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Validate_Empty()
        {
            var r = await GenderValidator.Default.ValidateAsync(new Gender());
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(3, r.Messages.Count);
            Assert.AreEqual("Id", r.Messages[0].Property);
            Assert.AreEqual("Code", r.Messages[1].Property);
            Assert.AreEqual("Text", r.Messages[2].Property);
        }

        [Test]
        public async Task Validate_Dates()
        {
            var r = await GenderValidator.Default.ValidateAsync(new Gender { Id = 1, Code = "X", Text = "XX", StartDate = new DateTime(2000, 01, 01), EndDate = new DateTime(1950, 01, 01) });
            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("End Date must be greater than or equal to Start Date.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("EndDate", r.Messages[0].Property);
        }
    }
}
