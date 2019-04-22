// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Caching
{
    [TestFixture]
    public class KeyValueCacheTest
    {
        public static Guid ToGuid(int value)
        {
            return new Guid(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void GetAndContainsKey()
        {
            var i = 0;
            var mtc = new KeyValueCache<int, string>((key) => { i++; return key.ToString(); });

            Assert.IsFalse(mtc.ContainsKey(1));
            Assert.AreEqual(0, i);
            Assert.IsTrue(mtc.TryGetByKey(1, out string val));
            Assert.AreEqual(1, i);
            Assert.AreEqual("1", val);
            Assert.IsTrue(mtc.ContainsKey(1));
        }

        [Test]
        public void PolicyManager()
        {
            CachePolicyManager.Reset();

            var i = 0;
            var mtc = new KeyValueCache<int, string>((key) => { i++; return key.ToString(); }, "KeyValueCacheTest");
            Assert.IsNotNull(mtc.PolicyKey);

            var policy = new DailyCachePolicy();
            CachePolicyManager.Set(mtc.PolicyKey, policy);

            var policy2 = mtc.GetPolicy();
            Assert.IsNotNull(policy2);
            Assert.AreSame(policy, policy2);

            var pa = CachePolicyManager.GetPolicies();
            Assert.AreEqual(2, pa.Length);

            // Check the internal nocachepolicy.
            var p0 = pa.Where(x => x.Key.StartsWith("KeyValueCacheTest_")).SingleOrDefault();
            Assert.IsNotNull(p0);
            Assert.IsInstanceOf(typeof(NoCachePolicy), p0.Value);

            // Check the default policy for type.
            var p1 = pa.Where(x => x.Key == "KeyValueCacheTest").SingleOrDefault();
            Assert.IsNotNull(p1);
            Assert.IsInstanceOf(typeof(DailyCachePolicy), p1.Value);

            // Get (add) new item to cache.
            var s = mtc[1];
            Assert.AreEqual("1", s);
            Assert.AreEqual(1, i);

            // No new globally managed policies should have been created.
            pa = CachePolicyManager.GetPolicies();
            Assert.AreEqual(2, pa.Length);

            // Check policy for item is DailyCachePolicy but has its own instance.
            policy2 = mtc.GetPolicyByKey(1);
            Assert.IsNotNull(policy2);
            Assert.IsInstanceOf(typeof(DailyCachePolicy), policy2);
            Assert.AreNotSame(policy, policy2);

            // There should be no policy where item not found.
            Assert.IsNull(mtc.GetPolicyByKey(2));

            // Flush cache where not expired.
            mtc.Flush();
            s = mtc[1];
            Assert.AreEqual("1", s);
            Assert.AreEqual(1, i);

            // Force flush; should reload cache after.
            CachePolicyManager.ForceFlush();
            s = mtc[1];
            Assert.AreEqual("1", s);
            Assert.AreEqual(2, i);
        }

        public TestContext TestContext { get; set; }

        [Test]
        public void Concurrency()
        {
            var l = new object();
            CachePolicyManager.Reset();
            int i = 0;
            var mtc = new KeyValueCache<int, string>((key) => { lock (l) { i++; } TestContext.WriteLine($"GetValue {key} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]"); System.Threading.Thread.Sleep(20); return key.ToString(); });

            // Set an execution context.
            var tasks = new Task[13];
            tasks[0] = Task.Run(() => Timer(0, 1, () => { Assert.AreEqual(mtc[1], "1"); }));
            tasks[1] = Task.Run(() => Timer(1, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[2] = Task.Run(() => Timer(2, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[3] = Task.Run(() => Timer(3, 3, () => { Assert.AreEqual(mtc[3], "3"); }));
            tasks[4] = Task.Run(() => Timer(4, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[5] = Task.Run(() => Timer(5, 1, () => { Assert.AreEqual(mtc[1], "1"); }));
            tasks[6] = Task.Run(() => Timer(6, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[7] = Task.Run(() => Timer(7, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[8] = Task.Run(() => Timer(8, 3, () => { Assert.AreEqual(mtc[3], "3"); }));
            tasks[9] = Task.Run(() => Timer(9, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[10] = Task.Run(() => Timer(10, 1, () => { Assert.AreEqual(mtc[1], "1"); }));
            tasks[11] = Task.Run(() => Timer(11, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[12] = Task.Run(() => Timer(12, 3, () => { Assert.AreEqual(mtc[3], "3"); }));

            Task.WaitAll(tasks);

            Assert.AreEqual(3, i);

            TestContext.WriteLine("Round 2");

            tasks = new Task[13];
            tasks[0] = Task.Run(() => Timer(0, 1, () => { Assert.AreEqual(mtc[1], "1"); }));
            tasks[1] = Task.Run(() => Timer(1, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[2] = Task.Run(() => Timer(2, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[3] = Task.Run(() => Timer(3, 3, () => { Assert.AreEqual(mtc[3], "3"); }));
            tasks[4] = Task.Run(() => Timer(4, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[5] = Task.Run(() => Timer(5, 1, () => { Assert.AreEqual(mtc[1], "1"); }));
            tasks[6] = Task.Run(() => Timer(6, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[7] = Task.Run(() => Timer(7, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[8] = Task.Run(() => Timer(8, 3, () => { Assert.AreEqual(mtc[3], "3"); }));
            tasks[9] = Task.Run(() => Timer(9, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[10] = Task.Run(() => Timer(10, 1, () => { Assert.AreEqual(mtc[1], "1"); }));
            tasks[11] = Task.Run(() => Timer(11, 2, () => { Assert.AreEqual(mtc[2], "2"); }));
            tasks[12] = Task.Run(() => Timer(12, 3, () => { Assert.AreEqual(mtc[3], "3"); }));

            Task.WaitAll(tasks);

            Assert.AreEqual(3, i);
        }

        private void Timer(int number, int key, Action a)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            a?.Invoke();
            sw.Stop();
            TestContext.WriteLine($"{number}, {key}, {sw.ElapsedMilliseconds}ms [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
        }
    }
}
