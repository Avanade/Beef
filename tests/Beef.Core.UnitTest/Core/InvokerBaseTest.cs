using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Core
{
    [TestFixture]
    public class InvokerBaseTest
    {
        private class TestInvoker : InvokerBase<TestInvoker, object> 
        {
            public List<int> ThreadIds = new List<int>();

            protected async override Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, object param = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
            {
                ThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                var res = await base.WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber).ConfigureAwait(false);
                ThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                return res;
            }

            protected async override Task WrapInvokeAsync(object caller, Func<Task> func, object param = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
            {
                ThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                await base.WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber).ConfigureAwait(false); ;
                ThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
            }
        }

        [Test]
        public async Task InvokeAsync_SameThreadValue()
        {
            var threadIds = new List<int> { Thread.CurrentThread.ManagedThreadId };

            var ti = new TestInvoker();
            var val = await ti.InvokeAsync(null, async () => { threadIds.Add(Thread.CurrentThread.ManagedThreadId); await Task.Delay(0); return 1; });
            threadIds.Add(Thread.CurrentThread.ManagedThreadId);

            Assert.AreEqual(1, val);
            Assert.AreEqual(1, threadIds.Distinct().Count());
            Assert.AreEqual(1, ti.ThreadIds.Distinct().Count());
            Assert.AreEqual(threadIds.Distinct().First(), ti.ThreadIds.Distinct().First());
        }

        [Test]
        public async Task InvokeAsync_SameThreadNoValue()
        {
            var threadIds = new List<int> { Thread.CurrentThread.ManagedThreadId };

            var ti = new TestInvoker();
            await ti.InvokeAsync(null, async () => { threadIds.Add(Thread.CurrentThread.ManagedThreadId); await Task.Delay(0); });
            threadIds.Add(Thread.CurrentThread.ManagedThreadId);

            Assert.AreEqual(1, threadIds.Distinct().Count());
            Assert.AreEqual(1, ti.ThreadIds.Distinct().Count());
            Assert.AreEqual(threadIds.Distinct().First(), ti.ThreadIds.Distinct().First());
        }
    }
}