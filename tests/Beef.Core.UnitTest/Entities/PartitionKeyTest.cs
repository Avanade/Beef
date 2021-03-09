using Beef.Entities;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.Entities
{
    [TestFixture]
    public class PartitionKeyTest
    {
        public class Person
        {
            public string First { get; set; }
            public string Last { get; set; }
        }


        [Test]
        public void Generate()
        {
            Assert.AreEqual("X", PartitionKeyGenerator.Generate("X"));
            Assert.AreEqual("1", PartitionKeyGenerator.Generate(1));
            Assert.AreEqual("1.51", PartitionKeyGenerator.Generate(1.51m));
            Assert.AreEqual("1990-01-31T08:09:10.0000000", PartitionKeyGenerator.Generate(new DateTime(1990, 01, 31, 08, 09, 10)));

            var g = Guid.NewGuid();
            Assert.AreEqual(g.ToString(), PartitionKeyGenerator.Generate(g));

            var p = new Person { First = "Jane", Last = "Doe" };
            Assert.AreEqual("XCmFjH2zioLOelNdmU8hEQ==", PartitionKeyGenerator.Generate(p));
            Assert.AreEqual("E9ik+Agqes6hzHMezoJigw==", PartitionKeyGenerator.Generate(p, "XYZ"));
        }
    }
}