using Beef.Demo.Business.Entities;
using NUnit.Framework;

namespace Beef.Demo.Test
{
    [TestFixture]
    public class OtherTest
    {
        [Test]
        public void Verify_Default_Equals_Initial()
        {
            var ss = new SpecialSauce();
            Assert.That(ss.IsInitial, Is.True);

            ss.Ingredient = "special herbs and spices";
            Assert.That(ss.IsInitial, Is.True);
        }

        [Test]
        public void Verify_Default_NotEquals_NotInitial()
        {
            var ss = new SpecialSauce
            {
                Ingredient = "special herbs and_spices"
            };

            Assert.That(ss.IsInitial, Is.False);

            ss.Ingredient = "special herbs and spices";
            Assert.That(ss.IsInitial, Is.True);
        }

        [Test]
        public void Verify_CompositeKey()
        {
            var ss = new SpecialSauce { Key1 = "A", Key2 = "B" };
            Assert.That(ss.PrimaryKey, Is.EqualTo(SpecialSauce.CreatePrimaryKey("A", "B")));
            Assert.That(ss.PrimaryKey, Is.Not.EqualTo(SpecialSauce.CreatePrimaryKey("A", "C")));
        }

        [Test]
        public void Verify_IsChanged()
        {
            var p = new Person { FirstName = "Bill", LastName = "Gates", Address = new Address { Street = "1 Main St", City = "Portland" } };
            Assert.Multiple(() =>
            {
                Assert.That(p.IsChanged, Is.True);
                Assert.That(p.Address.IsChanged, Is.True);
            });

            p.AcceptChanges();
            Assert.Multiple(() =>
            {
                Assert.That(p.IsChanged, Is.False);
                Assert.That(p.Address.IsChanged, Is.False);
            });

            p.FirstName = "William";
            Assert.Multiple(() =>
            {
                Assert.That(p.IsChanged, Is.True);
                Assert.That(p.Address.IsChanged, Is.False);
            });
            p.AcceptChanges();

            p.Address.Street = "2 Main St";
            Assert.Multiple(() =>
            {
                Assert.That(p.IsChanged, Is.True);
                Assert.That(p.Address.IsChanged, Is.True);
            });
        }
    }
}