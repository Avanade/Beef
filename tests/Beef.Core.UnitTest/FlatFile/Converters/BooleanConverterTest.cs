// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using Beef.FlatFile.Converters;
using Beef.Core.UnitTest.FlatFile.Entities;
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile.Converters
{
    [TestFixture]
    public class BooleanConverterTest
    {
        [Test]
        public void Prop_StringComparer()
        {
            var bc = new BooleanConverter();
            Assert.AreEqual(StringComparer.Ordinal, bc.StringComparer);

            bc.StringComparer = StringComparer.InvariantCulture;
            Assert.AreEqual(StringComparer.InvariantCulture, bc.StringComparer);
        }

        [Test]
        public void TryFormat()
        {
            var bc = new BooleanConverter();
            Assert.IsTrue(bc.TryFormat(true, out string result));
            Assert.AreEqual("Y", result);

            Assert.IsTrue(bc.TryFormat(false, out result));
            Assert.AreEqual("N", result);
        }

        [Test]
        public void TryFormat_ITextValueConverter()
        {
            var bcx = new BooleanConverter();
            var bc = (ITextValueConverter)bcx;
            Assert.IsTrue(bc.TryFormat(true, out string result));
            Assert.AreEqual("Y", result);

            Assert.IsTrue(bc.TryFormat(false, out result));
            Assert.AreEqual("N", result);
        }

        [Test]
        public void TryParse()
        {
            var bc = new BooleanConverter();
            Assert.IsFalse(bc.TryParse(null, out bool result));
            Assert.IsFalse(result);

            Assert.IsTrue(bc.TryParse(string.Empty, out result));
            Assert.IsFalse(result);

            Assert.IsTrue(bc.TryParse("N", out result));
            Assert.IsFalse(result);

            Assert.IsTrue(bc.TryParse("False", out result));
            Assert.IsFalse(result);

            Assert.IsTrue(bc.TryParse("Y", out result));
            Assert.IsTrue(result);

            Assert.IsTrue(bc.TryParse("True", out result));
            Assert.IsTrue(result);

            Assert.IsFalse(bc.TryParse("Z", out result));
            Assert.IsFalse(result);
        }

        [Test]
        public void TryParse_ITextValueConverter()
        {
            var bcx = new BooleanConverter();
            var bc = (ITextValueConverter)bcx;
            Assert.IsFalse(bc.TryParse(null, out object result));
            Assert.IsFalse((bool)result);

            Assert.IsTrue(bc.TryParse(string.Empty, out result));
            Assert.IsFalse((bool)result);

            Assert.IsTrue(bc.TryParse("N", out result));
            Assert.IsFalse((bool)result);

            Assert.IsTrue(bc.TryParse("False", out result));
            Assert.IsFalse((bool)result);

            Assert.IsTrue(bc.TryParse("Y", out result));
            Assert.IsTrue((bool)result);

            Assert.IsTrue(bc.TryParse("True", out result));
            Assert.IsTrue((bool)result);

            Assert.IsFalse(bc.TryParse("Z", out result));
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void FileReader_Read()
        {
            using (var sr = new StringReader("Y,1"))
            {
                var ff = new DelimitedFileFormat<BoolConverterData>();
                ff.Converters.Add(new BooleanConverter());
                ff.Converters.Add("Bool", new BooleanConverter());
                var fr = new FileReader<BoolConverterData>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);

                var val = (BoolConverterData)result.Records[0].Value;
                Assert.IsTrue(val.BoolA);
                Assert.IsTrue(val.BoolB);
            }
        }

        [Test]
        public void FileReader_Read_ConverterNotFoundException()
        {
            ExpectException.Throws<InvalidOperationException>("FileColumnAttribute has TextValueConverterKey of 'Bool' is not found within the FileFormat.Converters.", () =>
            {
                using (var sr = new StringReader("Y,1"))
                {
                    var ff = new DelimitedFileFormat<BoolConverterData>();
                    ff.Converters.Add(new BooleanConverter());
                    var fr = new FileReader<BoolConverterData>(sr, ff);

                    var result = fr.Read();
                }
            });
        }

        [Test]
        public void FileWriter_Write()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<BoolConverterData>();
                ff.Converters.Add(new BooleanConverter());
                ff.Converters.Add("Bool", new BooleanConverter());
                var fw = new FileWriter<BoolConverterData>(sw, ff);

                var result = fw.Write(new BoolConverterData { BoolA = true, BoolB = true });
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("Y,Y\r\n", sb.ToString());
            }
        }

        [Test]
        public void FileWriter_Write_ConverterNotFoundException()
        {
            ExpectException.Throws<InvalidOperationException>("FileColumnAttribute has TextValueConverterKey of 'Bool' is not found within the FileFormat.Converters.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<BoolConverterData>();
                    ff.Converters.Add(new BooleanConverter());
                    var fw = new FileWriter<BoolConverterData>(sw, ff);

                    fw.Write(new BoolConverterData { BoolA = true, BoolB = true });
                }
            });
        }
    }
}
