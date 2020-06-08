using Beef.Entities;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.Entities
{
    [TestFixture, NonParallelizable]
    public class CleanerTest
    {
        [Test]
        public void DateTimeCleaning()
        {
            Cleaner.DefaultDateTimeTransform = DateTimeTransform.DateTimeLocal;
            var dt = DateTime.UtcNow;
            var dtc = Cleaner.Clean(dt);
            Assert.AreEqual(DateTimeKind.Local, dtc.Kind);

            Cleaner.DefaultDateTimeTransform = DateTimeTransform.DateTimeUtc;
            dtc = Cleaner.Clean(dt);
            Assert.AreEqual(DateTimeKind.Utc, dtc.Kind);

            Cleaner.DefaultDateTimeTransform = DateTimeTransform.DateTimeLocal;
        }

        [Test]
        public void NullableDateTimeCleaning()
        {
            Cleaner.DefaultDateTimeTransform = DateTimeTransform.DateTimeLocal;
            DateTime? dt = DateTime.UtcNow;
            DateTime? dtc = Cleaner.Clean(dt);
            Assert.AreEqual(DateTimeKind.Local, dtc.Value.Kind);

            Cleaner.DefaultDateTimeTransform = DateTimeTransform.DateTimeUtc;
            dtc = Cleaner.Clean(dt);
            Assert.AreEqual(DateTimeKind.Utc, dtc.Value.Kind);

            Cleaner.DefaultDateTimeTransform = DateTimeTransform.DateTimeLocal;

            dt = null;
            dtc = Cleaner.Clean(dt);
            Assert.IsNull(dtc);
        }

        [Test]
        public void StringTransformCleaning()
        {
            var s1 = "";
            var s2 = (string)null;
            var s3 = "ABC";

            Assert.IsNull(Cleaner.Clean(s1));
            Assert.IsNull(Cleaner.Clean(s2));
            Assert.AreEqual("ABC", Cleaner.Clean(s3));

            Cleaner.DefaultStringTransform = StringTransform.NullToEmpty;
            Assert.AreEqual("", Cleaner.Clean(s1));
            Assert.AreEqual("", Cleaner.Clean(s2));
            Assert.AreEqual("ABC", Cleaner.Clean(s3));

            Cleaner.DefaultStringTransform = StringTransform.EmptyToNull;
        }

        [Test]
        public void StringTrimCleaning()
        {
            var s = " ABC ";
            Assert.AreEqual(" ABC", Cleaner.Clean(s));

            Cleaner.DefaultStringTrim = StringTrim.Both;
            Assert.AreEqual("ABC", Cleaner.Clean(s));

            Cleaner.DefaultStringTrim = StringTrim.End;
        }
    }
}
