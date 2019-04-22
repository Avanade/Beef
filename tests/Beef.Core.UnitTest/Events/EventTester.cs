// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Events
{
    [TestFixture, NonParallelizable]
    public class EventTester
    {
        [Test]
        public void Match()
        {
            Assert.IsFalse(Event.Match("domain.entity.123", "domain.entity.000"));
            Assert.IsFalse(Event.Match("domain.ytitne.123", "domain.entity.123"));
            Assert.IsFalse(Event.Match("domain.ytitne.*", "domain.entity.123"));
            Assert.IsFalse(Event.Match("domain.entity.123.*", "domain.entity.123"));

            Assert.IsTrue(Event.Match("domain.entity.123", "domain.entity.123"));
            Assert.IsTrue(Event.Match("domain.entity.*", "domain.entity.123"));
            Assert.IsTrue(Event.Match("domain.*.123", "domain.entity.123"));
            Assert.IsTrue(Event.Match("domain.entity.*", "domain.entity.123.456"));
        }

        [Test]
        public void CreateSubjectFromTemplate()
        {
            Assert.AreEqual("domain.entity", Event.CreateSubjectFromTemplate("domain.entity"));
            Assert.AreEqual("domain.entity", Event.CreateSubjectFromTemplate("domain.entity", new KeyValuePair<string, object>("id", 123)));
            Assert.AreEqual("domain.entity.123", Event.CreateSubjectFromTemplate("domain.entity.{id}", new KeyValuePair<string, object>("id", 123)));
            Assert.AreEqual("domain.entity.123,xyz", Event.CreateSubjectFromTemplate("domain.entity.{id},{code}", new KeyValuePair<string, object>("id", 123), new KeyValuePair<string, object>("code", "xyz")));

            Assert.Throws<ArgumentException>(() => Event.CreateSubjectFromTemplate("domain.entity.{xx}", new KeyValuePair<string, object>("id", 123)));
            Assert.Throws<ArgumentException>(() => Event.CreateSubjectFromTemplate("domain.{entity.123", new KeyValuePair<string, object>("id", 123)));
            Assert.Throws<ArgumentException>(() => Event.CreateSubjectFromTemplate("domain.entity.123}", new KeyValuePair<string, object>("id", 123)));
        }

        [Test]
        public void PublishAsync_NoValueWithTemplate()
        {
            ExecutionContext.Reset(false);
            var start = DateTime.Now;

            EventData ed = null;
            Event.RegisterReset();
            Event.Register((x) => { ed = x[0]; return Task.CompletedTask; });

            Event.PublishAsync("domain.entity.{id}", "create", new KeyValuePair<string, object>("id", 123)).Wait();
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(123, ed.Key);
            Assert.IsFalse(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
        }

        [Test]
        public void PublishAsync_ValueWithTemplate()
        {
            ExecutionContext.Reset(false);
            var start = DateTime.Now;

            EventData<string> ed = null;
            Event.RegisterReset();
            Event.Register((x) => { ed = (EventData<string>)x[0]; return Task.CompletedTask; });

            Event.PublishAsync<string>("TESTER", "domain.entity.{id}", "create", new KeyValuePair<string, object>("id", 123)).Wait();
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(123, ed.Key);
            Assert.IsTrue(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
            Assert.AreEqual("TESTER", ed.Value);
        }

        [Test]
        public void PublishAsync_NoValueWithSubject()
        {
            ExecutionContext.Reset(false);
            var start = DateTime.Now;

            EventData ed = null;
            Event.RegisterReset();
            Event.Register((x) => { ed = x[0]; return Task.CompletedTask; });

            Event.PublishAsync("domain.entity.123", "create").Wait();
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(null, ed.Key);
            Assert.IsFalse(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
        }

        [Test]
        public void PublishAsync_ValueWithSubject()
        {
            ExecutionContext.Reset(false);
            var start = DateTime.Now;

            EventData<string> ed = null;
            Event.RegisterReset();
            Event.Register((x) => { ed = (EventData<string>)x[0]; return Task.CompletedTask; });

            Event.PublishAsync<string>("TESTER", "domain.entity.123", "create").Wait();
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(null, ed.Key);
            Assert.IsTrue(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
            Assert.AreEqual("TESTER", ed.Value);
        }

        [Test]
        public void PublishAsync_RegisterMulti_Sync()
        {
            var reg1 = false;
            var reg2 = false;

            Event.RegisterReset();
            Event.PublishSynchronously = true;
            Event.Register((x) => { reg1 = true; return Task.CompletedTask; });
            Event.Register((x) => { reg2 = true; return Task.CompletedTask; });

            Event.PublishAsync<string>("TESTER", "domain.entity.123", "create").Wait();
            Assert.IsTrue(reg1);
            Assert.IsTrue(reg2);
        }

        [Test]
        public void PublishAsync_RegisterMulti_Async()
        {
            var reg1 = false;
            var reg2 = false;

            Event.RegisterReset();
            Event.PublishSynchronously = false;
            Event.Register((x) => { reg1 = true; return Task.CompletedTask; });
            Event.Register((x) => { reg2 = true; return Task.CompletedTask; });

            Event.PublishAsync<string>("TESTER", "domain.entity.123", "create").Wait();
            Assert.IsTrue(reg1);
            Assert.IsTrue(reg2);
        }

        [Test]
        public void PublishAsync_RegisterMulti_Sync_WithDelay()
        {
            var reg1 = false;
            var reg2 = false;

            Event.RegisterReset();
            Event.PublishSynchronously = true;
            Event.Register((x) => { reg1 = true; return Task.Delay(100); });
            Event.Register((x) => { reg2 = true; return Task.Delay(100); });

            var sw = Stopwatch.StartNew();
            Event.PublishAsync<string>("TESTER", "domain.entity.123", "create").Wait();
            sw.Stop();
            Assert.IsTrue(reg1);
            Assert.IsTrue(reg2);
            Assert.IsTrue(sw.ElapsedMilliseconds >= 200);
        }

        [Test]
        public void PublishAsync_RegisterMulti_Async_WithDelay()
        {
            var reg1 = false;
            var reg2 = false;

            Event.RegisterReset();
            Event.PublishSynchronously = false;
            Event.Register((x) => { reg1 = true; return Task.Delay(100); });
            Event.Register((x) => { reg2 = true; return Task.Delay(100); });

            var sw = Stopwatch.StartNew();
            Event.PublishAsync<string>("TESTER", "domain.entity.123", "create").Wait();
            sw.Stop();
            Assert.IsTrue(reg1);
            Assert.IsTrue(reg2);
            Assert.IsTrue(sw.ElapsedMilliseconds < 200);
        }

        [Test]
        public void EventData_Value()
        {
            var ed1 = new EventData<int> { Value = 123 };
            Assert.IsTrue(ed1.HasValue);
            Assert.AreEqual(123, ed1.Value);
            Assert.AreEqual(123, ed1.GetValue());
            Assert.IsNull(ed1.ETag);

            ed1.ResetValue();
            Assert.AreEqual(0, ed1.Value);
            Assert.AreEqual(0, ed1.GetValue());

            ed1.Value = 456;
            var ed2 = ed1 as EventData;
            ed2.ResetValue();
            Assert.AreEqual(0, ed1.Value);
            Assert.AreEqual(0, ed1.GetValue());
            Assert.IsTrue(ed1.HasValue);
            Assert.IsTrue(ed2.HasValue);

            ed2 = new EventData();
            ed2.ResetValue();
            Assert.IsFalse(ed2.HasValue);
            Assert.IsNull(ed2.GetValue());
        }

        public class TestData : IETag
        {
            public string Blah { get; set; }
            public string ETag { get; set; }
        }

        [Test]
        public void EventData_IETag()
        {
            var td = new TestData();
            var ed = new EventData<TestData> { Value = td };
            Assert.AreSame(td, ed.Value);
            Assert.IsNull(td.ETag);

            td = new TestData() { Blah = "B" };
            ed = new EventData<TestData> { Value = td };
            Assert.AreSame(td, ed.Value);
            Assert.IsNull(td.ETag);

            td = new TestData { Blah = "B", ETag = "E" };
            ed = new EventData<TestData> { Value = td };
            Assert.AreSame(td, ed.Value);
            Assert.AreEqual("E", td.ETag);

            ed.ETag = "X";
            Assert.AreEqual("X", ed.ETag);

            ed.ResetValue();
            Assert.IsNull(ed.Value);
            Assert.AreEqual("X", ed.ETag);
        }
    }
}
