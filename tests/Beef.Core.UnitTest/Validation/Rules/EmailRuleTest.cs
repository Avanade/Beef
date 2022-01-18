using Beef.Validation;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class EmailRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate()
        {
            var v1 = await ((string)null).Validate().Email().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await "blah@.com".Validate().Email().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await "blah.com".Validate().Email().RunAsync();
            Assert.IsTrue(v1.HasError);

            v1 = await "@blah.com".Validate().Email().RunAsync();
            Assert.IsTrue(v1.HasError);

            v1 = await "blah@".Validate().Email().RunAsync();
            Assert.IsTrue(v1.HasError);

            v1 = await $"mynameis@{new string('x', 250)}.com".Validate().Email().RunAsync();
            Assert.IsTrue(v1.HasError);

            v1 = await $"mynameis@{new string('x', 250)}.com".Validate().Email(null).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await $"mynameis@{new string('x', 500)}.com".Validate().Email(null).RunAsync();
            Assert.IsFalse(v1.HasError);
        }
    }
}