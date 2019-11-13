// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using Beef.Entities;
using Beef.Core.UnitTest.FlatFile.Entities;
using System;
using System.IO;
using NUnit.Framework;

namespace Beef.Core.UnitTest.FlatFile
{
    [TestFixture]
    public class DelimitedFileReaderTest
    {
        [Test]
        public void Ctor_Default()
        {
            using (var sr = new StringReader(string.Empty))
            {
                var dff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, dff);
                AssertReaderReady(fr);
            }
        }

        private void AssertReaderReady(FileReaderBase fr)
        {
            Assert.IsFalse(fr.StopOnError);
            Assert.IsFalse(fr.IsEndOfFile);
            Assert.IsFalse(fr.IsLastRecord);
        }

        [Test]
        public void Ctor_TextReaderNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var dff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(null, dff);
            });
        }

        [Test]
        public void Ctor_FileFormatNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                using (var sr = new StringReader(string.Empty))
                {
                    var fr = new FileReader<Data>(sr, null);
                }
            });
        }

        private DelimitedFileFormat<Data> CreateFileFormat(string contentRecordIdentifer = null, int hierarchyColumnIndex = 0, TextQualifierHandling tqh = TextQualifierHandling.Strict)
        {
            DelimitedFileFormat<Data> dff = null;

            if (contentRecordIdentifer == null)
                dff = new DelimitedFileFormat<Data>();
            else
                dff = new DelimitedFileFormat<Data>(contentRecordIdentifer, hierarchyColumnIndex);

            dff.TextQualifierHandling = tqh;

            return dff;
        }

        private FileOperationResult ReadRecord(string record, DelimitedFileFormat<Data> dff, string[] cols, bool hasErrors = false, params string[] messages)
        {
            using (var sr = new StringReader(record))
            {
                var fr = new FileReader<Data>(sr, dff);
                AssertReaderReady(fr);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.AreEqual(hasErrors, result.HasErrors);
                Assert.IsNotNull(result.Records);
                Assert.AreEqual(1, result.Records.Count);

                var rec = result.Records[0];
                Assert.AreEqual(1, rec.LineNumber);
                Assert.AreEqual(record, rec.LineData);
                Assert.AreEqual(hasErrors, rec.HasErrors);

                if (messages != null && messages.Length > 0)
                {
                    Assert.IsTrue(rec.HasMessages);
                    Assert.AreEqual(messages.Length, rec.Messages.Count);
                    for (int i = 0; i < messages.Length; i++)
                    {
                        Assert.AreEqual(hasErrors ? MessageType.Error : MessageType.Warning, rec.Messages[i].Type);
                        Assert.AreEqual(messages[i], rec.Messages[i].Text);
                        Assert.IsNull(rec.Messages[i].Property);
                    }
                }
                else
                    Assert.IsFalse(rec.HasMessages);

                if (cols == null)
                    Assert.IsNull(rec.Columns);
                else
                {
                    Assert.AreEqual(cols.Length, rec.Columns.Count);
                    for (int i = 0; i < cols.Length; i++)
                    {
                        Assert.AreEqual(cols[i], rec.Columns[i]);
                    }
                }

                if (hasErrors)
                    Assert.IsNull(result.Value);
                else
                    Assert.IsNotNull(result.Value);

                Assert.IsFalse(fr.IsEndOfFile);
                Assert.IsTrue(fr.IsLastRecord);
                return result;
            }
        }

        #region Read_DelimiterOnly

        // ABCD,EFGH,IJKL == ABCD , EFGH , IJKL
        private const string Record1 = "ABCD,EFGH,IJKL"; 
        private static readonly string[] Record1Cols = new string[] { "ABCD", "EFGH", "IJKL" };

        [Test]
        public void Read_DelimiterOnly_Strict()
        {
            ReadRecord(Record1, CreateFileFormat(tqh: TextQualifierHandling.Strict), Record1Cols);
        }

        [Test]
        public void Read_DelimiterOnly_LooseAllow()
        {
            ReadRecord(Record1, CreateFileFormat(tqh: TextQualifierHandling.LooseAllow), Record1Cols);
        }


        [Test]
        public void Read_DelimiterOnly_LooseSkip()
        {
            ReadRecord(Record1, CreateFileFormat(tqh: TextQualifierHandling.LooseSkip), Record1Cols);
        }

        #endregion

        #region Read_QualifierValid 

        // "AB,CD","EF""GH","IJ""""KL" == AB,CD , EF"GH , IJ""KL
        private const string Record2 = "\"AB,CD\",\"EF\"\"GH\",\"IJ\"\"\"\"KL\"";
        private static readonly string[] Record2Cols = new string[] { "AB,CD", "EF\"GH", "IJ\"\"KL" };

        [Test]
        public void Read_QualifierValid_Strict()
        {
            ReadRecord(Record2, CreateFileFormat(tqh: TextQualifierHandling.Strict), Record2Cols);
        }

        [Test]
        public void Read_QualifierValid_LooseAllow()
        {
            ReadRecord(Record2, CreateFileFormat(tqh: TextQualifierHandling.LooseAllow), Record2Cols);
        }


        [Test]
        public void Read_QualifierValid_LooseSkip()
        {
            ReadRecord(Record2, CreateFileFormat(tqh: TextQualifierHandling.LooseSkip), Record2Cols);
        }

        #endregion

        #region Read_InsideQualified

        // "AB,"EF""GH","IJ""""KL"
        private const string Record3 = "\"AB,\"EF\"\"GH\",\"IJ\"\"\"\"KL\"";
        private const string Record3Message = "Text qualifier character found (position 5) inside qualified text without being escaped correctly (e.g. double qualifier '\"\"').";

        [Test]
        public void Read_InsideQualified_Strict()
        {
            // N/A
            ReadRecord(Record3, CreateFileFormat(tqh: TextQualifierHandling.Strict), null, true, Record3Message);
        }

        [Test]
        public void Read_InsideQualified_LooseAllow()
        {
            // AB,"EF"GH , IJ""KL
            ReadRecord(Record3, CreateFileFormat(tqh: TextQualifierHandling.LooseAllow), new string[] { "AB,\"EF\"GH", "IJ\"\"KL" }, false, Record3Message);
        }


        [Test]
        public void Read_InsideQualified_LooseSkip()
        {
            // AB,EF"GH , IJ""KL
            ReadRecord(Record3, CreateFileFormat(tqh: TextQualifierHandling.LooseSkip), new string[] { "AB,EF\"GH", "IJ\"\"KL" }, false, Record3Message);
        }

        #endregion

        #region Read_InsideUnqualified

        // AB,EF"GH","IJ""""KL"
        private const string Record4 = "AB,EF\"GH\",\"IJ\"\"\"\"KL\"";
        private const string Record4Message = "Text qualifier character found (position {0}) inside unqualified text; text must be qualified and escaped correctly (e.g. double qualifier '\"\"').";

        [Test]
        public void Read_InsideUnqualified_Strict()
        {
            // N/A
            ReadRecord(Record4, CreateFileFormat(tqh: TextQualifierHandling.Strict), null, true, string.Format(Record4Message, 6));
        }

        [Test]
        public void Read_InsideUnqualified_LooseAllow()
        {
            // AB , EF"GH" , IJ""KL
            ReadRecord(Record4, CreateFileFormat(tqh: TextQualifierHandling.LooseAllow), new string[] { "AB", "EF\"GH\"", "IJ\"\"KL" }, false, string.Format(Record4Message, 6), string.Format(Record4Message, 9));
        }


        [Test]
        public void Read_InsideUnqualified_LooseSkip()
        {
            // AB , EFGH , IJ""KL
            ReadRecord(Record4, CreateFileFormat(tqh: TextQualifierHandling.LooseSkip), new string[] { "AB", "EFGH", "IJ\"\"KL" }, false, string.Format(Record4Message, 6), string.Format(Record4Message, 9));
        }

        #endregion

        #region Read_QualifiedClose

        // "AB,EF""GH","IJ""KL
        private const string Record5 = "\"AB,EF\"\"GH\",\"IJ\"\"KL";
        private const string Record5Message = "Text qualifier character missing to close the text qualification for the final column.";

        [Test]
        public void Read_QualifiedClose_Strict()
        {
            // N/A
            ReadRecord(Record5, CreateFileFormat(tqh: TextQualifierHandling.Strict), null, true, Record5Message);
        }

        [Test]
        public void Read_QualifiedClose_LooseAllow()
        {
            // AB,EF"GH , IJ"KL
            ReadRecord(Record5, CreateFileFormat(tqh: TextQualifierHandling.LooseAllow), new string[] { "AB,EF\"GH", "IJ\"KL" }, false, Record5Message);
        }


        [Test]
        public void Read_QualifiedClose_LooseSkip()
        {
            // AB,EF"GH , IJ"KL
            ReadRecord(Record5, CreateFileFormat(tqh: TextQualifierHandling.LooseSkip), new string[] { "AB,EF\"GH", "IJ\"KL" }, false, Record5Message);
        }

        #endregion

        [Test] 
        public void Read_NoHierarchyValid()
        {
            var result = ReadRecord("AB,CD", CreateFileFormat(), new string[] { "AB", "CD" });
            Assert.IsInstanceOf<Data>(result.Value);
            var data = (Data)result.Value;
            Assert.AreEqual("AB", data.Col1);
            Assert.AreEqual("CD", data.Col2);
            Assert.AreEqual(string.Empty, data.Col3);
            Assert.IsNull(result.Records[0].RecordIdentifier);
        }


        [Test]
        public void Read_HierarchyValid()
        {
            var result = ReadRecord("AB,CD,X", CreateFileFormat("X", 2), new string[] { "AB", "CD", "X" });
            Assert.IsInstanceOf<Data>(result.Value);
            var data = (Data)result.Value;
            Assert.AreEqual("AB", data.Col1);
            Assert.AreEqual("CD", data.Col2);
            Assert.AreEqual("X", data.Col3);
            Assert.AreEqual("X", result.Records[0].RecordIdentifier);
        }

        [Test]
        public void Read_HierarchyOutOfBounds()
        {
            var result = ReadRecord("AB,CD", CreateFileFormat("X", 2), new string[] { "AB", "CD" }, true, "Unable to determine the code identitier as the hierarchy column index is outside the bounds of the number of columns found for the record.");
            Assert.IsNull(result.Records[0].RecordIdentifier);
        }
    }
}
