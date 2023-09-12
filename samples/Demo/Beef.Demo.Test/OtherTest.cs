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
            Assert.IsTrue(ss.IsInitial);

            ss.Ingredient = "special herbs and spices";
            Assert.IsTrue(ss.IsInitial);
        }

        [Test]
        public void Verify_Default_NotEquals_NotInitial()
        {
            var ss = new SpecialSauce
            {
                Ingredient = "special herbs and_spices"
            };

            Assert.IsFalse(ss.IsInitial);

            ss.Ingredient = "special herbs and spices";
            Assert.IsTrue(ss.IsInitial);
        }

        [Test]
        public void Verify_CompositeKey()
        {
            var ss = new SpecialSauce { Key1 = "A", Key2 = "B" };
            Assert.AreEqual(SpecialSauce.CreatePrimaryKey("A", "B"), ss.PrimaryKey);
            Assert.AreNotEqual(SpecialSauce.CreatePrimaryKey("A", "C"), ss.PrimaryKey);
        }

        [Test]
        public void Verify_IsChanged()
        {
            var p = new Person { FirstName = "Bill", LastName = "Gates", Address = new Address { Street = "1 Main St", City = "Portland" } };
            Assert.IsTrue(p.IsChanged);
            Assert.IsTrue(p.Address.IsChanged);

            p.AcceptChanges();
            Assert.IsFalse(p.IsChanged);
            Assert.IsFalse(p.Address.IsChanged);

            p.FirstName = "William";
            Assert.IsTrue(p.IsChanged);
            Assert.IsFalse(p.Address.IsChanged);
            p.AcceptChanges();

            p.Address.Street = "2 Main St";
            Assert.IsTrue(p.IsChanged);
            Assert.IsTrue(p.Address.IsChanged);
        }
    }
}