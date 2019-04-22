// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Beef.Test.NUnit;
using Beef.Data.OData.UnitTest.Entities;

namespace Beef.Data.OData.UnitTest
{
    [TestFixture]
    public class ODataMapperTest
    {
        [Test]
        public void Ctor_EntityIsString()
        {
            ExpectException.Throws<InvalidOperationException>("SrceType must not be a String.", () => new ODataMapper<string>("Abc"));
        }

        [Test]
        public void Ctor_EntityIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ODataMapper<Person>(null));
        }

        [Test]
        public void Ctor_EntityIsEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new ODataMapper<Person>(""));
        }

        [Test]
        public void Ctor_Valid()
        {
            var om = new ODataMapper<Person>("X");
            Assert.AreEqual("X", om.ODataEntityName);
            Assert.AreEqual(typeof(Person), om.SrceType);
            Assert.IsNull(om.GetKeyUrl(new Person()));
            Assert.IsNull(om.GetKeyUrl(new IComparable[0]));
            Assert.AreEqual(0, om.GetODataFieldNames()?.Length);
            Assert.IsNull(om.GetODataFieldNamesQuery());

            var val = om.MapFromOData(JObject.Parse(@"{""Name"":""John""}"), Mapper.OperationTypes.Unspecified);
            Assert.IsNotNull(val);
            Assert.IsNull(val.Name);

            val.Name = "John";
            var json = om.MapToOData(val, Mapper.OperationTypes.Unspecified);
            Assert.IsNotNull(json);
            Assert.IsInstanceOf(typeof(JObject), json);
            Assert.AreEqual("{}", json.ToString());

            var iom = (IODataMapper)om;
            Assert.AreEqual("X", iom.ODataEntityName);
            Assert.AreEqual(typeof(Person), iom.SrceType);
            Assert.IsNull(iom.GetKeyUrl(new Person()));
            Assert.IsNull(iom.GetKeyUrl(new IComparable[0]));

            var obj = iom.MapFromOData(JObject.Parse(@"{""Name"":""John""}"), Mapper.OperationTypes.Unspecified);
            Assert.IsInstanceOf(typeof(Person), obj);
            val = (Person)obj;
            Assert.IsNotNull(val);
            Assert.IsNull(val.Name);

            val.Name = "John";
            json = iom.MapToOData(val, Mapper.OperationTypes.Unspecified);
            Assert.IsNotNull(json);
            Assert.IsInstanceOf(typeof(JObject), json);
            Assert.AreEqual("{}", json.ToString());
        }

        [Test]
        public void MapFromOData_DefaultCtor()
        {
            var pm = new PersonMapper();
            var p = pm.MapFromOData(JObject.Parse(Person.JsonData), Mapper.OperationTypes.Get);

            Assert.IsNotNull(p);
            Assert.AreEqual(1, p.Id);
            Assert.AreEqual("Angela", p.Name);
            Assert.AreEqual(new DateTime(1970, 02, 05), p.Birthday);
            Assert.AreEqual(100000m, p.Salary);

            Assert.IsNotNull(p.Address);
            Assert.AreEqual(123, p.Address.StreetNumber);
            Assert.AreEqual("Petherick", p.Address.StreetName);

            Assert.IsNotNull(p.Addresses);
            Assert.AreEqual(2, p.Addresses.Count);
            Assert.AreEqual(456, p.Addresses[0].StreetNumber);
            Assert.AreEqual("Simpsons", p.Addresses[0].StreetName);
            Assert.AreEqual(789, p.Addresses[1].StreetNumber);
            Assert.AreEqual("Park", p.Addresses[1].StreetName);

            Assert.IsNotNull(p.Nicknames);
            Assert.AreEqual(2, p.Nicknames.Length);
            Assert.AreEqual("Bro", p.Nicknames[0]);
            Assert.AreEqual("Mate", p.Nicknames[1]);

            Assert.IsTrue(p.IsDeceased);
        }

        [Test]
        public void MapFromOData_PropertyCtor()
        {
            var pm = new Person2Mapper();
            var p = pm.MapFromOData(JObject.Parse(Person.JsonData), Mapper.OperationTypes.Get);

            Assert.IsNotNull(p);
            Assert.AreEqual(1, p.Id);
            Assert.AreEqual("Angela", p.Name);
            Assert.AreEqual(new DateTime(1970, 02, 05), p.Birthday);
            Assert.AreEqual(100000m, p.Salary);

            Assert.IsNotNull(p.Address);
            Assert.AreEqual(123, p.Address.StreetNumber);
            Assert.AreEqual("Petherick", p.Address.StreetName);

            Assert.IsNotNull(p.Addresses);
            Assert.AreEqual(2, p.Addresses.Count);
            Assert.AreEqual(456, p.Addresses[0].StreetNumber);
            Assert.AreEqual("Simpsons", p.Addresses[0].StreetName);
            Assert.AreEqual(789, p.Addresses[1].StreetNumber);
            Assert.AreEqual("Park", p.Addresses[1].StreetName);

            Assert.IsNotNull(p.Nicknames);
            Assert.AreEqual(2, p.Nicknames.Length);
            Assert.AreEqual("Bro", p.Nicknames[0]);
            Assert.AreEqual("Mate", p.Nicknames[1]);

            Assert.IsTrue(p.IsDeceased);
        }


        [Test]
        public void MapToOData()
        {
            var pm = new PersonMapper();
            var p = Person.Create();

            var json = pm.MapToOData(p, Mapper.OperationTypes.Update);
            Assert.IsNotNull(json);
            Assert.AreEqual(Person.JsonData, System.Text.RegularExpressions.Regex.Replace(json.ToString(), @"\s+", " "));

            json = pm.MapToOData(p, Mapper.OperationTypes.Create);
            Assert.AreEqual(Person.JsonData.Replace("\"IdX\": 1, ", ""), System.Text.RegularExpressions.Regex.Replace(json.ToString(), @"\s+", " "));
        }
    }
}
