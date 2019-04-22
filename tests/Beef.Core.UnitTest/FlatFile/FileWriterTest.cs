// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile;
using Beef.Core.UnitTest.FlatFile.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile
{
    [TestFixture]
    public class FileWriterTest
    {
        #region Ctor

        [Test]
        public void Ctor_TextWriter_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(null, ff);
            });
        }

        [Test]
        public void Ctor_FileFormat_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var fw = new FileWriter<Data>(sw, null);
                }
            });
        }

        #endregion

        #region WriteHeader

        [Test]
        public void WriteHeader_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    fw.WriteHeader(null);
                }
            });
        }

        [Test]
        public void WriteHeader_NoHeaderRowTypeException()
        {
            ExpectException.Throws<InvalidOperationException>("FileFormat has no corresponding HeaderRowType specified.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    fw.WriteHeader(new Header());
                }
            });
        }

        [Test]
        public void WriteHeader_DifferentHeaderRowTypeException()
        {
            ExpectException.Throws<ArgumentException>("Header Type must be the same as the specified FileFormat HeaderRowType.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    fw.WriteHeader(new Trailer());
                }
            });
        }

        [Test]
        public void WriteHeader_NotFirstRecordException()
        {
            ExpectException.Throws<InvalidOperationException>("A Header can only be written as the first record.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    fw.Write(new Data());
                    fw.WriteHeader(new Header());
                }
            });
        }

        [Test]
        public void WriteHeader_EndOfFileException()
        {
            ExpectException.Throws<InvalidOperationException>("Attempt made to write past the end of the file.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    fw.EndOfFile();
                    fw.WriteHeader(new Header());
                }
            });
        }

        [Test]
        public void WriteHeader()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                var result = fw.WriteHeader(new Header { Col = "hdr" });
                Assert.AreEqual(FileContentStatus.Header, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("\"hdr\"\r\n", sb.ToString());
            }
        }

        #endregion

        #region WriteTrailer

        [Test]
        public void WriteTrailer_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    fw.WriteTrailer(null);
                }
            });
        }

        [Test]
        public void WriteTrailer_NoHeaderRowTypeException()
        {
            ExpectException.Throws<InvalidOperationException>("FileFormat has no corresponding TrailerRowType specified.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    fw.WriteTrailer(new Trailer());
                }
            });
        }

        [Test]
        public void WriteTrailer_DifferentHeaderRowTypeException()
        {
            ExpectException.Throws<ArgumentException>("Trailer Type must be the same as the specified FileFormat TrailerRowType.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetTrailerRowType<Trailer>();
                    fw.WriteTrailer(new Header());
                }
            });
        }

        [Test]
        public void WriteTrailer_NotFirstRecordException()
        {
            ExpectException.Throws<InvalidOperationException>("A Trailer can only be written once.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetTrailerRowType<Trailer>();
                    fw.WriteTrailer(new Trailer());
                    fw.WriteTrailer(new Trailer());
                }
            });
        }

        [Test]
        public void WriteTrailer_EndOfFileException()
        {
            ExpectException.Throws<InvalidOperationException>("Attempt made to write past the end of the file.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetTrailerRowType<Trailer>();
                    fw.EndOfFile();
                    fw.WriteTrailer(new Trailer());
                }
            });
        }

        [Test]
        public void WriteTrailer_WriteAfterTrailerException()
        {
            ExpectException.Throws<InvalidOperationException>("Attempt made to write past a Trailer row; Trailer must be the last record.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetTrailerRowType<Trailer>();
                    fw.WriteTrailer(new Trailer());
                    fw.Write(new Data());
                }
            });
        }

        [Test]
        public void WriteTrailer()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetTrailerRowType<Trailer>();
                var result = fw.WriteTrailer(new Trailer { Col = "trl" });
                Assert.AreEqual(FileContentStatus.Trailer, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("\"trl\"\r\n", sb.ToString());
            }
        }

        #endregion

        #region Write

        [Test]
        public void Write_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    fw.Write((Data)null);
                }
            });
        }

        [Test]
        public void Write_EndOfFileException()
        {
            ExpectException.Throws<InvalidOperationException>("Attempt made to write past the end of the file.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    fw.EndOfFile();
                    fw.Write(new Data());
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void Write_NoHierarchy_Delimited()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>(textQualifier: FileFormatBase.NoCharacter);
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new Data { Col1 = "A", Col2 = "BB", Col3 = "CCC" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                fw.EndOfFile();
                Assert.AreEqual("A,BB,CCC\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_NoHierarchy_Fixed()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new FixedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new Data { Col1 = "A", Col2 = "BB", Col3 = "CCC" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("A         BB        CCC       \r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_NoHierarchy_FixedPaddingChar()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new FixedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.PaddingChar = 'x';
                var result = fw.Write(new Data { Col1 = "A", Col2 = "BB", Col3 = "CCC" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("AxxxxxxxxxBBxxxxxxxxCCCxxxxxxx\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_DelimiterInContent_NoTextQualifier()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>(textQualifier: FileFormatBase.NoCharacter);
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new Data { Col1 = "Ab,De", Col2 = "fgh", Col3 = "ijk" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("Text delimiter character found inside column text; no text qualifier has been specified and would result in errant record.", result.Records[0].Messages[0].Text);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("", sb.ToString());
            }
        }

        [Test]
        public void Write_DelimiterInContent_WithTextQualifier()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new Data { Col1 = "Ab,De", Col2 = "fgh", Col3 = "ijk" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("\"Ab,De\",\"fgh\",\"ijk\"\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_DelimiterInContent_WithTextQualifier_TextQualifierOnlyWithDelimiterOnWrite()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.TextQualifierOnlyWithDelimiterOnWrite = true;
                var result = fw.Write(new Data { Col1 = "Ab,De", Col2 = "fgh", Col3 = "ijk" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("\"Ab,De\",fgh,ijk\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_TextQualifierInContent()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new Data { Col1 = "Ab\"De", Col2 = "fgh", Col3 = "ijk" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("\"Ab\"\"De\",\"fgh\",\"ijk\"\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_TextQualifierInContent_TextQualifierOnlyWithDelimiterOnWrite()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.TextQualifierOnlyWithDelimiterOnWrite = true;
                var result = fw.Write(new Data { Col1 = "Ab\"De", Col2 = "fgh", Col3 = "ijk" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("\"Ab\"\"De\",fgh,ijk\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_ColumnWidthOverflow()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new Data { Col1 = "ABCDE", Col2 = "ABCDEFGHIJKL", Col3 = "ABCDEFGHIJKLM" });
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);
                Assert.IsNull(result.Value);
                Assert.AreEqual(2, result.Records[0].Messages.Count);

                Assert.AreEqual("Col2 must not exceed 10 characters in length.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[0].Type);
                Assert.AreEqual("Col3 exceeded 10 characters in length; value was truncated.", result.Records[0].Messages[1].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[1].Type);
            }
        }

        [Test]
        public void Write_Hierarchy_Delimited()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<HierarchyDataA>("A", 1, textQualifier: FileFormatBase.NoCharacter);
                var fw = new FileWriter<HierarchyDataA>(sw, ff);
                var result = fw.Write(CreateHierarchyA());
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(13, result.Records.Length);
                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D1", 2, false, result.Records[4]);
                AssertFileRecord(6, "D2", 2, false, result.Records[5]);
                AssertFileRecord(7, "D2", 2, false, result.Records[6]);
                AssertFileRecord(8, "D3", 2, false, result.Records[7]);
                AssertFileRecord(9, "D3", 2, false, result.Records[8]);
                AssertFileRecord(10, "E", 2, false, result.Records[9]);
                AssertFileRecord(11, "F", 1, false, result.Records[10]);
                AssertFileRecord(12, "G", 2, false, result.Records[11]);
                AssertFileRecord(13, "H", 3, false, result.Records[12]);

                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("001,A\r\n002,B\r\n003,C\r\n004,D1\r\n005,D1\r\n006,D2\r\n007,D2\r\n008,D3\r\n009,D3\r\n010,E\r\n011,F\r\n012,G\r\n013,H\r\n", sb.ToString());
            }
        }

        [Test]
        public void Write_Hierarchy_Fixed()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fw = new FileWriter<HierarchyDataA>(sw, ff);
                var result = fw.Write(CreateHierarchyA());
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(13, result.Records.Length);
                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D1", 2, false, result.Records[4]);
                AssertFileRecord(6, "D2", 2, false, result.Records[5]);
                AssertFileRecord(7, "D2", 2, false, result.Records[6]);
                AssertFileRecord(8, "D3", 2, false, result.Records[7]);
                AssertFileRecord(9, "D3", 2, false, result.Records[8]);
                AssertFileRecord(10, "E", 2, false, result.Records[9]);
                AssertFileRecord(11, "F", 1, false, result.Records[10]);
                AssertFileRecord(12, "G", 2, false, result.Records[11]);
                AssertFileRecord(13, "H", 3, false, result.Records[12]);

                result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("001A \r\n002B \r\n003C \r\n004D1\r\n005D1\r\n006D2\r\n007D2\r\n008D3\r\n009D3\r\n010E \r\n011F \r\n012G \r\n013H \r\n", sb.ToString());
            }
        }

        private HierarchyDataA CreateHierarchyA()
        {
            var a = new HierarchyDataA();
            a.B = new HierarchyDataB();
            a.C = new HierarchyDataC();
            a.C.D1 = new HierarchyDataD[] { new HierarchyDataD(), new HierarchyDataD() };
            a.C.D2 = new List<HierarchyDataD>(new HierarchyDataD[] { new HierarchyDataD(), new HierarchyDataD() });
            a.C.D3 = new HierarchyDataD[] { new HierarchyDataD(), new HierarchyDataD() };
            a.C.E = new HierarchyDataE();
            a.F = new HierarchyDataF();
            a.F.G = new HierarchyDataG();
            a.F.G.H = new HierarchyDataH();
            return a;
        }

        private void AssertFileRecord(long lineNumber, string recordIdentifier, int level, bool hasErrors, FileRecord fr)
        {
            Assert.AreEqual(lineNumber, fr.LineNumber);
            Assert.AreEqual(recordIdentifier, fr.RecordIdentifier);
            Assert.AreEqual(level, fr.Level);
            Assert.AreEqual(hasErrors, fr.HasErrors);
        }

        #endregion

        #region FileValidation

        [Test]
        public void Write_FileValidation_MustHaveRows()
        {
            try
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.FileValidation = FileValidation.MustHaveRows;
                    fw.EndOfFile();
                    Assert.Fail();
                }
            }
            catch (FileValidationException ex)
            {
                Assert.AreEqual(FileValidation.MustHaveRows, ex.FileValidation);
                Assert.AreEqual("The file must contain at least one row; i.e. cannot be empty.", ex.Message);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void Write_FileValidation_MustHaveHeaderRow_NoHeaderType()
        {
            ExpectException.Throws<InvalidOperationException>("FileFormat specifies FileValidation with MustHaveHeaderRow; no corresponding HeaderRowType specified.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.FileValidation = FileValidation.MustHaveHeaderRow;
                    fw.EndOfFile();
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void Write_FileValidation_MustHaveHeaderRow_NoContent()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                ff.SetTrailerRowType<Trailer>();
                ff.FileValidation = FileValidation.MustHaveHeaderRow;
                fw.EndOfFile();
            }
        }

        [Test]
        public void Write_FileValidation_MustHaveHeaderRow_WithContentNoHeader()
        {
            try
            { 
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    ff.SetTrailerRowType<Trailer>();
                    ff.FileValidation = FileValidation.MustHaveHeaderRow;
                    fw.Write(new Data());
                    Assert.Fail();
                }
            }
            catch (FileValidationException ex)
            {
                Assert.AreEqual(FileValidation.MustHaveHeaderRow, ex.FileValidation);
                Assert.AreEqual("The first record must be identified as a Header row.", ex.Message);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void Write_FileValidation_MustHaveHeaderRow_WithHeaderAndContent()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                ff.SetTrailerRowType<Trailer>();
                ff.FileValidation = FileValidation.MustHaveHeaderRow;
                fw.WriteHeader(new Header());
                fw.Write(new Data());
                fw.EndOfFile();
            }
        }

        [Test]
        public void Write_FileValidation_MustHaveTrailerRow_NoHeaderType()
        {
            ExpectException.Throws<InvalidOperationException>("FileFormat specifies FileValidation with MustHaveTrailerRow; no corresponding TrailerRowType specified.", () =>
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.FileValidation = FileValidation.MustHaveTrailerRow;
                    fw.EndOfFile();
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void Write_FileValidation_MustHaveTrailerRow_NoContent()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                ff.SetTrailerRowType<Trailer>();
                ff.FileValidation = FileValidation.MustHaveTrailerRow;
                fw.EndOfFile();
            }
        }

        [Test]
        public void Write_FileValidation_MustHaveTrailerRow_WithContentNoTrailer()
        {
            try
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    ff.SetTrailerRowType<Trailer>();
                    ff.FileValidation = FileValidation.MustHaveTrailerRow;
                    fw.Write(new Data());
                    fw.EndOfFile();
                    Assert.Fail();
                }
            }
            catch (FileValidationException ex)
            {
                Assert.AreEqual(FileValidation.MustHaveTrailerRow, ex.FileValidation);
                Assert.AreEqual("The file must contain a Trailer row as the last record.", ex.Message);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void Write_FileValidation_MustHaveTrailerRow_WithContentAndTrailer()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                ff.SetTrailerRowType<Trailer>();
                ff.FileValidation = FileValidation.MustHaveTrailerRow;
                fw.Write(new Data());
                fw.WriteTrailer(new Trailer());
                fw.EndOfFile();
            }
        }

        [Test]
        public void Write_FileValidation_MustHaveAtLeastOneContentRow_NoRecords()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                ff.SetTrailerRowType<Trailer>();
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                fw.EndOfFile();
            }
        }

        [Test]
        public void Write_FileValidation_MustHaveAtLeastOneContentRow_WithHeaderRecord()
        {
            try
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    ff.SetTrailerRowType<Trailer>();
                    ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                    fw.WriteHeader(new Header());
                    fw.EndOfFile();
                }
            }
            catch (FileValidationException ex)
            {
                Assert.AreEqual(FileValidation.MustHaveAtLeastOneContentRow, ex.FileValidation);
                Assert.AreEqual("The file must contain at least one content row.", ex.Message);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void Write_FileValidation_MustHaveAtLeastOneContentRow_WithTrailerRecord()
        {
            try
            {
                var sb = new StringBuilder();
                using (var sw = new StringWriter(sb))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    var fw = new FileWriter<Data>(sw, ff);
                    ff.SetHeaderRowType<Header>();
                    ff.SetTrailerRowType<Trailer>();
                    ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                    fw.WriteTrailer(new Trailer());
                    fw.EndOfFile();
                }
            }
            catch (FileValidationException ex)
            {
                Assert.AreEqual(FileValidation.MustHaveAtLeastOneContentRow, ex.FileValidation);
                Assert.AreEqual("The file must contain at least one content row.", ex.Message);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void Write_FileValidation_MustHaveAtLeastOneContentRow_WithContent()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                ff.SetHeaderRowType<Header>();
                ff.SetTrailerRowType<Trailer>();
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                fw.Write(new Data());
                fw.EndOfFile();
            }
        }

        #endregion

        #region Write_Collection

        [Test]
        public void Write_Collection_Null()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write((Data[])null);
                Assert.AreEqual(0, result.Length);
            }
        }

        [Test]
        public void Write_Collection_Empty()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fw = new FileWriter<Data>(sw, ff);
                var result = fw.Write(new List<Data>());
                Assert.AreEqual(0, result.Length);
            }
        }

        [Test]
        public void Write_Collection()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                var ff = new DelimitedFileFormat<Data>(textQualifier: FileFormatBase.NoCharacter);
                var fw = new FileWriter<Data>(sw, ff);
                var results = fw.Write(new Data[] { new Data { Col1 = "A" }, new Data { Col1 = "B" } });
                Assert.AreEqual(2, results.Length);
                Assert.AreEqual(FileContentStatus.Content, results[0].Status);
                Assert.IsFalse(results[0].HasErrors);
                Assert.AreEqual(FileContentStatus.Content, results[1].Status);
                Assert.IsFalse(results[1].HasErrors);

                var result = fw.EndOfFile();
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("A,,\r\nB,,\r\n", sb.ToString());
            }
        }

        #endregion
    }
}
