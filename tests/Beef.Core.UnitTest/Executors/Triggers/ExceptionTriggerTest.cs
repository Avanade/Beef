// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Executors;
using Beef.Executors.Triggers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Executors.Triggers
{
    [TestFixture]
    public class ExceptionTriggerTest
    {
        [Test]
        public void OnStarted_Exception()
        {
            int i = 0;
            var em = ExecutionManager.Create(async (x) => { i++; await Task.CompletedTask; }, new TestTrigger(1)).Run();
            Assert.AreEqual(0, i);
            Assert.AreEqual(0, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerException, em.StopReason);
            Assert.AreEqual(TriggerResult.Unsuccessful, em.Trigger.Result);
            Assert.IsNotNull(em.Trigger.Exception);
            Assert.IsInstanceOf(typeof(DivideByZeroException), em.Trigger.Exception);
        }

        [Test]
        public void OnStopped_Exception()
        {
            int i = 0;
            var em = ExecutionManager.Create(async (x) => { i++; await Task.CompletedTask; }, new TestTrigger(2)).Run();
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerException, em.StopReason);
            Assert.AreEqual(TriggerResult.Unsuccessful, em.Trigger.Result);
            Assert.IsNotNull(em.Trigger.Exception);
            Assert.IsInstanceOf(typeof(DivideByZeroException), em.Trigger.Exception);
        }

        private class TestTrigger : TriggerBase
        {
            private readonly int _throwState;

            public TestTrigger(int throwState)
            {
                _throwState = throwState;
            }

            private void CheckAndThrow(int throwState)
            {
                if (throwState == _throwState)
                    throw new DivideByZeroException("Test trigger throw state match.");
            }

            protected override void OnStarted()
            {
                CheckAndThrow(1);
                Run(() => Stop());
            }

            protected override void OnStopped()
            {
                CheckAndThrow(2);
            }
        }
    }
}
