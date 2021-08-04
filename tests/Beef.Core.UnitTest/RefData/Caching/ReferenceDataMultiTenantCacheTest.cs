// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using Beef.RefData;
using Beef.RefData.Caching;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.RefData.Caching
{
    [TestFixture]
    public class ReferenceDataMultiTenantCacheTest
    {
        private static int _count08;
        private static int _count09;

        private class TestRd : ReferenceDataBaseInt32
        {
            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        private class TestRdCollection : ReferenceDataCollectionBase<TestRd>
        { }

        private Guid GetGuid(int value)
        {
            return new Guid(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        private Task<TestRdCollection> GetData()
        {
            TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId}: GetData {ExecutionContext.Current.TenantId}");

            if (ExecutionContext.Current.TenantId == GetGuid(08))
            {
                _count08++;
                System.Threading.Thread.Sleep(50);
                return Task.FromResult(new TestRdCollection
                {
                    new TestRd { Id = 50, Code = "EE" },
                    new TestRd { Id = 51, Code = "FF" }
                });
            }
            if (ExecutionContext.Current.TenantId == GetGuid(09))
            {
                _count09++;
                System.Threading.Thread.Sleep(60);
                return Task.FromResult(new TestRdCollection
                {
                    new TestRd { Id = 5, Code = "E" },
                    new TestRd { Id = 6, Code = "F" }
                });
            }
            if (ExecutionContext.Current.TenantId == GetGuid(1))
                return Task.FromResult(new TestRdCollection
                {
                    new TestRd { Id = 1, Code = "A" },
                    new TestRd { Id = 2, Code = "B" }
                });
            else
            {
                return Task.FromResult(new TestRdCollection
                {
                    new TestRd { Id = 7, Code = "X" },
                    new TestRd { Id = 8, Code = "Y" },
                    new TestRd { Id = 9, Code = "Z" }
                });
            }
        }

        [Test]
        public void Exercise()
        {
            var sp = UnitTest.Caching.Policy.CachePolicyManagerTest.TestSetUp();

            int i = 0;
            var rdc = new ReferenceDataMultiTenantCache<TestRdCollection, TestRd>(() => { i++; return GetData(); });

            // Set an execution context.
            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(1), ServiceProvider = sp });

            // Nothing loaded.
            Assert.AreEqual(0, i);
            Assert.AreEqual(0, rdc.Count);
            Assert.IsTrue(rdc.IsExpired);

            // Now loaded.
            var c = rdc.GetCollection();
            Assert.AreEqual(1, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(1, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Same cached version.
            c = rdc.GetCollection();
            Assert.AreEqual(1, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(1, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Change the execution context.
            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(2), ServiceProvider = sp });

            // Now load new tenant.
            c = rdc.GetCollection();
            Assert.AreEqual(2, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(3, c.ActiveList.Count);
            Assert.AreEqual(2, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Flush the cache.
            rdc.Flush(true);
            Assert.AreEqual(2, i);
            Assert.IsTrue(rdc.IsExpired);
            Assert.AreEqual(0, rdc.Count);

            // New collection cached.
            c = rdc.GetCollection();
            Assert.AreEqual(3, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(3, c.ActiveList.Count);
            Assert.AreEqual(1, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);
        }

        [Test]
        public void Concurrency()
        {
            var sp = UnitTest.Caching.Policy.CachePolicyManagerTest.TestSetUp();

            int i = 0;
            var rdc = new ReferenceDataMultiTenantCache<TestRdCollection, TestRd>(() => { i++; return GetData(); });

            ExecutionContext.Reset();

            // Set an execution context.
            var tasks = new Task[10];
            tasks[0] = Task.Run(() => Timer(0, GetGuid(08), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(08), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[1] = Task.Run(() => Timer(1, GetGuid(09), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(09), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[2] = Task.Run(() => Timer(2, GetGuid(1), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(1), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[3] = Task.Run(() => Timer(3, GetGuid(2), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(2), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[4] = Task.Run(() => Timer(4, GetGuid(08), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(08), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[5] = Task.Run(() => Timer(5, GetGuid(09), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(09), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[6] = Task.Run(() => Timer(6, GetGuid(1), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(1), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[7] = Task.Run(() => Timer(7, GetGuid(2), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(2), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[8] = Task.Run(() => Timer(8, GetGuid(08), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(08), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[9] = Task.Run(() => Timer(9, GetGuid(09), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(09), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));

            Task.WaitAll(tasks);

            TestContext.WriteLine("ROUND TWO");

            tasks = new Task[10];
            tasks[0] = Task.Run(() => Timer(0, GetGuid(08), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(08), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[1] = Task.Run(() => Timer(1, GetGuid(09), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(09), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[2] = Task.Run(() => Timer(2, GetGuid(1), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(1), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[3] = Task.Run(() => Timer(3, GetGuid(2), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(2), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[4] = Task.Run(() => Timer(4, GetGuid(08), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(08), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[5] = Task.Run(() => Timer(5, GetGuid(09), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(09), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[6] = Task.Run(() => Timer(6, GetGuid(1), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(1), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[7] = Task.Run(() => Timer(7, GetGuid(2), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(2), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[8] = Task.Run(() => Timer(8, GetGuid(08), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(08), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));
            tasks[9] = Task.Run(() => Timer(9, GetGuid(09), () => { ExecutionContext.SetCurrent(new ExecutionContext { TenantId = GetGuid(09), ServiceProvider = sp }); Assert.IsTrue(rdc.GetCollection().ActiveList.Count > 0); }));

            Task.WaitAll(tasks);
        }

        public TestContext TestContext { get; set; }

        private void Timer(int number, Guid key, Action a)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            a?.Invoke();
            sw.Stop();
            TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId}: {number}, {key}, {sw.ElapsedMilliseconds}");
        }

        [Test]
        public void Exercise_SlidingCache()
        {
            UnitTest.Caching.Policy.CachePolicyManagerTest.TestSetUp();

            var coll = new TestRdCollection
                {
                    new TestRd { Id = 1, Code = "A" },
                    new TestRd { Id = 2, Code = "B" }
                };

            var policy = new SlidingCachePolicy
            {
                Duration = new TimeSpan(0, 0, 3),
                MaxDuration = new TimeSpan(0, 0, 4)
            };

            int dataRequests = 0;
            var rdc = new ReferenceDataMultiTenantCache<TestRdCollection, TestRd>(() => { dataRequests++; return GetData(); });

            CachePolicyManager.Current.Set(rdc.PolicyKey, policy);
            ExecutionContext.Current.TenantId = GetGuid(08);

            // Nothing loaded.
            Assert.AreEqual(0, dataRequests);
            Assert.AreEqual(0, rdc.Count);
            Assert.IsTrue(rdc.IsExpired);

            // Now loaded.
            var c = rdc.GetCollection();
            Assert.AreEqual(1, dataRequests);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(1, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Multiple cycles to test Max cache
            for (int cycle = 0; cycle < 3; cycle++)
            {
                // Running till max cache expires.
                while (!rdc.IsExpired)
                {
                    c = rdc.GetCollection();
                    Assert.AreEqual(1 + cycle, dataRequests);
                    Assert.IsNotNull(c);
                    Assert.AreEqual(2, c.ActiveList.Count);
                    Assert.AreEqual(1, rdc.Count);
                    Assert.IsFalse(rdc.IsExpired);
                    System.Threading.Thread.Sleep((int)Math.Floor(policy.Duration.TotalMilliseconds / 2));
                }

                // reload collection cached.
                c = rdc.GetCollection();
                Assert.AreEqual(2 + cycle, dataRequests);
                Assert.IsNotNull(c);
                Assert.AreEqual(2, c.ActiveList.Count);
                Assert.AreEqual(1, rdc.Count);
                Assert.IsFalse(rdc.IsExpired);

                // New collection cached.
                c = rdc.GetCollection();
                Assert.AreEqual(2 + cycle, dataRequests);
                Assert.IsNotNull(c);
                Assert.AreEqual(2, c.ActiveList.Count);
                Assert.AreEqual(1, rdc.Count);
                Assert.IsFalse(rdc.IsExpired);
            }
        }
    }
}
