// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper.Converters;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Beef.Core.UnitTest.Mapper.Converters
{
    [TestFixture]
    public class JObjectToJsonConverterTest
    {
        private class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [Test]
        public void IPropertyMapperConverter()
        {
            var c = (IPropertyMapperConverter)new JObjectToJsonConverter();
            Assert.AreEqual(typeof(JObject), c.SrceType);
            Assert.AreEqual(typeof(JObject), c.SrceUnderlyingType);
            Assert.AreEqual(typeof(string), c.DestType);
            Assert.AreEqual(typeof(string), c.DestUnderlyingType);
        }

        [Test]
        public void ConvertToDest()
        {
            var c = new JObjectToJsonConverter();
            var d = c.ConvertToDest(null);
            Assert.IsNull(d);

            d = c.ConvertToDest(JObject.FromObject(new Person { FirstName = "Jen", LastName = "Browne" }));
            Assert.AreEqual("{\"FirstName\":\"Jen\",\"LastName\":\"Browne\"}", d);
        }

        [Test]
        public void ConvertToSrce()
        {
            var c = new JObjectToJsonConverter();
            var s = c.ConvertToSrce(null);
            Assert.IsNull(s);

            s = c.ConvertToSrce("{\"FirstName\":\"Jen\",\"LastName\":\"Browne\"}");
            Assert.IsNotNull(s);

            var p = s.ToObject<Person>();
            Assert.IsNotNull(p);
            Assert.AreEqual("Jen", p.FirstName);
            Assert.AreEqual("Browne", p.LastName);
        }
    }
}
