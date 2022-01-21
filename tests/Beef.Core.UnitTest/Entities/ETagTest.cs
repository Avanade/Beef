using Beef.Entities;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.Entities
{
    [TestFixture]
    public class ETagTest
    {
        public class Person
        {
            public string First { get; set; }
            public string Last { get; set; }
        }


        [Test]
        public void ConvertToString()
        {
            Assert.AreEqual(("X", false), ETagGenerator.ConvertToString("X"));
            Assert.AreEqual(("1", false), ETagGenerator.ConvertToString(1));
            Assert.AreEqual(("1.51", false), ETagGenerator.ConvertToString(1.51m));
            Assert.AreEqual(("1990-01-31T08:09:10.0000000", false), ETagGenerator.ConvertToString(new DateTime(1990, 01, 31, 08, 09, 10)));

            var g = Guid.NewGuid();
            Assert.AreEqual((g.ToString(), false), ETagGenerator.ConvertToString(g));

            var p = new Person { First = "Jane", Last = "Doe" };
            Assert.AreEqual(("{\"First\":\"Jane\",\"Last\":\"Doe\"}", true), ETagGenerator.ConvertToString(p));
        }

        [Test]
        public void Generate()
        {
            var p = new Person { First = "Jane", Last = "Doe" };
            Assert.AreEqual("\"2I0QhKAlJMdHjCAocBhQeuzmze73jSnnWqyOkofhRn4=\"", ETagGenerator.Generate(p));
            Assert.AreEqual("\"471H/SShWDDr5wOVpMnhcu00Fw3nhOHN1aE8VNpXCsY=\"", ETagGenerator.Generate(p, "XYZ"));
        }
    }
}