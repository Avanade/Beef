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
            Assert.IsTrue(pt.IsChanged); // Collections always cloned - therefore changed.
            Assert.AreNotSame(pf.History, pt.History);
        }
    }
}
