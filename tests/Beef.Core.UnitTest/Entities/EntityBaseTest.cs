// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Entities
{
    [TestFixture]
    public class EntityBaseTest
    {
        private class TestA : EntityBase
        {
            private long _id;
            private string _text;
            private DateTime _now;
            private DateTime? _time;

            public long Id
            {
                get { return this._id; }
                set { SetValue<long>(ref this._id, value, false, false, "Id"); }
            }

            public string Text
            {
                get { return this._text; }
                set { SetValue(ref this._text, value, false, StringTrim.End, StringTransform.EmptyToNull, "Text"); }
            }

            public DateTime Now
            {
                get { return this._now; }
                set { SetValue(ref this._now, value, false, DateTimeTransform.DateOnly, "Now"); }
            }

            public DateTime? Time
            {
                get { return this._time; }
                set { SetValue(ref this._time, value, false, DateTimeTransform.None, "Time"); }
            }

            public override bool IsInitial => throw new NotImplementedException();

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public override void CleanUp()
            {
                base.CleanUp();
                this.Id = Cleaner.Clean(this.Id);
                this.Text = Cleaner.Clean(this.Text);
                this.Now = Cleaner.Clean(this.Now);
                this.Time = Cleaner.Clean(this.Time);
            }
        }

        [Test]
        public void Property_ConcurrentUpdating()
        {
            // EntityBase etc. is not designed to be thread-sage. Not generally supported.
            var a = new TestA();
            a.TrackChanges();

            var ts = new Task[100];

            for (int i = 0; i < ts.Length; i++)
                ts[i] = CreateValueUpdateTask(a);

            for (int i = 0; i < ts.Length; i++)
                ts[i].Start();

            Task.WaitAll(ts);

            Assert.IsNotNull(a.ChangeTracking);
            Assert.AreEqual(4, a.ChangeTracking.Count);
            Assert.AreEqual("Id", a.ChangeTracking[0]);
            Assert.AreEqual("Text", a.ChangeTracking[1]);
            Assert.AreEqual("Now", a.ChangeTracking[2]);
            Assert.AreEqual("Time", a.ChangeTracking[3]);
        }

        private Task CreateValueUpdateTask(TestA a)
        {
            return new Task(() =>
            {
                var now = DateTime.Now;
                a.Id = now.Ticks;
                a.Text = now.ToLongDateString();
                a.Now = now;
                a.Time = now;
                Cleaner.CleanUp(a);
            });
        }
    }
}
