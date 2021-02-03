using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Data.Cosmos.UnitTest
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class CosmosDbContainer
    {
        private CosmosDb _db;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _db = new CosmosDb();
            await _db.SetUp();
        }

        [Test]
        public async Task Get1Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons1.GetAsync());
            ExpectException.Throws<NotSupportedException>("Only a single key value is currently supported.", () => _db.Persons1.GetAsync(1, 2));

            Assert.IsNull(await _db.Persons1.GetAsync(404.ToGuid()));

            var v = await _db.Persons1.GetAsync(1.ToGuid());
            Assert.IsNotNull(v);
            Assert.AreEqual("Rebecca", v.Name);
        }

        [Test]
        public async Task Get2Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons2.GetAsync());
            ExpectException.Throws<NotSupportedException>("Only a single key value is currently supported.", () => _db.Persons2.GetAsync(1, 2));

            Assert.IsNull(await _db.Persons2.GetAsync(404.ToGuid()));

            var v = await _db.Persons2.GetAsync(1.ToGuid());
            Assert.IsNotNull(v);
            Assert.AreEqual("Rebecca", v.Name);
        }

        [Test]
        public async Task Get3Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons3.GetAsync());
            ExpectException.Throws<NotSupportedException>("Only a single key value is currently supported.", () => _db.Persons3.GetAsync(1, 2));

            Assert.IsNull(await _db.Persons3.GetAsync(404.ToGuid()));

            var v = await _db.Persons3.GetAsync(1.ToGuid());
            Assert.IsNotNull(v);
            Assert.AreEqual(1.ToGuid(), v.Id);
            Assert.AreEqual("Rebecca", v.Name);

            // Different type.
            Assert.IsNull(await _db.Persons3.GetAsync(100.ToGuid()));
        }

        [Test]
        public async Task Create1Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons1.CreateAsync(null));

            var id = Guid.NewGuid();
            var v = new Person1 { Id = id, Name = "Michelle", Birthday = new DateTime(1979, 08, 12), Salary = 181000m };
            v = await _db.Persons1.CreateAsync(v);
            Assert.IsNotNull(v);
            Assert.AreEqual(id, v.Id);
            Assert.AreEqual("Michelle", v.Name);
            Assert.AreEqual(new DateTime(1979, 08, 12), v.Birthday);
            Assert.AreEqual(181000m, v.Salary);

            Assert.IsNotNull(await _db.Persons1.GetAsync(v.Id));
        }

        [Test]
        public async Task Create2Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons2.CreateAsync(null));

            var id = Guid.NewGuid();
            var v = new Person2 { Id = id, Name = "Michelle", Birthday = new DateTime(1979, 08, 12), Salary = 181000m };
            v = await _db.Persons2.CreateAsync(v);

            Assert.IsNotNull(v);
            Assert.AreEqual(id, v.Id);
            Assert.AreEqual("Michelle", v.Name);
            Assert.AreEqual(new DateTime(1979, 08, 12), v.Birthday);
            Assert.AreEqual(181000m, v.Salary);
            Assert.IsNotNull(v.ChangeLog);
            Assert.IsNotNull(v.ChangeLog.CreatedBy);
            Assert.IsNotNull(v.ChangeLog.CreatedDate);
            Assert.IsNull(v.ChangeLog.UpdatedBy);
            Assert.IsNull(v.ChangeLog.UpdatedDate);

            Assert.IsNotNull(await _db.Persons2.GetAsync(v.Id));
        }

        [Test]
        public async Task Create3Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons3.CreateAsync(null));

            var id = Guid.NewGuid();
            var v = new Person3 { Id = id, Name = "Michelle", Birthday = new DateTime(1979, 08, 12), Salary = 181000m };
            v = await _db.Persons3.CreateAsync(v);

            Assert.IsNotNull(v);
            Assert.AreEqual(id, v.Id);
            Assert.AreEqual("Michelle", v.Name);
            Assert.AreEqual(new DateTime(1979, 08, 12), v.Birthday);
            Assert.AreEqual(181000m, v.Salary);
            Assert.IsNotNull(v.ChangeLog);
            Assert.IsNotNull(v.ChangeLog.CreatedBy);
            Assert.IsNotNull(v.ChangeLog.CreatedDate);
            Assert.IsNull(v.ChangeLog.UpdatedBy);
            Assert.IsNull(v.ChangeLog.UpdatedDate);

            Assert.IsNotNull(await _db.Persons3.GetAsync(v.Id));
        }

        [Test]
        public async Task Update1Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons1.UpdateAsync(null));

            // Get previous.
            var v = await _db.Persons1.GetAsync(5.ToGuid());
            Assert.NotNull(v);

            // Update testing.
            v.Id = 404.ToGuid();
            ExpectException.Throws<NotFoundException>("*", () => _db.Persons1.UpdateAsync(v));

            v.Id = 5.ToGuid();
            v.Name += "X";
            v = await _db.Persons1.UpdateAsync(v);
            Assert.NotNull(v);
            Assert.AreEqual(5.ToGuid(), v.Id);
            Assert.AreEqual("MikeX", v.Name);
        }

        [Test]
        public async Task Update2Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons2.UpdateAsync(null));

            // Get previous.
            var v = await _db.Persons2.GetAsync(5.ToGuid());
            Assert.NotNull(v);

            // Update testing.
            v.Id = 404.ToGuid();
            ExpectException.Throws<NotFoundException>("*", () => _db.Persons2.UpdateAsync(v));

            v.Id = 5.ToGuid();
            v.Name += "X";
            v.ChangeLog = null;
            v = await _db.Persons2.UpdateAsync(v);
            Assert.NotNull(v);
            Assert.AreEqual(5.ToGuid(), v.Id);
            Assert.AreEqual("MikeX", v.Name);
            Assert.IsNotNull(v.ChangeLog);
            Assert.IsNotNull(v.ChangeLog.CreatedBy);
            Assert.IsNotNull(v.ChangeLog.CreatedDate);
            Assert.IsNotNull(v.ChangeLog.UpdatedBy);
            Assert.IsNotNull(v.ChangeLog.UpdatedDate);
        }

        [Test]
        public async Task Update3Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons3.UpdateAsync(null));

            // Get previous.
            var v = await _db.Persons3.GetAsync(5.ToGuid());
            Assert.NotNull(v);

            // Update testing.
            v.Id = 404.ToGuid();
            ExpectException.Throws<NotFoundException>("*", () => _db.Persons3.UpdateAsync(v));

            v.Id = 5.ToGuid();
            v.Name += "X";
            v.ChangeLog = null;
            v = await _db.Persons3.UpdateAsync(v);
            Assert.NotNull(v);
            Assert.AreEqual(5.ToGuid(), v.Id);
            Assert.AreEqual("MikeX", v.Name);
            Assert.IsNotNull(v.ChangeLog);
            Assert.IsNotNull(v.ChangeLog.CreatedBy);
            Assert.IsNotNull(v.ChangeLog.CreatedDate);
            Assert.IsNotNull(v.ChangeLog.UpdatedBy);
            Assert.IsNotNull(v.ChangeLog.UpdatedDate);
        }

        [Test]
        public async Task Delete1Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons1.DeleteAsync());
            ExpectException.Throws<NotSupportedException>("Only a single key value is currently supported.", () => _db.Persons1.DeleteAsync(1, 2));

            ExpectException.Throws<NotFoundException>("*", () => _db.Persons1.DeleteAsync(404.ToGuid()));
            ExpectException.Throws<NotFoundException>("*", () => _db.Persons1.DeleteAsync(100.ToGuid()));
            await _db.Persons1.DeleteAsync(4.ToGuid());

            using (var r = await _db.Persons1.Container.ReadItemStreamAsync(4.ToGuid().ToString(), Microsoft.Azure.Cosmos.PartitionKey.None))
            {
                Assert.NotNull(r);
                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, r.StatusCode);
            };
        }

        [Test]
        public async Task Delete2Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons2.DeleteAsync());
            ExpectException.Throws<NotSupportedException>("Only a single key value is currently supported.", () => _db.Persons2.DeleteAsync(1, 2));

            ExpectException.Throws<NotFoundException>("*", () => _db.Persons2.DeleteAsync(404.ToGuid()));
            ExpectException.Throws<NotFoundException>("*", () => _db.Persons2.DeleteAsync(100.ToGuid()));
            await _db.Persons2.DeleteAsync(4.ToGuid());

            using (var r = await _db.Persons2.Container.ReadItemStreamAsync(4.ToGuid().ToString(), Microsoft.Azure.Cosmos.PartitionKey.None))
            {
                Assert.NotNull(r);
                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, r.StatusCode);
            };
        }

        [Test]
        public async Task Delete3Async()
        {
            ExpectException.Throws<ArgumentNullException>("*", () => _db.Persons3.DeleteAsync());
            ExpectException.Throws<NotSupportedException>("Only a single key value is currently supported.", () => _db.Persons3.DeleteAsync(1, 2));

            ExpectException.Throws<NotFoundException>("*", () => _db.Persons3.DeleteAsync(404.ToGuid()));
            ExpectException.Throws<NotFoundException>("*", () => _db.Persons3.DeleteAsync(100.ToGuid()));
            await _db.Persons3.DeleteAsync(4.ToGuid());

            using (var r = await _db.Persons3.Container.ReadItemStreamAsync(4.ToGuid().ToString(), Microsoft.Azure.Cosmos.PartitionKey.None))
            {
                Assert.NotNull(r);
                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, r.StatusCode);
            };

            using (var r = await _db.Persons3.Container.ReadItemStreamAsync(100.ToGuid().ToString(), Microsoft.Azure.Cosmos.PartitionKey.None))
            {
                Assert.NotNull(r);
                Assert.AreEqual(System.Net.HttpStatusCode.OK, r.StatusCode);
            };
        }
    }
}
