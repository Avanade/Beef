// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile.Converters;
using NUnit.Framework;
using System;
using System.Globalization;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile.Converters
{
    [TestFixture]
    public class NumberConverterTest
    {
        [Test]
        public void Ctor_NotNumericTypeException()
        {
            ExpectException.Throws<InvalidOperationException>("Type T must be a numeric Type.", () => new NumberConverter<DateTime>());
        }

        [Test]
        public void TryFormat()
        {
            var nc = new NumberConverter<Decimal> { FormatString = "C", FormatProvider = NumberFormatInfo.CurrentInfo };
            string str = null;
            Assert.IsTrue(nc.TryFormat(1234.56m, out str));
            Assert.AreEqual("$1,234.56", str);

            nc = new NumberConverter<Decimal> { FormatString = "G", FormatProvider = CultureInfo.CreateSpecificCulture("fr-FR") };
            str = null;
            Assert.IsTrue(nc.TryFormat(1234.56m, out str));
            Assert.AreEqual("1234,56", str);
        }

        [Test]
        public void TryFormat_ITextValueConverter()
        {
            var ncx = new NumberConverter<Decimal> { FormatString = "C", FormatProvider = NumberFormatInfo.CurrentInfo };
            var nc = (ITextValueConverter)ncx;
            string str = null;
            Assert.IsTrue(nc.TryFormat(1234.56m, out str));
            Assert.AreEqual("$1,234.56", str);

            ncx = new NumberConverter<Decimal> { FormatString = "G", FormatProvider = CultureInfo.CreateSpecificCulture("fr-FR") };
            nc = (ITextValueConverter)ncx;
            str = null;
            Assert.IsTrue(nc.TryFormat(1234.56m, out str));
            Assert.AreEqual("1234,56", str);
        }

        [Test]
        public void TryParse()
        {
            var nc = new NumberConverter<Decimal> { NumberStyles = NumberStyles.Currency };
            decimal result = 0m;
            Assert.IsTrue(nc.TryParse("$1,234.56", out result));
            Assert.AreEqual(1234.56m, result);

            nc = new NumberConverter<Decimal>();
            result = 0m;
            Assert.IsFalse(nc.TryParse("$1,234.56", out result));
            Assert.AreEqual(0m, result);
        }

        [Test]
        public void TryParse_ITextValueConverter()
        {
            var ncx = new NumberConverter<Decimal> { NumberStyles = NumberStyles.Currency };
            var nc = (ITextValueConverter)ncx;
            object result = null;
            Assert.IsTrue(nc.TryParse("$1,234.56", out result));
            Assert.AreEqual(1234.56m, (decimal)result);

            ncx = new NumberConverter<Decimal>();
            nc = (ITextValueConverter)ncx;
            result = null;
            Assert.IsFalse(nc.TryParse("$1,234.56", out result));
            Assert.AreEqual(0m, (decimal)result);
        }
    }
}
