using Beef.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Events.UnitTest
{
    [TestFixture]
    public class EventDataMapperTest
    {
        private readonly IEventPublisher _ep = new NullEventPublisher();

        [Test]
        public void SubjectOnly()
        {
            var ed = EventData.CreateEvent(_ep, "Subject");
            var eh = ed.ToEventHubsEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", eh.Properties[EventDataMapper.SubjectPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.ActionPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.TenantIdPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.KeyPropertyName]);

            ed = eh.ToBeefEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", ed.Subject);
            Assert.AreEqual(null, ed.Action);
            Assert.AreEqual(null, ed.TenantId);
            Assert.AreEqual(null, ed.Key);
        }

        [Test]
        public void SubjectAndAction()
        {
            var ed = EventData.CreateEvent(_ep, "Subject", "Action");
            var eh = ed.ToEventHubsEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", eh.Properties[EventDataMapper.SubjectPropertyName]);
            Assert.AreEqual("Action", eh.Properties[EventDataMapper.ActionPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.TenantIdPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.KeyPropertyName]);

            ed = eh.ToBeefEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", ed.Subject);
            Assert.AreEqual("Action", ed.Action);
            Assert.AreEqual(null, ed.TenantId);
            Assert.AreEqual(null, ed.Key);
        }

        [Test]
        public void SubjectActionAndKey()
        {
            var id = Guid.NewGuid();

            var ed = EventData.CreateEvent(_ep, "Subject", "Action", id);
            var eh = ed.ToEventHubsEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", eh.Properties[EventDataMapper.SubjectPropertyName]);
            Assert.AreEqual("Action", eh.Properties[EventDataMapper.ActionPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.TenantIdPropertyName]);
            Assert.AreEqual(id, eh.Properties[EventDataMapper.KeyPropertyName]);

            ed = eh.ToBeefEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", ed.Subject);
            Assert.AreEqual("Action", ed.Action);
            Assert.AreEqual(null, ed.TenantId);
            Assert.AreEqual(id, ed.Key);
        }

        [Test]
        public void SubjectActionAndArrayKey()
        {
            var id = Guid.NewGuid();
            var no = 123;

            var ed = EventData.CreateEvent(_ep, "Subject", "Action", id, no);
            var eh = ed.ToEventHubsEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", eh.Properties[EventDataMapper.SubjectPropertyName]);
            Assert.AreEqual("Action", eh.Properties[EventDataMapper.ActionPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.TenantIdPropertyName]);
            Assert.AreEqual(id, ((object[])eh.Properties[EventDataMapper.KeyPropertyName])[0]);
            Assert.AreEqual(no, ((object[])eh.Properties[EventDataMapper.KeyPropertyName])[1]);

            ed = eh.ToBeefEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", ed.Subject);
            Assert.AreEqual("Action", ed.Action);
            Assert.AreEqual(null, ed.TenantId);
            Assert.AreEqual(id, ((object[])ed.Key)[0]);
            Assert.AreEqual(no, ((object[])ed.Key)[1]);
        }

        public class Person : IGuidIdentifier
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void SubjectActionKeyAndValue()
        {
            var p = new Person { Id = Guid.NewGuid(), Name = "Caleb" };

            var ed = EventData.CreateValueEvent(_ep, p, "Subject", "Action");
            var eh = ed.ToEventHubsEventData();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", eh.Properties[EventDataMapper.SubjectPropertyName]);
            Assert.AreEqual("Action", eh.Properties[EventDataMapper.ActionPropertyName]);
            Assert.AreEqual(null, eh.Properties[EventDataMapper.TenantIdPropertyName]);
            Assert.AreEqual(p.Id, eh.Properties[EventDataMapper.KeyPropertyName]);

            ed = eh.ToBeefEventData<Person>();
            Assert.IsNotNull(eh);
            Assert.AreEqual("Subject", ed.Subject);
            Assert.AreEqual("Action", ed.Action);
            Assert.AreEqual(null, ed.TenantId);
            Assert.AreEqual(p.Id, ed.Key);
            Assert.IsNotNull(ed.Value);
            Assert.AreEqual(p.Id, ed.Value.Id);
            Assert.AreEqual(p.Name, ed.Value.Name);
        }
    }
}