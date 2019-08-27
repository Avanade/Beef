using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.Cosmos.UnitTest
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class CosmosDbQueryTest
    {
        private CosmosDb _db;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _db = new CosmosDb();
            await _db.SetUp();
        }

        [Test]
        public void AsQueryable_NoPaging1()
        {
            var v = _db.Persons1.AsQueryable().ToArray();
            Assert.AreEqual(5, v.Length);

            v = _db.Persons1.AsQueryable().Where(x => x.Name == "Greg").ToArray();
            Assert.AreEqual(1, v.Length);
            Assert.AreEqual("Greg", v[0].Name);

            v = _db.Persons1.AsQueryable().Where(x => x.Name == "GREG").ToArray();
            Assert.AreEqual(0, v.Length);
        }

        [Test]
        public void AsQueryable_Paging1()
        {
            var v = _db.Persons1.AsQueryable().Paging(1, 2).ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Name);
            Assert.AreEqual("Greg", v[1].Name);

            v = _db.Persons1.AsQueryable().OrderBy(x => x.Name).Paging(1, 2).ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Greg", v[0].Name);
            Assert.AreEqual("Mike", v[1].Name);
        }

        [Test]
        public void AsQueryable_Wildcards1()
        {
            var v = _db.Persons1.AsQueryable().WhereWildcard(x => x.Name, "g*").ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Name);
            Assert.AreEqual("Greg", v[1].Name);

            v = _db.Persons1.AsQueryable().WhereWildcard(x => x.Name, "*Y").ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Name);
            Assert.AreEqual("Sally", v[1].Name);

            v = _db.Persons1.AsQueryable().WhereWildcard(x => x.Name, "*e*").ToArray();
            Assert.AreEqual(3, v.Length);
            Assert.AreEqual("Rebecca", v[0].Name);
            Assert.AreEqual("Greg", v[1].Name);
            Assert.AreEqual("Mike", v[2].Name);

            ExpectException.Throws<InvalidOperationException>("Wildcard selection text is not supported.",
                () => _db.Persons1.AsQueryable().WhereWildcard(x => x.Name, "*m*e").ToArray());
        }

        [Test]
        public void AsQueryable_NoPaging2()
        {
            var v = _db.Persons2.AsQueryable().ToArray();
            Assert.AreEqual(5, v.Length);

            v = _db.Persons2.AsQueryable().Where(x => x.Name == "Greg").ToArray();
            Assert.AreEqual(1, v.Length);
            Assert.AreEqual("Greg", v[0].Name);

            v = _db.Persons2.AsQueryable().Where(x => x.Name == "GREG").ToArray();
            Assert.AreEqual(0, v.Length);
        }

        [Test]
        public void AsQueryable_Paging2()
        {
            var v = _db.Persons2.AsQueryable().Paging(1, 2).ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Name);
            Assert.AreEqual("Greg", v[1].Name);

            v = _db.Persons2.AsQueryable().OrderBy(x => x.Name).Paging(1, 2).ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Greg", v[0].Name);
            Assert.AreEqual("Mike", v[1].Name);
        }

        [Test]
        public void AsQueryable_Wildcards2()
        {
            var v = _db.Persons2.AsQueryable().WhereWildcard(x => x.Name, "g*").ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Name);
            Assert.AreEqual("Greg", v[1].Name);

            v = _db.Persons2.AsQueryable().WhereWildcard(x => x.Name, "*Y").ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Name);
            Assert.AreEqual("Sally", v[1].Name);

            v = _db.Persons2.AsQueryable().WhereWildcard(x => x.Name, "*e*").ToArray();
            Assert.AreEqual(3, v.Length);
            Assert.AreEqual("Rebecca", v[0].Name);
            Assert.AreEqual("Greg", v[1].Name);
            Assert.AreEqual("Mike", v[2].Name);

            ExpectException.Throws<InvalidOperationException>("Wildcard selection text is not supported.",
                () => _db.Persons2.AsQueryable().WhereWildcard(x => x.Name, "*m*e").ToArray());
        }

        [Test]
        public void AsQueryable_NoPaging3()
        {
            var v = _db.Persons3.AsQueryable().ToArray();
            Assert.AreEqual(5, v.Length);

            v = _db.Persons3.AsQueryable().Where(x => x.Value.Name == "Greg").ToArray();
            Assert.AreEqual(1, v.Length);
            Assert.AreEqual("Greg", v[0].Value.Name);

            v = _db.Persons3.AsQueryable().Where(x => x.Value.Name == "GREG").ToArray();
            Assert.AreEqual(0, v.Length);
        }

        [Test]
        public void AsQueryable_Paging3()
        {
            var v = _db.Persons3.AsQueryable().Paging(1, 2).ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Value.Name);
            Assert.AreEqual("Greg", v[1].Value.Name);

            v = _db.Persons3.AsQueryable().OrderBy(x => x.Value.Name).Paging(1, 2).ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Greg", v[0].Value.Name);
            Assert.AreEqual("Mike", v[1].Value.Name);
        }

        [Test]
        public void AsQueryable_Wildcards3()
        {
            var v = _db.Persons3.AsQueryable().WhereWildcard(x => x.Value.Name, "g*").ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Value.Name);
            Assert.AreEqual("Greg", v[1].Value.Name);

            v = _db.Persons3.AsQueryable().WhereWildcard(x => x.Value.Name, "*Y").ToArray();
            Assert.AreEqual(2, v.Length);
            Assert.AreEqual("Gary", v[0].Value.Name);
            Assert.AreEqual("Sally", v[1].Value.Name);

            v = _db.Persons3.AsQueryable().WhereWildcard(x => x.Value.Name, "*e*").ToArray();
            Assert.AreEqual(3, v.Length);
            Assert.AreEqual("Rebecca", v[0].Value.Name);
            Assert.AreEqual("Greg", v[1].Value.Name);
            Assert.AreEqual("Mike", v[2].Value.Name);

            ExpectException.Throws<InvalidOperationException>("Wildcard selection text is not supported.",
                () => _db.Persons3.AsQueryable().WhereWildcard(x => x.Value.Name, "*m*e").ToArray());
        }
    }
}