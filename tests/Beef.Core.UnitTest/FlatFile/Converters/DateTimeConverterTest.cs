// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile.Converters;
using NUnit.Framework;
using System;
using System.Globalization;

namespace Beef.Core.UnitTest.FlatFile.Converters
{
    [TestFixture]
    public class DateTimeConverterTest
    {
        [Test]
        public void TryFormat()
        {
            var dc = new DateTimeConverter { FormatString = "d", FormatProvider = CultureInfo.CreateSpecificCulture("en-NZ").DateTimeFormat };
            string str = null; 
            Assert.IsTrue(dc.TryFormat(new DateTime(2010, 01, 05), out str));
            Assert.AreEqual("5/01/2010", str);

            dc = new DateTimeConverter { FormatString = "d", FormatProvider = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat };
            str = null;
            Assert.IsTrue(dc.TryFormat(new DateTime(2010, 01, 05), out str));
            Assert.AreEqual("1/5/2010", str);
        }

        [Test]
        public void TryFormat_ITextValueConverter()
        {
            var dcx = new DateTimeConverter { FormatString = "d", FormatProvider = CultureInfo.CreateSpecificCulture("en-NZ").DateTimeFormat };
            var dc = (ITextValueConverter)dcx;
            string str = null;
            Assert.IsTrue(dc.TryFormat(new DateTime(2010, 01, 05), out str));
            Assert.AreEqual("5/01/2010", str);

            dcx = new DateTimeConverter { FormatString = "d", FormatProvider = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat };
            dc = (ITextValueConverter)dcx;
            str = null;
            Assert.IsTrue(dc.TryFormat(new DateTime(2010, 01, 05), out str));
            Assert.AreEqual("1/5/2010", str);
        }

        [Test]
        public void TryParse()
        {
            var dc = new DateTimeConverter { FormatProvider = CultureInfo.CreateSpecificCulture("en-AU").DateTimeFormat };
            DateTime result = DateTime.MinValue;
            Assert.IsTrue(dc.TryParse("5/01/2010", out result));
            Assert.AreEqual(new DateTime(2010, 01, 05), result);

            Assert.IsTrue(dc.TryParse("1/5/2010", out result));
            Assert.AreEqual(new DateTime(2010, 05, 01), result);

            dc = new DateTimeConverter { FormatProvider = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat };
            result = DateTime.MinValue;
            Assert.IsTrue(dc.TryParse("1/5/2010", out result));
            Assert.AreEqual(new DateTime(2010, 01, 05), result);

            Assert.IsFalse(dc.TryParse("28/5/2010", out result));
            Assert.AreEqual(DateTime.MinValue, result);
        }

        [Test]
        public void TryParse_ITextValueConverter()
        {
            var dcx = new DateTimeConverter { FormatProvider = CultureInfo.CreateSpecificCulture("en-AU").DateTimeFormat };
            var dc = (ITextValueConverter)dcx;
            object result = DateTime.MinValue;
            Assert.IsTrue(dc.TryParse("5/01/2010", out result));
            Assert.AreEqual(new DateTime(2010, 01, 05), result);

            Assert.IsTrue(dc.TryParse("1/5/2010", out result));
            Assert.AreEqual(new DateTime(2010, 05, 01), result);

            dcx = new DateTimeConverter { FormatProvider = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat };
            dc = (ITextValueConverter)dcx;
            result = DateTime.MinValue;
            Assert.IsTrue(dc.TryParse("1/5/2010", out result));
            Assert.AreEqual(new DateTime(2010, 01, 05), result);

            Assert.IsFalse(dc.TryParse("28/5/2010", out result));
            Assert.AreEqual(DateTime.MinValue, result);
        }
    }
}
