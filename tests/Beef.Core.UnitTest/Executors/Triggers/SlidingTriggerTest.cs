// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Executors;
using Beef.Executors.Triggers;
using NUnit.Framework;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Executors.Triggers
{
    [TestFixture]
    public class SlidingTriggerTest
    {
        private static readonly int iterations = 10;

        public SlidingTriggerTest()
        {
            Logger.EnabledLogMessageTypes = LogMessageType.All;
            Logger.RegisterGlobal((a) => TestContext.WriteLine($"{a.ToString()} [{Thread.CurrentThread.ManagedThreadId}]"));
            ExecutionManager.IsInternalTracingEnabledForAll = true;
        }

        public TestContext TestContext { get; set; }

        [Test]
        public void Run_NoArgs_Fast()
        {
            int i = 0;
            var sw = Stopwatch.StartNew();
            var em = ExecutionManager.Create(async (x) => { i++; TestContext.WriteLine($"i: {i}"); await Task.CompletedTask; }, new SlidingTimerTrigger(10, iterations)).Run();
            sw.Stop();
            Assert.AreEqual(iterations, i);
            Assert.AreEqual(iterations, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
        }

        [Test]
        public void Run_WithArgs_Fast()
        {
            int i = 0;
            var tt = new TestTrigger();
            var sw = Stopwatch.StartNew();
            var em = ExecutionManager.CreateWithArgs<string>(async (x) => { i++; TestContext.WriteLine($"i: {i}"); Assert.AreEqual("XXX", x.Value); await Task.CompletedTask; }, tt).Run();
            sw.Stop();
            Assert.AreEqual(iterations, i);
            Assert.AreEqual(iterations, tt.Count);
            Assert.AreEqual(iterations, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
        }

        [Test]
        public void Run_NoArgs_Slow()
        {
            int i = 0;
            var sw = Stopwatch.StartNew();
            var em = ExecutionManager.Create(async (x) => { i++; TestContext.WriteLine($"i: {i}"); await Task.Delay(200); }, new SlidingTimerTrigger(10, iterations)).Run();
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds > 2000, sw.ElapsedMilliseconds.ToString());
            Assert.AreEqual(iterations, i);
            Assert.AreEqual(iterations, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
        }

        [Test]
        public void Run_WithArgs_Slow()
        {
            int i = 0;
            var tt = new TestTrigger();
            var sw = Stopwatch.StartNew();
            var em = ExecutionManager.CreateWithArgs<string>(async (x) => { i++; TestContext.WriteLine($"i: {i}"); Assert.AreEqual("XXX", x.Value); await Task.Delay(200); }, tt).Run();
            sw.Stop();
            Assert.IsTrue(sw.ElapsedMilliseconds > 2000, sw.ElapsedMilliseconds.ToString());
            Assert.AreEqual(iterations, i);
            Assert.AreEqual(iterations, tt.Count);
            Assert.AreEqual(iterations, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
        }

        private class TestTrigger : SlidingTimerTrigger<string>
        {
            public TestTrigger() : base(10, SlidingTriggerTest.iterations) { }

            public int Count { get; set; }

            protected override TimerTriggerResult<string> OnTrigger()
            {
                Count++;
                return new TimerTriggerResult<string>("XXX");
            }
        }
    }
}
