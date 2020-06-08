using Beef.Events.Triggers.Config;
using Beef.Events.Triggers.Listener;
using Beef.Events.Triggers.PoisonMessages;
using EventHubs = Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Beef.Test.NUnit;
using Beef.Events.Subscribe;

namespace Beef.Events.UnitTest.Triggers
{
    [TestFixture]
    public class ResilientEventHubProcessorTest
    {
        private static PoisonPesist CreatePoisonPersistence()
        {
            var pp = new PoisonPesist();
            PoisonMessagePersistence.Register((_) => pp);
            return pp;
        }

        private class PoisonPesist : IPoisonMessagePersistence
        {
            private readonly object _lock = new object();

            public PoisonMessageAction Result { get; private set; } = PoisonMessageAction.NotPoison;

            public List<EventHubs.EventData> AddedEvents { get; } = new List<EventHubs.EventData>();

            public List<EventHubs.EventData> RemovedEvents { get; } = new List<EventHubs.EventData>();

            public Task<PoisonMessageAction> CheckAsync(EventHubs.EventData @event) => Task.FromResult(Result);

            public List<EventHubs.EventData> AuditedEvents { get; } = new List<EventHubs.EventData>();

            public Task RemoveAsync(EventHubs.EventData @event, PoisonMessageAction action)
            {
                lock (_lock)
                {
                    RemovedEvents.Add(@event);
                    Result = PoisonMessageAction.NotPoison;
                    return Task.CompletedTask;
                }
            }

            public Task SetAsync(EventHubs.EventData @event, Exception exception)
            {
                lock (_lock)
                {
                    AddedEvents.Add(@event);
                    Result = PoisonMessageAction.PoisonRetry;
                    return Task.CompletedTask;
                }
            }

            public void SetResult(PoisonMessageAction pma)
            {
                lock (_lock)
                {
                    Result = pma;
                }
            }

            public Task SkipAuditAsync(EventHubs.EventData @event, string exceptionText)
            {
                lock (_lock)
                {
                    AuditedEvents.Add(@event);
                    return Task.CompletedTask;
                }
            }
        }

        private class FuncExe : ITriggeredFunctionExecutor
        {
            public int TotalSuccess { get; set; }

            public int TotalError { get; set; }

            public Task<FunctionResult> TryExecuteAsync(TriggeredFunctionData input, CancellationToken cancellationToken)
            {
                var e = ((ResilientEventHubData)input.TriggerValue).EventData;
                e.Properties.TryGetValue("ExceptionCount", out var count);

                if (e.Properties.TryGetValue("StopInvalidData", out var exc) && exc is bool bexc && bexc == true)
                {
                    return Task.FromResult(new FunctionResult(false,
                         (Exception)typeof(EventSubscriberStopException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
                            new Type[] { typeof(Result) }, null)
                            .Invoke(new object[] { CreateResult(SubscriberStatus.InvalidData, null) })));
                }

                if ((int)count > 0)
                {
                    e.Properties["ExceptionCount"] = (int)count - 1;
                    TotalError++;
                    return Task.FromResult(new FunctionResult(false, new DivideByZeroException()));
                }
                else
                {
                    TotalSuccess++;
                    return Task.FromResult(new FunctionResult(true));
                }
            }

            private Result CreateResult(SubscriberStatus status, Exception exception)
            {
                var result = (Result)typeof(Events.Subscribe.Result).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[0], null).Invoke(null);
                typeof(Result).GetProperty("Status", BindingFlags.Public | BindingFlags.Instance).SetValue(result, status);
                typeof(Result).GetProperty("Exception", BindingFlags.Public | BindingFlags.Instance).SetValue(result, exception);
                typeof(Result).GetProperty("Reason", BindingFlags.Public | BindingFlags.Instance).SetValue(result, "Blah blah");
                return result;
            }
        }

        public static ResilientEventHubOptions CreateOptions()
        {
            return new ResilientEventHubOptions
            {
                EventHubPath = "path",
                EventHubName = "eventhub",
                FunctionName = "testfunc",
                FunctionType = "ns.class",
                MaxRetryTimespan = new TimeSpan(0, 0, 0, 0, 25),
                LogPoisonMessageAfterRetryCount = 3
            };
        }

        public static PartitionContext CreatePartitionContext()
        {
            var constructor = typeof(PartitionContext).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(EventProcessorHost), typeof(string), typeof(string), typeof(string), typeof(CancellationToken) },
                null);

            return (PartitionContext)constructor.Invoke(new object[] { null, "0", "path", "consumergroup", null });
        }

        public static EventHubs.EventData CreateEvent(int exceptionCount)
        {
            var e = new EventHubs.EventData(Encoding.UTF8.GetBytes(Entities.Cleaner.Clean(DateTime.Now).ToString()));
            e.Properties.Add("ExceptionCount", exceptionCount);
            return e;
        }

        private ResilientEventHubProcessor CreateProcessor(FuncExe exe, List<EventHubs.EventData> checkpoints) => new ResilientEventHubProcessor(exe, CreateOptions(), null, TestSetUp.CreateLogger())
        {
            Checkpointer = (c, e) =>
            {
                checkpoints.Add(e);
                return Task.CompletedTask;
            }
        };

        [Test]
        public async Task A010_OpenAndClose()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, cp.Count);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A020_SingleGood()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e = CreateEvent(0);
            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e });
            Assert.AreEqual(1, f.TotalSuccess);
            Assert.AreEqual(0, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A030_MultiGood()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e = CreateEvent(0);
            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { CreateEvent(0), CreateEvent(0), e });
            Assert.AreEqual(3, f.TotalSuccess);
            Assert.AreEqual(0, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A040_SingleRetry()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e = CreateEvent(1);
            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e });
            Assert.AreEqual(1, f.TotalSuccess);
            Assert.AreEqual(1, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A050_MultiRetry()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e1 = CreateEvent(1);
            var e2 = CreateEvent(1);
            var e3 = CreateEvent(1);
            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e1, e2, e3 });
            Assert.AreEqual(3, f.TotalSuccess);
            Assert.AreEqual(3, f.TotalError);
            Assert.AreEqual(3, cp.Count);
            Assert.AreSame(e1, cp[0]);
            Assert.AreSame(e2, cp[1]);
            Assert.AreSame(e3, cp[2]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A060_SinglePoison()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e = CreateEvent(5);
            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e });
            Assert.AreEqual(1, f.TotalSuccess);
            Assert.AreEqual(5, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(1, pp.AddedEvents.Count);
            Assert.AreSame(e, pp.AddedEvents[0]);
            Assert.AreEqual(1, pp.RemovedEvents.Count);
            Assert.AreSame(e, pp.RemovedEvents[0]);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A070_MultiPoison()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e1 = CreateEvent(5);
            var e2 = CreateEvent(0); // This is a good one!
            var e3 = CreateEvent(5);
            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e1, e2, e3 });
            Assert.AreEqual(3, f.TotalSuccess);
            Assert.AreEqual(10, f.TotalError);
            Assert.AreEqual(2, cp.Count);
            Assert.AreSame(e2, cp[0]);
            Assert.AreSame(e3, cp[1]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(2, pp.AddedEvents.Count);
            Assert.AreSame(e1, pp.AddedEvents[0]);
            Assert.AreSame(e3, pp.AddedEvents[1]);
            Assert.AreEqual(2, pp.RemovedEvents.Count);
            Assert.AreSame(e1, pp.RemovedEvents[0]);
            Assert.AreSame(e3, pp.RemovedEvents[1]);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A080_PoisonManualSkip()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e1 = CreateEvent(int.MaxValue);
            var e2 = CreateEvent(0);

            // Let it go run away.
            var t = p.ProcessEventsAsync(c, new EventHubs.EventData[] { e1, e2 });

            // Wait a wee bit before checking it is in retry loop.
            await Task.Delay(500);
            Assert.AreEqual(0, f.TotalSuccess);
            Console.WriteLine($"TotalError {f.TotalError}");
            Assert.IsTrue(f.TotalError > 0);
            Assert.AreEqual(0, cp.Count);

            Assert.AreEqual(1, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.PoisonRetry, pp.Result);

            // Skip the poison message and wait until all events are done.
            pp.SetResult(PoisonMessageAction.PoisonSkip);
            t.Wait();

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(1, f.TotalSuccess);
            Assert.AreEqual(1, pp.AddedEvents.Count);
            Assert.AreEqual(1, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A090_RestartFromPoisonRetry()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            pp.SetResult(PoisonMessageAction.PoisonRetry);

            await p.OpenAsync(c);
            var e = CreateEvent(0);

            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e });
            Assert.AreEqual(1, f.TotalSuccess);
            Assert.AreEqual(0, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(1, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A100_RestartFromPoisonSkip()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            pp.SetResult(PoisonMessageAction.PoisonSkip);

            await p.OpenAsync(c);
            var e = CreateEvent(0);

            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e });
            Assert.AreEqual(0, f.TotalSuccess);
            Assert.AreEqual(0, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(1, pp.RemovedEvents.Count);
            Assert.AreEqual(0, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }

        [Test]
        public async Task A110_PoisonMessageAuditAndContinueException()
        {
            var pp = CreatePoisonPersistence();
            var f = new FuncExe();
            var cp = new List<EventHubs.EventData>();
            var p = CreateProcessor(f, cp);
            var c = CreatePartitionContext();

            await p.OpenAsync(c);
            var e = CreateEvent(0);
            e.Properties.Add("StopInvalidData", true);

            await p.ProcessEventsAsync(c, new EventHubs.EventData[] { e });
            Assert.AreEqual(0, f.TotalSuccess);
            Assert.AreEqual(0, f.TotalError);
            Assert.AreEqual(1, cp.Count);
            Assert.AreSame(e, cp[0]);

            await p.CloseAsync(c, CloseReason.Shutdown);
            Assert.AreEqual(0, f.TotalSuccess);
            Assert.AreEqual(0, pp.AddedEvents.Count);
            Assert.AreEqual(0, pp.RemovedEvents.Count);
            Assert.AreEqual(1, pp.AuditedEvents.Count);
            Assert.AreEqual(PoisonMessageAction.NotPoison, pp.Result);
        }
    }
}