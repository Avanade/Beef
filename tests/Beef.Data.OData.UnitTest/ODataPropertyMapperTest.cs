// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.OData.UnitTest.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Test.NUnit;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Beef.Data.OData.UnitTest
{
    [TestFixture]
    public class ODataPropertyMapperTest
    {
        [Test]
        public void Ctor_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ODataPropertyMapper<Person, int>(null));
        }

        [Test]
        public void Ctor_Int32()
        {
            AssertDefault<int>(new ODataPropertyMapper<Person, int>(x => x.Id), typeof(int), "Id", "Id", Mapper.OperationTypes.Any, true);
            AssertDefault<int>(new ODataPropertyMapper<Person, int>(x => x.Id, "IdX"), typeof(int), "Id", "IdX", Mapper.OperationTypes.Any, true);
            AssertDefault<int>(new ODataPropertyMapper<Person, int>(x => x.Id, "IdX", Mapper.OperationTypes.AnyExceptCreate), typeof(int), "Id", "IdX", Mapper.OperationTypes.AnyExceptCreate, true);
        }

        [Test]
        public void Ctor_String()
        {
            AssertDefault<string>(new ODataPropertyMapper<Person, string>(x => x.Name), typeof(string), "Name", "Name", Mapper.OperationTypes.Any, true);
            AssertDefault<string>(new ODataPropertyMapper<Person, string>(x => x.Name, "NameX"), typeof(string), "Name", "NameX", Mapper.OperationTypes.Any, true);
            AssertDefault<string>(new ODataPropertyMapper<Person, string>(x => x.Name, "NameX", Mapper.OperationTypes.AnyExceptCreate), typeof(string), "Name", "NameX", Mapper.OperationTypes.AnyExceptCreate, true);
        }

        [Test]
        public void Ctor_Decimal()
        {
            AssertDefault<decimal>(new ODataPropertyMapper<Person, decimal>(x => x.Salary), typeof(decimal), "Salary", "Salary", Mapper.OperationTypes.Any, true);
            AssertDefault<decimal>(new ODataPropertyMapper<Person, decimal>(x => x.Salary, "SalaryX"), typeof(decimal), "Salary", "SalaryX", Mapper.OperationTypes.Any, true);
            AssertDefault<decimal>(new ODataPropertyMapper<Person, decimal>(x => x.Salary, "SalaryX", Mapper.OperationTypes.AnyExceptCreate), typeof(decimal), "Salary", "SalaryX", Mapper.OperationTypes.AnyExceptCreate, true);
        }

        [Test]
        public void Ctor_StringArray()
        {
            AssertDefault<string[]>(new ODataPropertyMapper<Person, string[]>(x => x.Nicknames), typeof(string[]), "Nicknames", "Nicknames", Mapper.OperationTypes.Any, false);
            AssertDefault<string[]>(new ODataPropertyMapper<Person, string[]>(x => x.Nicknames, "NicknamesX"), typeof(string[]), "Nicknames", "NicknamesX", Mapper.OperationTypes.Any, false);

            var pm = new ODataPropertyMapper<Person, string[]>(x => x.Nicknames, "NicknamesX", Mapper.OperationTypes.AnyExceptCreate);
            AssertDefault<string[]>(pm, typeof(string[]), "Nicknames", "NicknamesX", Mapper.OperationTypes.AnyExceptCreate, false);

            Assert.IsTrue(pm.IsSrceComplexType);
            Assert.IsNotNull(pm.SrceComplexTypeReflector);
            Assert.IsTrue(pm.SrceComplexTypeReflector.IsCollection);
            Assert.AreEqual(Reflection.ComplexTypeCode.Array, pm.SrceComplexTypeReflector.ComplexTypeCode);
            Assert.AreEqual(typeof(string), pm.SrceComplexTypeReflector.ItemType);
        }

        [Test]
        public void Ctor_ComplexType()
        {
            AssertDefault<Address>(new ODataPropertyMapper<Person, Address>(x => x.Address), typeof(Address), "Address", "Address", Mapper.OperationTypes.Any, false);
            AssertDefault<Address>(new ODataPropertyMapper<Person, Address>(x => x.Address, "AddressX"), typeof(Address), "Address", "AddressX", Mapper.OperationTypes.Any, false);

            var pm = new ODataPropertyMapper<Person, Address>(x => x.Address, "AddressX", Mapper.OperationTypes.AnyExceptCreate);
            AssertDefault<Address>(pm, typeof(Address), "Address", "AddressX", Mapper.OperationTypes.AnyExceptCreate, false);

            Assert.IsTrue(pm.IsSrceComplexType);
            Assert.IsNotNull(pm.SrceComplexTypeReflector);
            Assert.IsFalse(pm.SrceComplexTypeReflector.IsCollection);
            Assert.AreEqual(Reflection.ComplexTypeCode.Object, pm.SrceComplexTypeReflector.ComplexTypeCode);
            Assert.AreEqual(typeof(Address), pm.SrceComplexTypeReflector.ItemType);
        }

        [Test]
        public void Ctor_ComplexTypeList()
        {
            AssertDefault<List<Address>>(new ODataPropertyMapper<Person, List<Address>>(x => x.Addresses), typeof(List<Address>), "Addresses", "Addresses", Mapper.OperationTypes.Any, false);
            AssertDefault<List<Address>>(new ODataPropertyMapper<Person, List<Address>>(x => x.Addresses, "AddressesX"), typeof(List<Address>), "Addresses", "AddressesX", Mapper.OperationTypes.Any, false);

            var pm = new ODataPropertyMapper<Person, List<Address>>(x => x.Addresses, "AddressesX", Mapper.OperationTypes.AnyExceptCreate);
            AssertDefault<List<Address>>(pm, typeof(List<Address>), "Addresses", "AddressesX", Mapper.OperationTypes.AnyExceptCreate, false);

            Assert.IsTrue(pm.IsSrceComplexType);
            Assert.IsNotNull(pm.SrceComplexTypeReflector);
            Assert.IsTrue(pm.SrceComplexTypeReflector.IsCollection);
            Assert.AreEqual(Reflection.ComplexTypeCode.ICollection, pm.SrceComplexTypeReflector.ComplexTypeCode);
            Assert.AreEqual(typeof(Address), pm.SrceComplexTypeReflector.ItemType);
        }

        private void AssertDefault<T>(ODataPropertyMapper<Person, T> pm, Type type, string srceName, string destName, Mapper.OperationTypes operationTypes, bool isIntrinsic)
        {
            Assert.AreEqual(type, pm.SrcePropertyType);
            Assert.AreEqual(srceName, pm.SrcePropertyName);
            Assert.AreEqual(destName, pm.DestPropertyName);

            Assert.IsFalse(pm.IsUniqueKey);
            Assert.IsTrue(pm.IsUniqueKeyAutoGeneratedOnCreate);
            Assert.IsNull(pm.Converter);
            Assert.IsNull(pm.Mapper);
            Assert.AreEqual(operationTypes, pm.OperationTypes);

            if (isIntrinsic)
            {
                Assert.IsFalse(pm.IsSrceComplexType);
                Assert.IsNull(pm.SrceComplexTypeReflector);
            }
        }

        [Test]
        public void SetUniqueKey_Valid()
        {
            var pm = new ODataPropertyMapper<Person, string>(x => x.Name);
            Assert.IsFalse(pm.IsUniqueKey);
            Assert.IsTrue(pm.IsUniqueKeyAutoGeneratedOnCreate);

            pm.SetUniqueKey();
            Assert.IsTrue(pm.IsUniqueKey);
            Assert.IsTrue(pm.IsUniqueKeyAutoGeneratedOnCreate);

            pm.SetUniqueKey(false);
            Assert.IsTrue(pm.IsUniqueKey);
            Assert.IsFalse(pm.IsUniqueKeyAutoGeneratedOnCreate);

            var pm2 = new ODataPropertyMapper<Person, Address>(x => x.Address);
            Assert.IsFalse(pm2.IsUniqueKey);
            Assert.IsTrue(pm2.IsUniqueKeyAutoGeneratedOnCreate);

            ExpectException.Throws<InvalidOperationException>("A Unique Key with AutoGeneratedOnCreate cannot be set for a Property where IsSrceComplexType is true.",
                () => pm2.SetUniqueKey());
        }

        [Test]
        public void SetMapper_ViaInterface()
        {
            var pm = (IODataPropertyMapper)new ODataPropertyMapper<Person, Address>(x => x.Address);
            Assert.IsNull(pm.Mapper);

            pm.SetMapper(new ODataMapper<Address>("Address"));
            Assert.IsNotNull(pm.Mapper);

            ExpectException.Throws<MapperException>("The PropertyMapper SrceType 'Address' has an ItemType of 'Address' which must be the same as the underlying EntityMapper SrceType 'Person'.",
                () => pm.SetMapper(new EntityMapper<Person, Address>()));
        }

        [Test]
        public void SetMapper()
        {
            var pm = new ODataPropertyMapper<Person, Address>(x => x.Address);
            Assert.IsNull(pm.Mapper);

            pm.SetMapper(new ODataMapper<Address>("Address"));
            Assert.IsNotNull(pm.Mapper);

            ExpectException.Throws<ArgumentException>("Mapper must be instance of IODataMapper.", () => pm.SetMapper(new EntityMapper<Person, Address>()));

            ExpectException.Throws<MapperException>("The PropertyMapper SrceType 'Address' has an ItemType of 'Address' which must be the same as the underlying EntityMapper SrceType 'Person'.",
                () => pm.SetMapper(new ODataMapper<Person>("Person")));

            var pm2 = new ODataPropertyMapper<Person, int>(x => x.Id);
            ExpectException.Throws<MapperException>("The PropertyMapper SrceType 'Int32' must be a complex type to set a Mapper.",
                () => pm2.SetMapper(new ODataMapper<Person>("Person")));
        }

        [Test]
        public void SetConverter_ViaInterface()
        {
            var pm = (IODataPropertyMapper)new ODataPropertyMapper<Person, Address>(x => x.Address);
            Assert.IsNull(pm.Converter);

            ExpectException.Throws<MapperException>("The PropertyMapper SrceType 'Address' and Converter SrceType 'Boolean' must match.",
                () => pm.SetConverter(BooleanToYesNoConverter.Default));
        }

        [Test]
        public void SetConverter()
        {
            var pm = new ODataPropertyMapper<Person, bool>(x => x.IsDeceased);
            Assert.IsNull(pm.Converter);

            pm.SetConverter(BooleanToYesNoConverter.Default);
            Assert.IsNotNull(pm.Converter);

            pm.SetConverter<string>(null);
            Assert.IsNull(pm.Converter);
        }

        public class RandomTestConverter : PropertyMapperConverterBase<RandomTestConverter, Address, string>
        {
            public RandomTestConverter() : base((data) => null) { }
        }

        [Test]
        public void Set_Mapper_Converter_MutuallyExclusive()
        {
            var pm = new ODataPropertyMapper<Person, bool>(x => x.IsDeceased);
            pm.SetConverter(BooleanToYesNoConverter.Default);
            Assert.IsNotNull(pm.Converter);

            ExpectException.Throws<MapperException>("The Mapper and Converter cannot be both set; only one is permissible.", () => pm.SetMapper(new ODataMapper<Address>("Address")));

            var pm2 = new ODataPropertyMapper<Person, Address>(x => x.Address);
            pm2.SetMapper(new ODataMapper<Address>("Address"));
            Assert.IsNotNull(pm2.Mapper);

            ExpectException.Throws<MapperException>("The Mapper and Converter cannot be both set; only one is permissible.", () => pm2.SetConverter(RandomTestConverter.Default));
        }

        [Test]
        public void SetSrceValue_Intrinsic()
        {
            var p = new Person();
            var pm = new ODataPropertyMapper<Person, int>(x => x.Id);
            Assert.Throws<ArgumentNullException>(() => pm.SetSrceValue(p, null, OperationTypes.Get));

            var json = JObject.Parse(Person.JsonData);

            pm.SetSrceValue(p, json["IdX"], OperationTypes.Get);
            Assert.AreEqual(1, p.Id);
        }

        [Test]
        public void SetSrceValue_Converter()
        {
            var p = new Person();
            var pm = new ODataPropertyMapper<Person, bool>(x => x.IsDeceased);
            pm.SetConverter(BooleanToYesNoConverter.Default);

            var json = JObject.Parse(Person.JsonData);

            pm.SetSrceValue(p, json["IsDeceasedX"], OperationTypes.Get);
            Assert.AreEqual(true, p.IsDeceased);
        }

        [Test]
        public void SetSrceValue_MapperObject()
        {
            var p = new Person();
            var pm = new ODataPropertyMapper<Person, Address>(x => x.Address);
            pm.SetMapper(new AddressMapper());

            var json = JObject.Parse(Person.JsonData);

            pm.SetSrceValue(p, json["AddressX"], OperationTypes.Get);
            Assert.IsNotNull(p.Address);

            Assert.AreEqual(123, p.Address.StreetNumber);
            Assert.AreEqual("Petherick", p.Address.StreetName);
        }

        [Test]
        public void SetSrceValue_MapperCollection()
        {
            var p = new Person();
            var pm = new ODataPropertyMapper<Person, List<Address>>(x => x.Addresses);
            pm.SetMapper(new AddressMapper());

            var json = JObject.Parse(Person.JsonData);
            pm.SetSrceValue(p, json["AddressesX"], OperationTypes.Get);
            Assert.IsNotNull(p.Addresses);
            Assert.AreEqual(2, p.Addresses.Count);

            Assert.AreEqual(456, p.Addresses[0].StreetNumber);
            Assert.AreEqual("Simpsons", p.Addresses[0].StreetName);
            Assert.AreEqual(789, p.Addresses[1].StreetNumber);
            Assert.AreEqual("Park", p.Addresses[1].StreetName);

            var pm2 = new ODataPropertyMapper<Person, string[]>(x => x.Nicknames);
            pm2.SetSrceValue(p, json["NicknamesX"], OperationTypes.Get);
            Assert.IsNotNull(p.Nicknames);
            Assert.AreEqual(2, p.Nicknames.Length);

            Assert.AreEqual("Bro", p.Nicknames[0]);
            Assert.AreEqual("Mate", p.Nicknames[1]);
        }
    }
}
