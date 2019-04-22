// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Executors;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Executors.Triggers
{
    [TestFixture]
    public class SingleTriggerTest
    {
        [Test]
        public void Run_NoArgs()
        {
            int i = 0;
            var em = ExecutionManager.Create(async (x) => { i++; await Task.CompletedTask; }).Run();
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
        }

        [Test]
        public void Run_WithArgs()
        {
            int i = 0;
            var em = ExecutionManager.CreateWithArgs<string>(async (x) => { i++; Assert.AreEqual("XXX", x.Value); await Task.CompletedTask; }, "XXX").Run();
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
        }
    }
}
