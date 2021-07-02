using Beef.Events;
using NUnit.Framework;
using System;

namespace Beef.Data.Database.UnitTest
{
    [TestFixture]
    public class DatabaseEventOutboxItemTest
    {
        [Test]
        public void EndToEndNoValue()
        {
            var ed = new EventData
            {
                EventId = Guid.NewGuid(),
                Subject = "xxx",
                Action = "yyy",
                Source = new Uri("/xxx", UriKind.Relative),
                CorrelationId = "zzz",
                TenantId = Guid.NewGuid()
            };

            var deo = new DatabaseEventOutboxItem(ed);
            Assert.IsNotNull(deo);
            Assert.AreEqual(ed.EventId, deo.EventId);
            Assert.AreEqual(ed.Subject, deo.Subject);
            Assert.AreEqual(ed.Action, deo.Action);
            Assert.AreEqual(ed.CorrelationId, deo.CorrelationId);
            Assert.AreEqual(ed.TenantId, deo.TenantId);
            Assert.IsNull(deo.ValueType);
            Assert.IsNotNull(deo.EventData);
            Assert.NotZero(deo.EventData.Length);

            var ed2 = deo.ToEventData();
            Assert.IsNotNull(ed2);
            Assert.AreEqual(ed.EventId, ed2.EventId);
            Assert.AreEqual(ed.Subject, ed2.Subject);
            Assert.AreEqual(ed.Action, ed2.Action);
            Assert.AreEqual(ed.CorrelationId, ed2.CorrelationId);
            Assert.AreEqual(ed.TenantId, ed2.TenantId);
            Assert.AreEqual(ed.Source, ed2.Source);
        }

        [Test]
        public void EndToEndWithValueInt()
        {
            var ed = new EventData<int>
            {
                EventId = Guid.NewGuid(),
                Subject = "xxx",
                Action = "yyy",
                Source = new Uri("/xxx", UriKind.Relative),
                CorrelationId = "zzz",
                TenantId = Guid.NewGuid(),
                Value = 88
            };

            var deo = new DatabaseEventOutboxItem(ed);
            Assert.IsNotNull(deo);
            Assert.AreEqual(ed.EventId, deo.EventId);
            Assert.AreEqual(ed.Subject, deo.Subject);
            Assert.AreEqual(ed.Action, deo.Action);
            Assert.AreEqual(ed.CorrelationId, deo.CorrelationId);
            Assert.AreEqual(ed.TenantId, deo.TenantId);
            Assert.IsNotNull(deo.ValueType);
            Assert.IsNotNull(deo.EventData);
            Assert.NotZero(deo.EventData.Length);

            var ed2 = deo.ToEventData();
            Assert.IsNotNull(ed2);
            Assert.AreEqual(ed.EventId, ed2.EventId);
            Assert.AreEqual(ed.Subject, ed2.Subject);
            Assert.AreEqual(ed.Action, ed2.Action);
            Assert.AreEqual(ed.CorrelationId, ed2.CorrelationId);
            Assert.AreEqual(ed.TenantId, ed2.TenantId);
            Assert.AreEqual(ed.Source, ed2.Source);

            Assert.IsTrue(ed.HasValue);
            var edv = (EventData<int>)ed2;
            Assert.AreEqual(88, edv.Value);
        }

        [Test]
        public void EndToEndWithValueClass()
        {
            var ed = new EventData<Person>
            {
                EventId = Guid.NewGuid(),
                Subject = "xxx",
                Action = "yyy",
                Source = new Uri("/xxx", UriKind.Relative),
                CorrelationId = "zzz",
                TenantId = Guid.NewGuid(),
                Value = new Person { FirstName = "Karen", LastName = "Smith" }
            };

            var deo = new DatabaseEventOutboxItem(ed);
            Assert.IsNotNull(deo);
            Assert.AreEqual(ed.EventId, deo.EventId);
            Assert.AreEqual(ed.Subject, deo.Subject);
            Assert.AreEqual(ed.Action, deo.Action);
            Assert.AreEqual(ed.CorrelationId, deo.CorrelationId);
            Assert.AreEqual(ed.TenantId, deo.TenantId);
            Assert.IsNotNull(deo.ValueType);
            Assert.IsNotNull(deo.EventData);
            Assert.NotZero(deo.EventData.Length);

            var ed2 = deo.ToEventData();
            Assert.IsNotNull(ed2);
            Assert.AreEqual(ed.EventId, ed2.EventId);
            Assert.AreEqual(ed.Subject, ed2.Subject);
            Assert.AreEqual(ed.Action, ed2.Action);
            Assert.AreEqual(ed.CorrelationId, ed2.CorrelationId);
            Assert.AreEqual(ed.TenantId, ed2.TenantId);
            Assert.AreEqual(ed.Source, ed2.Source);

            Assert.IsTrue(ed.HasValue);
            var edv = (EventData<Person>)ed2;
            Assert.IsNotNull(edv.Value);
            Assert.AreEqual(ed.Value.FirstName, edv.Value.FirstName);
            Assert.AreEqual(ed.Value.LastName, edv.Value.LastName);
        }

        [Test]
        public void EndToEndWithValueClassNull()
        {
            var ed = new EventData<Person>
            {
                EventId = Guid.NewGuid(),
                Subject = "xxx",
                Action = "yyy",
                Source = new Uri("/xxx", UriKind.Relative),
                CorrelationId = "zzz",
                TenantId = Guid.NewGuid()
            };

            var deo = new DatabaseEventOutboxItem(ed);
            Assert.IsNotNull(deo);
            Assert.AreEqual(ed.EventId, deo.EventId);
            Assert.AreEqual(ed.Subject, deo.Subject);
            Assert.AreEqual(ed.Action, deo.Action);
            Assert.AreEqual(ed.CorrelationId, deo.CorrelationId);
            Assert.AreEqual(ed.TenantId, deo.TenantId);
            Assert.IsNotNull(deo.ValueType);
            Assert.IsNotNull(deo.EventData);
            Assert.NotZero(deo.EventData.Length);

            var ed2 = deo.ToEventData();
            Assert.IsNotNull(ed2);
            Assert.AreEqual(ed.EventId, ed2.EventId);
            Assert.AreEqual(ed.Subject, ed2.Subject);
            Assert.AreEqual(ed.Action, ed2.Action);
            Assert.AreEqual(ed.CorrelationId, ed2.CorrelationId);
            Assert.AreEqual(ed.TenantId, ed2.TenantId);
            Assert.AreEqual(ed.Source, ed2.Source);

            Assert.IsTrue(ed.HasValue);
            var edv = (EventData<Person>)ed2;
            Assert.IsNull(edv.Value);
        }

        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}