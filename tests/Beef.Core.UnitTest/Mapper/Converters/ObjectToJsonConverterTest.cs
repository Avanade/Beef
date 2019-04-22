// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper.Converters;
using NUnit.Framework;

namespace Beef.Core.UnitTest.Mapper.Converters
{
    [TestFixture]
    public class ObjectToJsonConverterTest
    {
        private class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [Test]
        public void IPropertyMapperConverter()
        {
            var c = (IPropertyMapperConverter)new ObjectToJsonConverter<Person>();
            Assert.AreEqual(typeof(Person), c.SrceType);
            Assert.AreEqual(typeof(Person), c.SrceUnderlyingType);
            Assert.AreEqual(typeof(string), c.DestType);
            Assert.AreEqual(typeof(string), c.DestUnderlyingType);

            c = (IPropertyMapperConverter)new ObjectToJsonConverter<int?>();
            Assert.AreEqual(typeof(int?), c.SrceType);
            Assert.AreEqual(typeof(int), c.SrceUnderlyingType);
            Assert.AreEqual(typeof(string), c.DestType);
            Assert.AreEqual(typeof(string), c.DestUnderlyingType);
        }

        [Test]
        public void ConvertToDest()
        {
            var c = new ObjectToJsonConverter<Person>();
            var d = c.ConvertToDest(null);
            Assert.IsNull(d);

            d = c.ConvertToDest(new Person { FirstName = "Jen", LastName = "Browne" });
            Assert.AreEqual("{\"FirstName\":\"Jen\",\"LastName\":\"Browne\"}", d);
        }

        [Test]
        public void ConvertToSrce()
        {
            var c = new ObjectToJsonConverter<Person>();
            var s = c.ConvertToSrce(null);
            Assert.IsNull(s);

            s = c.ConvertToSrce("{\"FirstName\":\"Jen\",\"LastName\":\"Browne\"}");
            Assert.IsNotNull(s);
            Assert.AreEqual("Jen", s.FirstName);
            Assert.AreEqual("Browne", s.LastName);
        }
    }
}
