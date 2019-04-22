// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Caching
{
    [TestFixture]
    public class KeyedLockTest
    {
        public TestContext TestContext { get; set; }

        [Test]
        public void LockOnSameKey()
        {
            var kl = new KeyedLock<int>();
            var cd = new ConcurrentDictionary<int, int>();

            var inLock = false;

            Parallel.ForEach(new int[] { 1, 1, 1, 1, 1, 1 }, (key) =>
            {
                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} X");

                kl.Lock(key, () =>
                {
                    Assert.IsFalse(inLock);

                    inLock = true;
                    TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XX");
                    System.Threading.Thread.Sleep(10);
                    TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXX");
                    inLock = false;
                });

                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXXX");
            });
        }

        [Test]
        public void LockOnSameKey2()
        {
            var kl = new KeyedLock<int>();
            var cd = new ConcurrentDictionary<int, int>();

            var inLock = false;

            Parallel.ForEach(new int[] { 1, 1, 1, 1, 1, 1 }, (key) =>
            {
                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} X");

                kl.Lock<int>(key, () =>
                {
                    Assert.IsFalse(inLock);

                    inLock = true;
                    TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XX");
                    System.Threading.Thread.Sleep(10);
                    TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXX");
                    inLock = false;
                    return key;
                });

                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXXX");
            });
        }

        [Test]
        public void LockOnDifferentKey()
        {
            var kl = new KeyedLock<int>();
            var cd = new ConcurrentDictionary<int, int>();

            var inLock1 = false;
            var inLock2 = false;

            Parallel.ForEach(new int[] { 1, 2, 1, 2, 1, 2 }, (key) =>
            {
                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} X");

                kl.Lock(key, () =>
                {
                    if (key == 1)
                    {
                        Assert.IsFalse(inLock1);

                        inLock1 = true;
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XX");
                        System.Threading.Thread.Sleep(10);
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXX");
                        inLock1 = false;
                    }
                    else
                    {
                        Assert.IsFalse(inLock2);

                        inLock2 = true;
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XX");
                        System.Threading.Thread.Sleep(10);
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXX");
                        inLock2 = false;
                    }
                });

                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXXX");
            });
        }

        [Test]
        public void LockOnDifferentKey2()
        {
            var kl = new KeyedLock<int>();
            var cd = new ConcurrentDictionary<int, int>();

            var inLock1 = false;
            var inLock2 = false;

            Parallel.ForEach(new int[] { 1, 2, 1, 2, 1, 2 }, (key) =>
            {
                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} X");

                kl.Lock<int>(key, () =>
                {
                    if (key == 1)
                    {
                        Assert.IsFalse(inLock1);

                        inLock1 = true;
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XX");
                        System.Threading.Thread.Sleep(10);
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXX");
                        inLock1 = false;
                    }
                    else
                    {
                        Assert.IsFalse(inLock2);

                        inLock2 = true;
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XX");
                        System.Threading.Thread.Sleep(10);
                        TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXX");
                        inLock2 = false;
                    }

                    return key;
                });

                TestContext.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} XXXX");
            });
        }
    }
}
