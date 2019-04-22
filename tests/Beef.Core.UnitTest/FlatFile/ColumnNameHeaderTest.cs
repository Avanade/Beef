// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using Beef.Entities;
using Beef.Core.UnitTest.FlatFile.Entities;
using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile
{
    [TestFixture]
    public class ColumnNameHeaderTest
    {
        [Test]
        public void Read_IsHierarchicalException()
        {
            ExpectException.Throws<InvalidOperationException>("The ColumnNameHeader cannot be used when the File Format is hierarchical.", () =>
            {
                using (var sr = new StringReader("Z"))
                {
                    var ff = new DelimitedFileFormat<HierarchyDataA>("A");
                    ff.SetHeaderRowType<ColumnNameHeader>("Z");
                    var fr = new FileReader<HierarchyDataA>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_OnlyForHeaderException()
        {
            ExpectException.Throws<InvalidOperationException>("The ColumnNameHeader can only be used for a Header row.", () =>
            {
                using (var sr = new StringReader("Col1,Col2,Col3"))
                {
                    var ff = new DelimitedFileFormat<ColumnNameHeader>();
                    var fr = new FileReader<ColumnNameHeader>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_IncorrectColumnCountWarning()
        {
            using (var sr = new StringReader("Col1,Col2"))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.SetHeaderRowType<ColumnNameHeader>();
                var result = fr.Read();
                Assert.AreEqual(FileContentStatus.Header, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[0].Type);
                Assert.AreEqual("The number of Header columns '2' does not match that specified for the expected Content '3'.", result.Records[0].Messages[0].Text);
            }
        }

        [Test]
        public void Read_IncorrectColumnName()
        {
            using (var sr = new StringReader("Col1,Xyz2,Col3"))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.SetHeaderRowType<ColumnNameHeader>();
                var result = fr.Read();
                Assert.AreEqual(FileContentStatus.Header, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[0].Type);
                Assert.AreEqual("Column (position 2) content 'Xyz2' does not match the expected name 'Col2'.", result.Records[0].Messages[0].Text);
            }
        }

        [Test]
        public void Read()
        {
            using (var sr = new StringReader("Col1,col2,Col3"))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.SetHeaderRowType<ColumnNameHeader>();
                var result = fr.Read();
                Assert.AreEqual(FileContentStatus.Header, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Write_IsHierarchicalException()
        {
            ExpectException.Throws<InvalidOperationException>("The ColumnNameHeader cannot be used when the File Format is hierarchical.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<HierarchyDataA>("A");
                    ff.SetHeaderRowType<ColumnNameHeader>("Z");
                    var fr = new FileWriter<HierarchyDataA>(sw, ff);
                    fr.WriteHeader(ColumnNameHeader.Default);
                }
            });
        }

        [Test]
        public void Write_OnlyForHeaderException()
        {
            ExpectException.Throws<InvalidOperationException>("The ColumnNameHeader can only be used for a Header row.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<ColumnNameHeader>();
                    var fr = new FileWriter<ColumnNameHeader>(sw, ff);
                    fr.Write(ColumnNameHeader.Default);
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void Write()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fr = new FileWriter<Data>(sw, ff);
                ff.TextQualifierOnlyWithDelimiterOnWrite = true;
                ff.SetHeaderRowType<ColumnNameHeader>();
                fr.WriteHeader(ColumnNameHeader.Default);
                Assert.AreEqual("Col1,Col2,Col3\r\n", sb.ToString());
            }
        }
    }
}
