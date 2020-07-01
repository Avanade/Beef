using Beef.Demo.Business.Data;
using Beef.Demo.Common.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Demo.Test
{
    [TestFixture]
    public class EntityTest
    {
        [Test]
        public void ChangeTracking_EntitySubPropertyTracking()
        {
            var p = new Person
            {
                Address = new Address()
            };

            p.AcceptChanges();
            p.TrackChanges();

            p.Address.City = "Bardon";

            Assert.IsTrue(p.IsChanged);
            Assert.AreEqual(new System.Collections.Specialized.StringCollection { "Address" }, p.ChangeTracking);
            Assert.IsTrue(p.Address.IsChanged);
            Assert.AreEqual(new System.Collections.Specialized.StringCollection { "City" }, p.Address.ChangeTracking);
        }

        [Test]
        public void ChangeTracking_EntityCollectionTracking()
        {
            var pc = new PersonCollection();
            pc.TrackChanges();
            Assert.IsTrue(pc.IsChangeTracking);

            var p = new Person { FirstName = "Jenny" };
            Assert.IsFalse(p.IsChangeTracking);

            // Adding item will not pick up changes made prior to TrackChanges.
            pc.Add(p);
            Assert.IsTrue(pc.IsChanged);
            Assert.IsTrue(p.IsChangeTracking);
            Assert.IsTrue(p.IsChanged);
            Assert.AreEqual(new System.Collections.Specialized.StringCollection(), p.ChangeTracking);

            // Accept changes so no longer tracking anything.
            pc.AcceptChanges();
            Assert.IsFalse(pc.IsChangeTracking);
            Assert.IsFalse(pc.IsChanged);
            Assert.IsFalse(p.IsChangeTracking);
            Assert.IsFalse(p.IsChanged);

            // Track changes again updating only the person name - changes should bubble.
            pc.TrackChanges();
            p.FirstName = "Jennifer";
            Assert.IsTrue(pc.IsChanged);
            Assert.IsTrue(p.IsChanged);
            Assert.AreEqual(new System.Collections.Specialized.StringCollection { "FirstName" }, p.ChangeTracking);

            // Remove item from collection, changes should no longer bubble.
            pc.AcceptChanges();
            pc.Remove(p);
            pc.AcceptChanges();
            p.FirstName = "Jen";
            Assert.IsFalse(pc.IsChanged);
            Assert.IsTrue(p.IsChanged);
            Assert.IsFalse(p.IsChangeTracking);
        }

        [Test]
        public void ChangeTracking_EntitySubCollectionTracking()
        {
            var wh = new WorkHistory { Name = "Avanade" };
            var pd = new PersonDetail { History = new WorkHistoryCollection { wh } };
            pd.AcceptChanges();
            Assert.IsFalse(pd.IsChanged);
            Assert.IsFalse(pd.History.IsChanged);
            Assert.IsFalse(wh.IsChanged);

            pd.TrackChanges();
            wh.Name = "Accenture";
            Assert.IsTrue(wh.IsChanged);
            Assert.AreEqual(new System.Collections.Specialized.StringCollection { "Name" }, wh.ChangeTracking);
            Assert.IsTrue(pd.History.IsChanged);
            Assert.IsNull(pd.History.ChangeTracking); // Collections always return null.
            Assert.IsTrue(pd.IsChanged);
            Assert.AreEqual(new System.Collections.Specialized.StringCollection { "History" }, pd.ChangeTracking);
        }

        [Test]
        public void CopyFrom_SubEntityNull()
        {
            var pf = new PersonDetail();
            var pt = new PersonDetail();

            pt.CopyFrom(pf);
            Assert.IsFalse(pt.IsChanged);
            Assert.IsNull(pf.Address);
            Assert.IsNull(pt.Address);
        }

        [Test]
        public void CopyFrom_SubEntityCopy()
        {
            var pf = new PersonDetail { Address = new Address { City = "Bardon" } };
            var pt = new PersonDetail { Address = new Address { City = "Bardon" } };
            pf.AcceptChanges();
            pt.AcceptChanges();
            pt.CopyFrom(pf);
            Assert.IsFalse(pt.IsChanged);
            Assert.AreNotSame(pf.Address, pt.Address);

            // Should result in a copyfrom.
            pf.Address = new Address { City = "Ashgrove" };
            pt.CopyFrom(pf);
            Assert.IsTrue(pt.IsChanged);
            Assert.AreNotSame(pf.Address, pt.Address);
            Assert.AreEqual("Ashgrove", pt.Address.City);

            // Should result in a clone.
            pt.Address = null;
            pf.AcceptChanges();
            pt.AcceptChanges();
            pt.CopyFrom(pf);
            Assert.IsTrue(pt.IsChanged);
            Assert.AreNotSame(pf.Address, pt.Address);
            Assert.AreEqual("Ashgrove", pt.Address.City);
        }

        [Test]
        public void CopyFrom_SubEntityCollClone()
        {
            var pf = new PersonDetail { History = new WorkHistoryCollection { new WorkHistory { Name = "Blah" } } };
            var pt = new PersonDetail { History = new WorkHistoryCollection { new WorkHistory { Name = "Blah" } } };
            pf.AcceptChanges();
            pt.AcceptChanges();
            pt.CopyFrom(pf);
            Assert.IsFalse(pt.IsChanged); // Although copied, data is the same - not changed.
            Assert.AreNotSame(pf.History, pt.History);

            pf.History[0].Name = "Blah, blah";
            pf.AcceptChanges();

            pt.CopyFrom(pf);
            Assert.IsTrue(pt.IsChanged); // Copied, and data was changed.
            Assert.AreNotSame(pf.History, pt.History);
            Assert.AreEqual(pf.History, pt.History);
        }

        [Test]
        public void Equality()
        {
            var g = Guid.NewGuid();
            var p1 = new PersonDetail { Id = g, FirstName = "Barry" };
            var p2 = p1;

            var p3 = new PersonDetail { Id = g, FirstName = "Barry" };
            var p4 = new PersonDetail { Id = g, FirstName = "Karen" };

            object o = null;
            Address a = new Address();
            Person p5 = null;

            // Null 
            Assert.IsFalse(p1.Equals(o));
            Assert.IsFalse(p1.Equals((PersonDetail)o));
            Assert.IsFalse(p1.Equals(p5));
            Assert.IsFalse(p1 == null);
            Assert.IsTrue(p1 != null);

            // Same instance
            Assert.IsTrue(p1.Equals(p2));
            Assert.IsTrue(p1.Equals((object)p2));
            Assert.IsTrue(p1 == p2);
            Assert.IsFalse(p1 != p2);

            // Same values
            Assert.IsTrue(p1.Equals(p3));
            Assert.IsTrue(p1.Equals((object)p3));
            Assert.IsTrue(p1 == p3);
            Assert.IsFalse(p1 != p3);

            // Not same
            Assert.IsFalse(p1.Equals(a));
            Assert.IsFalse(p1.Equals(p4));
            Assert.IsFalse(p1 == p4);
            Assert.IsTrue(p1 != p4);

            // Check into sub-entities.
            p1.Address = new Address { Street = "X" };
            p3.Address = new Address { Street = "X" };
            Assert.IsTrue(p1.Equals(p3));

            // Change the address to no longer match.
            p3.Address.Street += "X";
            Assert.IsFalse(p1.Equals(p3));

            // Change the address back again.
            p3.Address.Street = "X";
            Assert.IsTrue(p1.Equals(p3));

            // Compare the collections.
            p1.History = new WorkHistoryCollection { new WorkHistory { Name = "Q", StartDate = new DateTime(2000, 01, 01) } };
            p3.History = new WorkHistoryCollection { new WorkHistory { Name = "Q", StartDate = new DateTime(2000, 01, 01) } };
            Assert.IsTrue(p1.Equals(p3));

            // Change the collections to not match.
            p3.History[0].StartDate = new DateTime(2001, 01, 01);
            Assert.IsFalse(p1.Equals(p3));

            // Change the collections back again.
            p3.History[0].StartDate = new DateTime(2000, 01, 01);
            Assert.IsTrue(p1.Equals(p3));

            // Change the collections to not match.
            p3.History.Add(new WorkHistory { Name = "W", StartDate = new DateTime(2010, 01, 01) });
            Assert.IsFalse(p1.Equals(p3));

            // Make the collections match again.
            p1.History.Add(new WorkHistory { Name = "W", StartDate = new DateTime(2010, 01, 01) });
            Assert.IsTrue(p1.Equals(p3));
        }

        [Test]
        public void HashCode()
        {
            var g = Guid.NewGuid();
            var p1 = new PersonDetail { Id = g, FirstName = "Barry" };
            var p2 = p1;

            var p3 = new PersonDetail { Id = g, FirstName = "Barry" };
            var p4 = new PersonDetail { Id = g, FirstName = "Karen" };

            Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
            Assert.AreEqual(p1.GetHashCode(), p3.GetHashCode());
            Assert.AreNotEqual(p1.GetHashCode(), p4.GetHashCode());

            // Check into sub-entities.
            p1.Address = new Address { Street = "X" };
            p3.Address = new Address { Street = "X" };
            Assert.AreEqual(p1.GetHashCode(), p3.GetHashCode());

            // Change the address to no longer match.
            p3.Address.Street += "X";
            Assert.AreNotEqual(p1.GetHashCode(), p3.GetHashCode());

            // Change the address back again.
            p3.Address.Street = "X";
            Assert.AreEqual(p1.GetHashCode(), p3.GetHashCode());

            // Compare the collections.
            p1.History = new WorkHistoryCollection { new WorkHistory { Name = "Q", StartDate = new DateTime(2000, 01, 01) } };
            p3.History = new WorkHistoryCollection { new WorkHistory { Name = "Q", StartDate = new DateTime(2000, 01, 01) } };
            Assert.AreEqual(p1.GetHashCode(), p3.GetHashCode());

            // Change the collections to not match.
            p3.History[0].StartDate = new DateTime(2001, 01, 01);
            Assert.AreNotEqual(p1.GetHashCode(), p3.GetHashCode());

            // Change the collections back again.
            p3.History[0].StartDate = new DateTime(2000, 01, 01);
            Assert.AreEqual(p1.GetHashCode(), p3.GetHashCode());

            // Change the collections to not match.
            p3.History.Add(new WorkHistory { Name = "W", StartDate = new DateTime(2010, 01, 01) });
            Assert.AreNotEqual(p1.GetHashCode(), p3.GetHashCode());

            // Make the collections match again.
            p1.History.Add(new WorkHistory { Name = "W", StartDate = new DateTime(2010, 01, 01) });
            Assert.AreEqual(p1.GetHashCode(), p3.GetHashCode());
        }
    }
}