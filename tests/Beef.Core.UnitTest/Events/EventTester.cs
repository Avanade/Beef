// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Events
{
    [TestFixture, NonParallelizable]
    public class EventTester
    {
        [Test]
        public void Match()
        {
            Assert.IsFalse(EventSubjectMatcher.Match("*", ".", "domain.entity.123", "domain.entity.000"));
            Assert.IsFalse(EventSubjectMatcher.Match("*", ".", "domain.ytitne.123", "domain.entity.123"));
            Assert.IsFalse(EventSubjectMatcher.Match("*", ".", "domain.ytitne.*", "domain.entity.123"));
            Assert.IsFalse(EventSubjectMatcher.Match("*", ".", "domain.entity.123.*", "domain.entity.123"));

            Assert.IsTrue(EventSubjectMatcher.Match("*", ".", "domain.entity.123", "domain.entity.123"));
            Assert.IsTrue(EventSubjectMatcher.Match("*", ".", "domain.entity.*", "domain.entity.123"));
            Assert.IsTrue(EventSubjectMatcher.Match("*", ".", "domain.*.123", "domain.entity.123"));
            Assert.IsTrue(EventSubjectMatcher.Match("*", ".", "domain.entity.*", "domain.entity.123.456"));
        }

        private class TestEventPublisher : EventPublisherBase
        {
            public List<EventData> Events { get; } = new List<EventData>();

            protected override Task PublishEventsAsync(params EventData[] events) { Events.AddRange(events); return Task.CompletedTask; }
        }

        [Test]
        public async Task PublishAsync_NoValueWithTemplate()
        {
            ExecutionContext.Reset(false);
            var start = Cleaner.Clean(DateTime.Now);

            var tep = new TestEventPublisher();

            await tep.PublishAsync("domain.entity.123", "create", 123).ConfigureAwait(false);
            Assert.AreEqual(1, tep.Events.Count);
            var ed = tep.Events[0];
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(123, ed.Key);
            Assert.IsFalse(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
        }

        private class Entity : IIntIdentifier
        {
            public int Id { get; set; }
        }

        [Test]
        public async Task PublishValueAsync_ValueIIdentifier()
        {
            ExecutionContext.Reset(false);
            var start = Cleaner.Clean(DateTime.Now);

            var tep = new TestEventPublisher();
            var v = new Entity { Id = 123 };

            await tep.PublishValueAsync(v, "domain.entity.123", "create").ConfigureAwait(false);
            Assert.AreEqual(1, tep.Events.Count);
            var ed = (EventData<Entity>)tep.Events[0];
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(123, ed.Key);
            Assert.IsTrue(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
            Assert.AreEqual(v, ed.Value);
        }

        private class Entity2 : IUniqueKey
        {
            public int A { get; set; }
            public string B { get; set; }

            public bool HasUniqueKey => true;

            public UniqueKey UniqueKey => new UniqueKey(A, B);

            public string[] UniqueKeyProperties => throw new NotImplementedException();
        }

        [Test]
        public async Task PublishValueAsync_ValueIUniqueKey()
        {
            ExecutionContext.Reset(false);
            var start = Cleaner.Clean(DateTime.Now);

            var tep = new TestEventPublisher();
            var v = new Entity2 { A = 123, B = "Abc" };

            await tep.PublishValueAsync(v, "domain.entity.123", "create").ConfigureAwait(false);
            Assert.AreEqual(1, tep.Events.Count);
            var ed = (EventData<Entity2>)tep.Events[0];
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(new object[] { 123, "Abc" }, ed.Key);
            Assert.IsTrue(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
            Assert.AreEqual(v, ed.Value);
        }

        [Test]
        public async Task PublishAsync_SubjectAndAction()
        {
            ExecutionContext.Reset(false);
            var start = Cleaner.Clean(DateTime.Now);

            var tep = new TestEventPublisher();

            await tep.PublishAsync("domain.entity.123", "create").ConfigureAwait(false);
            Assert.AreEqual(1, tep.Events.Count);
            var ed = tep.Events[0];
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(null, ed.Key);
            Assert.IsFalse(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
        }

        [Test]
        public async Task PublishValueAsync_WithKey()
        {
            ExecutionContext.Reset(false);
            var start = Cleaner.Clean(DateTime.Now);

            var tep = new TestEventPublisher();

            await tep.PublishValueAsync("TESTER", "domain.entity.123", "create", 123).ConfigureAwait(false);
            Assert.AreEqual(1, tep.Events.Count);
            var ed = (EventData<string>)tep.Events[0];
            Assert.IsNotNull(ed);
            Assert.AreEqual("domain.entity.123", ed.Subject);
            Assert.AreEqual("create", ed.Action);
            Assert.AreEqual(123, ed.Key);
            Assert.IsTrue(ed.HasValue);
            Assert.IsTrue(ed.Timestamp >= start);
            Assert.AreEqual("TESTER", ed.Value);
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