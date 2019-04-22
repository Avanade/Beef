// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Json;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Beef.Core.UnitTest.Json
{
    [TestFixture]
    public class JsonPropertyFilterTest
    {
        [Test]
        public void Apply_String()
        {
            var jt = JsonPropertyFilter.Apply("Test string", null, null);
            Assert.IsNotNull(jt);
            Assert.AreEqual(JTokenType.String, jt.Type);
            Assert.AreEqual("Test string", jt.Value<string>());
            Assert.AreEqual("Test string", jt.ToString());
        }

        [Test]
        public void Apply_Int32()
        {
            var jt = JsonPropertyFilter.Apply(123, null, null);
            Assert.IsNotNull(jt);
            Assert.AreEqual(JTokenType.Integer, jt.Type);
            Assert.AreEqual(123, jt.Value<int>());
            Assert.AreEqual("123", jt.ToString());
        }

        [Test]
        public void Apply_JObjectInclude()
        {
            var p = Person.Create();

            var jo = JsonPropertyFilter.Apply(p, new string[] { "first", "other", "nos" });
            Assert.IsNotNull(jo);
            AssertJObject("{ 'First': 'A', 'Other': 'C', 'Nos': [ 1, 2 ] }", jo);

            jo = JsonPropertyFilter.Apply(p, new string[] { "first", "address" });
            Assert.IsNotNull(jo);
            AssertJObject("{ 'First': 'A', 'Address': { 'Street': 'STX', 'Suburb': 'SUX' } }", jo);

            jo = JsonPropertyFilter.Apply(p, new string[] { "first", "address.street", "addresses.suburb" });
            Assert.IsNotNull(jo);
            AssertJObject("{ 'First': 'A', 'Address': { 'Street': 'STX' }, 'Addresses': [ { 'Suburb': 'SU1' }, { 'Suburb': 'SU2' } ] }", jo);
        }

        [Test]
        public void Apply_JObjectExclude()
        {
            var p = Person.Create();

            var jo = JsonPropertyFilter.Apply(p, null, new string[] { "first", "other" });
            Assert.IsNotNull(jo);
            AssertJObject("{ 'Last': 'B', 'Address': { 'Street': 'STX', 'Suburb': 'SUX' }, 'Addresses': [ { 'Street': 'ST1', 'Suburb': 'SU1' }, { 'Street': 'ST2', 'Suburb': 'SU2' } ], 'Nos': [ 1, 2 ] }", jo);

            jo = JsonPropertyFilter.Apply(p, null, new string[] { "first", "other", "address" });
            Assert.IsNotNull(jo);
            AssertJObject("{ 'Last': 'B', 'Addresses': [ { 'Street': 'ST1', 'Suburb': 'SU1' }, { 'Street': 'ST2', 'Suburb': 'SU2' } ], 'Nos': [ 1, 2 ] }", jo);

            jo = JsonPropertyFilter.Apply(p, null, new string[] { "first", "other", "address.street", "addresses.suburb" });
            Assert.IsNotNull(jo);
            AssertJObject("{ 'Last': 'B', 'Address': { 'Suburb': 'SUX' }, 'Addresses': [ { 'Street': 'ST1' }, { 'Street': 'ST2' } ], 'Nos': [ 1, 2 ] }", jo);
        }

        [Test]
        public void Apply_JArrayInclude()
        {
            var a = new Address[] { new Address { Street = "ST1", Suburb = "SU1" }, new Address { Street = "ST2", Suburb = "SU2" } };
            var jo = JsonPropertyFilter.Apply(a, new string[] { "street" });
            Assert.IsNotNull(jo);
            AssertJArray("[ { 'Street': 'ST1' }, { 'Street': 'ST2' } ]", jo);
        }

        [Test]
        public void Apply_JArrayExclude()
        {
            var a = new Address[] { new Address { Street = "ST1", Suburb = "SU1" }, new Address { Street = "ST2", Suburb = "SU2" } };
            var jo = JsonPropertyFilter.Apply(a, null, new string[] { "street" });
            Assert.IsNotNull(jo);
            AssertJArray("[ { 'Suburb': 'SU1' }, { 'Suburb': 'SU2' } ]", jo);
        }

        private void AssertJObject(string exp, JToken act)
        {
            Assert.AreEqual(JObject.Parse(exp.Replace('\'', '\"')).ToString(), act.ToString());
        }

        private void AssertJArray(string exp, JToken act)
        {
            Assert.AreEqual(JArray.Parse(exp.Replace('\'', '\"')).ToString(), act.ToString());
        }

        public class Person
        {
            public string First { get; set; }
            public string Last { get; set; }
            public string Other { get; set; }
            public Address Address { get; set; }
            public Address[] Addresses { get; set; }
            public int[] Nos { get; set; }

            public static Person Create() => new Person { First = "A", Last = "B", Other = "C", Address = new Address { Street = "STX", Suburb = "SUX" }, Addresses = new Address[] { new Address { Street = "ST1", Suburb = "SU1" }, new Address { Street = "ST2", Suburb = "SU2" } }, Nos = new int[] { 1, 2 } };
        }

        public class Address
        {
            public string Street { get; set; }
            public string Suburb { get; set; }
        }
    }
}
