using Beef.Validation;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class GenericValidatorTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        public class IntValidator : GenericValidator<int>
        {
            public IntValidator()
            {
                Rule().Text("Count").Mandatory().CompareValue(CompareOperator.GreaterThanEqual, 10).CompareValue(CompareOperator.LessThanEqual, 20);
            }
        }

        [Test]
        public async Task Test()
        {
            var iv = new IntValidator();
            var vr = await iv.ValidateAsync(8);
            Assert.IsTrue(vr.HasErrors);
            Assert.AreEqual(1, vr.Messages.Count);
            Assert.AreEqual("Count must be greater than or equal to 10.", vr.Messages[0].Text);
            Assert.AreEqual("Value", vr.Messages[0].Property);
        }

        [Test]
        public async Task Test2()
        {
            var iv = Validator.CreateGeneric<int>().Rule(r => r.Text("Count").Mandatory().CompareValue(CompareOperator.GreaterThanEqual, 10).CompareValue(CompareOperator.LessThanEqual, 20));

            var vr = await iv.ValidateAsync(8);
            Assert.IsTrue(vr.HasErrors);
            Assert.AreEqual(1, vr.Messages.Count);
            Assert.AreEqual("Count must be greater than or equal to 10.", vr.Messages[0].Text);
            Assert.AreEqual("Value", vr.Messages[0].Property);
        }
    }
}