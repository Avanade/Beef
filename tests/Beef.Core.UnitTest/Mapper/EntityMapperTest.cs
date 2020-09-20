// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Collections.Generic;
using NUnit.Framework;
using Beef.Mapper;

namespace Beef.Core.UnitTest.Mapper
{
    /// <summary>
    /// Summary description for EntityMapperTest
    /// </summary>
    [TestFixture]
    public class EntityMapperTest
    {
        [Test]
        public void GetByProperty()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Street, d => d.StreetX);

            Assert.IsNotNull(r.GetBySrceProperty(s => s.Street));
            Assert.IsNull(r.GetBySrceProperty(s => s.Name));

            Assert.IsNotNull(r.GetByDestProperty(d => d.StreetX));
            Assert.IsNull(r.GetByDestProperty(d => d.Codes));
        }


        #region MapToDest

        [Test]
        public void MapToDest_Null()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Name, d => d.Name)
                .MapToDest(null);

            Assert.IsNull(r);
        }

        [Test]
        public void MapToDest_StringNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Name, d => d.Name)
                .MapToDest(new PersonA());

            Assert.IsNotNull(r);
            Assert.IsNull(r.Name);
        }

        [Test]
        public void MapToDest_StringValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Name, d => d.Name)
                .MapToDest(new PersonA { Name = "AAA" });

            Assert.IsNotNull(r);
            Assert.AreEqual("AAA", r.Name);
        }

        [Test]
        public void MapToDest_NullableNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Salary, d => d.Salary)
                .MapToDest(new PersonA { Salary = null });

            Assert.IsNotNull(r);
            Assert.AreEqual(0, r.Salary);
        }

        [Test]
        public void MapToDest_NullableValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Salary, d => d.Salary)
                .MapToDest(new PersonA { Salary = 10m });

            Assert.IsNotNull(r);
            Assert.AreEqual(10m, r.Salary);
        }

        [Test]
        public void MapToDest_IntValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Age, d => d.Age)
                .MapToDest(new PersonA { Age = 10 });

            Assert.IsNotNull(r);
            Assert.AreEqual(10, r.Age);
        }

        [Test]
        public void MapToDest_ArrayNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Codes, d => d.Codes)
                .MapToDest(new PersonA { Codes = null });

            Assert.IsNotNull(r);
            Assert.IsNull(r.Codes);
        }

        [Test]
        public void MapToDest_ArrayEmpty()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Codes, d => d.Codes)
                .MapToDest(new PersonA { Codes = new int[0] { } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Codes);
            Assert.AreEqual(0, r.Codes.Length);
        }

        [Test]
        public void MapToDest_ArrayValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Codes, d => d.Codes)
                .MapToDest(new PersonA { Codes = new int[] { 1, 2 } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Codes);
            Assert.AreEqual(2, r.Codes.Length);
            Assert.AreEqual(1, r.Codes[0]);
            Assert.AreEqual(2, r.Codes[1]);
        }

        [Test]
        public void MapToDest_EntityNull_AutoPropMapper()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX)
                .MapToDest(new PersonA());

            Assert.IsNotNull(r);
            Assert.IsNull(r.AddressX);
        }

        [Test]
        public void MapToDest_EntityValue_AutoPropMapper()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX)
                .MapToDest(new PersonA { Address = new Address { Street = "AAA", City = "BBB" } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.AddressX);
            Assert.AreEqual("AAA", r.AddressX.Street);
            Assert.AreEqual("BBB", r.AddressX.City);
        }

        [Test]
        public void MapToDest_EntityValue_PropMapper()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX, p => p.SetMapper(EntityMapper.Create<Address, AddressX>().HasProperty(sa => sa.Street, da => da.City)))
                .MapToDest(new PersonA { Address = new Address { Street = "AAA", City = "BBB" } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.AddressX);
            Assert.IsNull(r.AddressX.Street);
            Assert.AreEqual("AAA", r.AddressX.City);
        }

        [Test]
        public void MapToDest_EntityValue_PropMapper2()
        {
            var r = new PersonB { AddressX = new AddressX { City = "123", Street = "456" } };

            EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX, p => p.SetMapper(EntityMapper.Create<Address, AddressX>().HasProperty(sa => sa.Street, da => da.City)))
                .MapToDest(new PersonA { Address = new Address { Street = "AAA", City = "BBB" } }, r);

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.AddressX);
            Assert.AreEqual("456", r.AddressX.Street);
            Assert.AreEqual("AAA", r.AddressX.City);
        }

        [Test]
        public void MapToDest_EntityValue_PropMapper3()
        {
            var r = new PersonB { AddressX = new AddressX { City = "123", Street = "456" } };

            EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX, p => p.SetMapper(EntityMapper.Create<Address, AddressX>().HasProperty(sa => sa.Street, da => da.City)))
                .MapToDest(new PersonA(), r);

            Assert.IsNotNull(r);
            Assert.IsNull(r.AddressX);
        }

        [Test]
        public void MapToDest_CollNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Addresses, d => d.Addresses)
                .MapToDest(new PersonA());

            Assert.IsNotNull(r);
            Assert.IsNull(r.Addresses);
        }

        [Test]
        public void MapToDest_CollEmpty()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Addresses, d => d.Addresses)
                .MapToDest(new PersonA {  Addresses = System.Array.Empty<Address>() });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Addresses);
            Assert.AreEqual(0, r.Addresses.Count);
        }

        [Test]
        public void MapToDest_CollValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Addresses, d => d.Addresses)
                .MapToDest(new PersonA { Addresses = new Address[] { new Address { Street = "AAA", City = "BBB" }, new Address { Street = "YYY", City = "ZZZ" }, } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Addresses);
            Assert.AreEqual(2, r.Addresses.Count);
        }

        #endregion

        #region MapToSrce

        [Test]
        public void MapToSrce_Null()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Name, d => d.Name)
                .MapToSrce(null);

            Assert.IsNull(r);
        }

        [Test]
        public void MapToSrce_StringNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Name, d => d.Name)
                .MapToSrce(new PersonB());

            Assert.IsNotNull(r);
            Assert.IsNull(r.Name);
        }

        [Test]
        public void MapToSrce_StringValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Name, d => d.Name)
                .MapToSrce(new PersonB { Name = "AAA" });

            Assert.IsNotNull(r);
            Assert.AreEqual("AAA", r.Name);
        }

        [Test]
        public void MapToSrce_NullableZero()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Salary, d => d.Salary)
                .MapToSrce(new PersonB { Salary = 0 });

            Assert.IsNotNull(r);
            Assert.AreEqual(0, r.Salary);
        }

        [Test]
        public void MapToSrce_NullableValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Salary, d => d.Salary)
                .MapToSrce(new PersonB { Salary = 10m });

            Assert.IsNotNull(r);
            Assert.AreEqual(10m, r.Salary);
        }

        [Test]
        public void MapToSrce_IntValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Age, d => d.Age)
                .MapToSrce(new PersonB { Age = 10 });

            Assert.IsNotNull(r);
            Assert.AreEqual(10, r.Age);
        }

        [Test]
        public void MapToSrce_ArrayNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Codes, d => d.Codes)
                .MapToSrce(new PersonB { Codes = null });

            Assert.IsNotNull(r);
            Assert.IsNull(r.Codes);
        }

        [Test]
        public void MapToSrce_ArrayEmpty()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Codes, d => d.Codes)
                .MapToSrce(new PersonB { Codes = new int[0] { } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Codes);
            Assert.AreEqual(0, r.Codes.Length);
        }

        [Test]
        public void MapToSrce_ArrayValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Codes, d => d.Codes)
                .MapToSrce(new PersonB { Codes = new int[] { 1, 2 } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Codes);
            Assert.AreEqual(2, r.Codes.Length);
            Assert.AreEqual(1, r.Codes[0]);
            Assert.AreEqual(2, r.Codes[1]);
        }

        [Test]
        public void MapToSrce_EntityNull_AutoPropMapper()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX)
                .MapToSrce(new PersonB());

            Assert.IsNotNull(r);
            Assert.IsNull(r.Address);
        }

        [Test]
        public void MapToSrce_EntityValue_AutoPropMapper()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX)
                .MapToSrce(new PersonB { AddressX = new AddressX { Street = "AAA", City = "BBB" } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Address);
            Assert.AreEqual("AAA", r.Address.Street);
            Assert.AreEqual("BBB", r.Address.City);
        }

        [Test]
        public void MapToSrce_EntityValue_PropMapper()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Address, d => d.AddressX, p => p.SetMapper(EntityMapper.Create<Address, AddressX>().HasProperty(sa => sa.Street, da => da.City)))
                .MapToSrce(new PersonB { AddressX = new AddressX { Street = "AAA", City = "BBB" } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Address);
            Assert.IsNull(r.Address.City);
            Assert.AreEqual("BBB", r.Address.Street);
        }

        [Test]
        public void MapToSrce_CollNull()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Addresses, d => d.Addresses)
                .MapToSrce(new PersonB());

            Assert.IsNotNull(r);
            Assert.IsNull(r.Addresses);
        }

        [Test]
        public void MapToSrce_CollEmpty()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Addresses, d => d.Addresses)
                .MapToSrce(new PersonB { Addresses = new List<AddressX>() });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Addresses);
            Assert.AreEqual(0, r.Addresses.Length);
        }

        [Test]
        public void MapToSrce_CollValue()
        {
            var r = EntityMapper.Create<PersonA, PersonB>()
                .HasProperty(s => s.Addresses, d => d.Addresses)
                .MapToSrce(new PersonB { Addresses = new List<AddressX> { new AddressX { Street = "AAA", City = "BBB" }, new AddressX { Street = "YYY", City = "ZZZ" }, } });

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Addresses);
            Assert.AreEqual(2, r.Addresses.Length);
        }

        #endregion

        #region Classes

        public class PersonA
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public decimal? Salary { get; set; }

            public int[] Codes { get; set; }

            public string Street { get; set; }

            public string City { get; set; }

            public Address Address { get; set; }

            public Address[] Addresses { get; set; }
        }

        public class PersonB
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public decimal Salary { get; set; }

            public int[] Codes { get; set; }

            public string StreetX { get; set; }

            public string CityX { get; set; }

            public AddressX AddressX { get; set; }

            public List<AddressX> Addresses { get; set; }
        }

        public class Address
        {
            public string Street { get; set; }

            public string City { get; set; }
        }

        public class AddressX
        {
            public string Street { get; set; }

            public string City { get; set; }
        }

        #endregion
    }
}
