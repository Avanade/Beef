// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Caching
{
    [TestFixture]
    public class TwoKeyValueCacheTest
    {
        [Test]
        public void GetAndContainsKeys()
        {
            var i = 0;
            var mtc = new TwoKeyValueCache<int, string, string>(
                (key1) => { i++; return (key1 == 99) ? (false, null, null) : (true, key1.ToString(), $"x{key1}x"); },
                (key2) => { i++; return (true, int.Parse(key2), $"x{int.Parse(key2)}x"); });

            // TryGetByKey1.
            Assert.IsFalse(mtc.ContainsKey1(1));
            Assert.AreEqual(0, i);

            Assert.IsFalse(mtc.ContainsKey2("1"));
            Assert.AreEqual(0, i);

            Assert.IsTrue(mtc.TryGetByKey1(1, out string val));
            Assert.AreEqual(1, i);
            Assert.AreEqual("x1x", val);

            Assert.IsTrue(mtc.TryGetByKey2("1", out val));
            Assert.AreEqual(1, i);
            Assert.AreEqual("x1x", val);

            Assert.IsTrue(mtc.ContainsKey1(1));
            Assert.IsTrue(mtc.ContainsKey2("1"));
            Assert.AreEqual(1, mtc.Count);

            // TryGetByKey2.
            Assert.IsFalse(mtc.ContainsKey1(2));
            Assert.AreEqual(1, i);

            Assert.IsFalse(mtc.ContainsKey2("2"));
            Assert.AreEqual(1, i);

            Assert.IsTrue(mtc.TryGetByKey2("2", out val));
            Assert.AreEqual(2, i);
            Assert.AreEqual("x2x", val);

            Assert.IsTrue(mtc.TryGetByKey1(2, out val));
            Assert.AreEqual(2, i);
            Assert.AreEqual("x2x", val);

            Assert.IsTrue(mtc.ContainsKey1(2));
            Assert.IsTrue(mtc.ContainsKey2("2"));
            Assert.AreEqual(2, mtc.Count);

            // TryGetByKey1-NotFound.
            Assert.IsFalse(mtc.ContainsKey1(99));
            Assert.AreEqual(2, i);

            Assert.IsFalse(mtc.ContainsKey2("99"));
            Assert.AreEqual(2, i);

            Assert.IsFalse(mtc.TryGetByKey1(99, out val));
            Assert.AreEqual(3, i);
            Assert.AreEqual(null, val);

            Assert.AreEqual(2, mtc.Count);
        }

        [Test]
        public void PolicyManager()
        {
            CachePolicyManager.Reset();

            var mtc = new TwoKeyValueCache<int, string, string>(
                (key1) => { return (key1 == 99) ? (false, null, null) : (true, key1.ToString(), $"x{key1}x"); },
                (key2) => { return (true, int.Parse(key2), $"x{int.Parse(key2)}x"); },
                "TwoKeyValueCacheTest");

            Assert.IsTrue(mtc.TryGetByKey1(1, out string val));
            Assert.IsTrue(mtc.TryGetByKey2("2", out val));

            var pa = CachePolicyManager.GetPolicies();
            Assert.AreEqual(2, pa.Length);

            // Check the internal nocachepolicy.
            var p0 = pa.Where(x => x.Key.StartsWith("TwoKeyValueCacheTest_")).SingleOrDefault();
            Assert.IsNotNull(p0);
            Assert.IsInstanceOf(typeof(NoCachePolicy), p0.Value);

            // Check the default policy for type.
            var p1 = pa.Where(x => x.Key == "TwoKeyValueCacheTest").SingleOrDefault();
            Assert.IsNotNull(p1);
            Assert.IsInstanceOf(typeof(NoExpiryCachePolicy), p1.Value);

            // Each value should have its own policy.
            var policy1 = mtc.GetPolicyByKey1(1);
            Assert.IsNotNull(policy1);
            Assert.IsInstanceOf(typeof(NoExpiryCachePolicy), policy1);

            var policy2 = mtc.GetPolicyByKey2("2");
            Assert.IsNotNull(policy2);
            Assert.IsInstanceOf(typeof(NoExpiryCachePolicy), policy2);
            Assert.AreNotSame(policy1, policy2);

            // There should be no policy where item not found.
            Assert.IsNull(mtc.GetPolicyByKey1(3));

            // Flush cache where not expired; nothing happens.
            mtc.Flush();
            var v = mtc.GetByKey1(1);
            Assert.AreEqual("x1x", v);
            Assert.AreEqual(2, mtc.Count);

            // Force flush; should reload cache after.
            mtc.Flush(true);
            Assert.AreEqual(0, mtc.Count);
            Assert.IsFalse(mtc.ContainsKey1(1));
        }

        [Test]
        public void Concurrency()
        {
            // No way to effectively validate; console output needs to be reviewed.

            int key1Count = 0;
            int key2Count = 0;
            int dataValue1 = 100;
            int dataValue2 = 200;

            var random = new System.Random();

            var mtc = new TwoKeyValueCache<int, string, string>(
                (key1) => { System.Console.WriteLine($"get1 >> {key1Count++}"); Thread.Sleep(random.Next(0, 10)); return (key1 == 99) ? (false, null, null) : (true, key1.ToString(), $"x{dataValue1++}x"); },
                (key2) => { System.Console.WriteLine($"get2 >> {key2Count++}"); Thread.Sleep(random.Next(0, 10)); return (true, int.Parse(key2), $"y{dataValue2++}y"); },
                "TwoKeyValueCacheTest2");

            var tasks = new Task[9];

            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                { 
                    tasks[0] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"0 >> {r}"); });
                    tasks[1] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"1 >> {r}"); });
                    tasks[2] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"2 >> {r}"); });
                    tasks[3] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"3 >> {r}"); });
                    tasks[4] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); System.Console.WriteLine("4 >> removed"); mtc.Remove2("1"); });
                    tasks[5] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"5 >> {r}"); });
                    tasks[6] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"6 >> {r}"); });
                    tasks[7] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"7 >> {r}"); });
                    tasks[8] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"8 >> {r}"); });

                    Task.WaitAll(tasks);

                    System.Console.WriteLine($"one> key1: {key1Count}; key2: {key2Count}");
                }
                else
                {
                    tasks[0] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"0 >> {r}"); });
                    tasks[1] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"1 >> {r}"); });
                    tasks[2] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"2 >> {r}"); });
                    tasks[3] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"3 >> {r}"); });
                    tasks[4] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); System.Console.WriteLine("4 >> removed"); mtc.Remove1(1); });
                    tasks[5] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"5 >> {r}"); });
                    tasks[6] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey1(1); System.Console.WriteLine($"6 >> {r}"); });
                    tasks[7] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"7 >> {r}"); });
                    tasks[8] = Task.Run(() => { Thread.Sleep(random.Next(0, 10)); var r = mtc.GetByKey2("1"); System.Console.WriteLine($"8 >> {r}"); });

                    Task.WaitAll(tasks);

                    System.Console.WriteLine($"two> key1: {key1Count}; key2: {key2Count}");
                }
            }
        }
    }
}
