// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Executors;
using Beef.Executors.Triggers;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Executors
{
    [TestFixture]
    public class ExecutorBaseTest
    {
        public ExecutorBaseTest()
        {
            Logger.EnabledLogMessageTypes = LogMessageType.All;
            Logger.RegisterGlobal((a) => TestContext.WriteLine($"{a.ToString()} [{Thread.CurrentThread.ManagedThreadId}]"));
            ExecutionManager.IsInternalTracingEnabledForAll = true;
        }

        public TestContext TestContext { get; set; }

        #region OnStarted

        [Test]
        public void Run_NoArgs_OnStarted_Exception_Continue()
        {
            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(100); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Continue;
            em.Run();
            Assert.AreEqual(5, i);
            Assert.AreEqual(5, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNull(em.StopExecutor);
        }

        [Test]
        public void Run_NoArgs_OnStarted_Exception_Stop()
        {
            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(100); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Stop;
            em.Run();
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.ExecutorExceptionStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNotNull(em.StopExecutor);
            Assert.AreEqual(ExecutorResult.Unsuccessful, em.StopExecutor.Result);
            Assert.IsNotNull(em.StopExecutor.Exception);
            Assert.IsInstanceOf(typeof(NotSupportedException), em.StopExecutor.Exception);
        }

        [Test]
        public void Run_NoArgs_OnStarted_StopOk_Continue()
        {
            Assert.Inconclusive("Need to investigate");

            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(101); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Continue;
            em.Run();
            Assert.AreEqual(5, i);
            Assert.AreEqual(5, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNull(em.StopExecutor);
        }

        [Test]
        public void Run_NoArgs_OnStarted_StopOk_Stop()
        {
            Assert.Inconclusive("Need to investigate");

            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(101); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Stop;
            em.Run();
            Assert.AreEqual(5, i);
            Assert.AreEqual(5, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNull(em.StopExecutor);
            Assert.AreEqual(ExecutorResult.Successful, em.StopExecutor.Result);
            Assert.IsNull(em.StopExecutor.Exception);
        }

        #endregion

        [Test]
        public void Run_NoArgs_OnRunException_Continue()
        {
            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(200); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Continue;
            em.Run();
            Assert.AreEqual(5, i);
            Assert.AreEqual(5, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNull(em.StopExecutor);
        }

        [Test]
        public void Run_NoArgs_OnRunException_Stop()
        {
            Assert.Inconclusive("Need to investigate");

            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(200); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Stop;
            em.Run();
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.ExecutorExceptionStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNotNull(em.StopExecutor);
            Assert.AreEqual(ExecutorResult.Unsuccessful, em.StopExecutor.Result);
            Assert.IsNotNull(em.StopExecutor.Exception);
            Assert.IsInstanceOf(typeof(DivideByZeroException), em.StopExecutor.Exception);
        }

        [Test]
        public void Run_NoArgs_OnStoppedException_Continue()
        {
            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(300); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Continue;
            em.Run();
            Assert.AreEqual(5, i);
            Assert.AreEqual(5, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.TriggerStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNull(em.StopExecutor);
        }

        [Test]
        public void Run_NoArgs_OnStoppedException_Stop()
        {
            Assert.Inconclusive("Need to investigate");

            var i = 0;
            var em = ExecutionManager.Create(() => { i++; return new TestNotArgsExe(300); }, new TimerTrigger(10, 5));
            em.ExceptionHandling = ExceptionHandling.Stop;
            em.Run();
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, em.ExecutorCount);
            Assert.AreEqual(ExecutionManagerStopReason.ExecutorExceptionStop, em.StopReason);
            Assert.AreEqual(TriggerResult.Successful, em.Trigger.Result);
            Assert.IsNotNull(em.StopExecutor);
            Assert.AreEqual(ExecutorResult.Unsuccessful, em.StopExecutor.Result);
            Assert.IsNotNull(em.StopExecutor.Exception);
            Assert.IsInstanceOf(typeof(NotImplementedException), em.StopExecutor.Exception);
        }

        private class TestNotArgsExe : ExecutorBase
        {
            private readonly int _throwState;

            public TestNotArgsExe(int throwState) => _throwState = throwState;

            protected override Task OnStartedAsync()
            {
                if (_throwState == 100)
                    throw new NotSupportedException();
                else if (_throwState == 101)
                    Stop();
                else if (_throwState == 102)
                    Stop(new Exception("Stop"));
                else if (_throwState == 103)
                    StopExecutionManager();
                else if (_throwState == 104)
                    StopExecutionManager(new Exception("StopExecutionManager"));

                return Task.CompletedTask;
            }

            protected override Task OnRunAsync(ExecutorRunArgs args)
            {
                if (_throwState == 2)
                    throw new DivideByZeroException();

                return Task.CompletedTask;
            }

            protected override Task OnStoppedAsync()
            {
                if (_throwState == 3)
                    throw new NotImplementedException();

                return Task.CompletedTask;
            }
        }
    }
}
