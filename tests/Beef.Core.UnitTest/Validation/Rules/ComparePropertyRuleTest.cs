// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Core.UnitTest.Validation.Entities;
using Beef.Entities;
using Beef.Validation;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class ComparePropertyRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate()
        {
            var v = Validator.Create<TestData>()
                .HasProperty(x => x.DateA, p => p.CompareValue(CompareOperator.GreaterThan, new DateTime(1950, 1, 1), "Minimum"))
                .HasProperty(x => x.DateB, p => p.CompareProperty(CompareOperator.GreaterThanEqual, y => y.DateA));

            // Date B will be bad.
            var v1 = await v.ValidateAsync(new TestData { DateA = new DateTime(2000, 1, 1), DateB = new DateTime(1999, 1, 1) });
            Assert.IsNotNull(v1);
            Assert.IsTrue(v1.HasErrors);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Date B must be greater than or equal to Date A.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("DateB", v1.Messages[0].Property);

            // Date B should not validate as dependent DateA has already failed.
            var v2 = await v.ValidateAsync(new TestData { DateA = new DateTime(1949, 1, 1), DateB = new DateTime(1939, 1, 1) });
            Assert.IsNotNull(v2);
            Assert.IsTrue(v2.HasErrors);
            Assert.AreEqual(1, v2.Messages.Count);
            Assert.AreEqual("Date A must be greater than Minimum.", v2.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v2.Messages[0].Type);
            Assert.AreEqual("DateA", v2.Messages[0].Property);

            // All is a-ok.
            var v3 = await v.ValidateAsync(new TestData { DateA = new DateTime(2001, 1, 1), DateB = new DateTime(2001, 1, 1) });
            Assert.IsNotNull(v3);
            Assert.IsFalse(v3.HasErrors);
        }
    }
}
