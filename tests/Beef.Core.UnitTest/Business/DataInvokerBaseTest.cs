using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beef.Business;

namespace Beef.Core.UnitTest.Business
{
    [TestFixture]
    public class DataInvokerBaseTest
    {
        [Test]
        public async Task InvokeAsync_SameThreadValue()
        {
            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(new ExecutionContext());
            var threadIds = new List<int> { Thread.CurrentThread.ManagedThreadId };

            var di = new DataInvoker();
            var val = await di.InvokeAsync(null, async () => { threadIds.Add(Thread.CurrentThread.ManagedThreadId); await Task.Delay(0); return 1; });
            threadIds.Add(Thread.CurrentThread.ManagedThreadId);

            Assert.AreEqual(1, val);
            Assert.AreEqual(1, threadIds.Distinct().Count());
        }

        [Test]
        public async Task InvokeAsync_SameThreadNoValue()
        {
            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(new ExecutionContext());
            var threadIds = new List<int> { Thread.CurrentThread.ManagedThreadId };

            var di = new DataInvoker();
            await di.InvokeAsync(null, async () => { threadIds.Add(Thread.CurrentThread.ManagedThreadId); await Task.Delay(0); });
            threadIds.Add(Thread.CurrentThread.ManagedThreadId);

            Assert.AreEqual(1, threadIds.Distinct().Count());
        }
    }
}