// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using Beef.Entities;
using Beef.Core.UnitTest.FlatFile.Entities;
using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Beef.FlatFile.Converters;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile
{
    [TestFixture]
    public class FileReaderTest
    {
        #region FileValidation

        [Test]
        public void Read_FileValidation_MustHaveHeaderRow_NoType()
        {
            ExpectException.Throws<InvalidOperationException>("FileFormat specifies FileValidation with MustHaveHeaderRow; no corresponding HeaderRowType specified.", () =>
            {
                using (var sr = new StringReader(string.Empty))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    ff.FileValidation = FileValidation.MustHaveHeaderRow;
                    var fr = new FileReader<Data>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveTrailerRow_NoType()
        {
            ExpectException.Throws<InvalidOperationException>("FileFormat specifies FileValidation with MustHaveTrailerRow; no corresponding TrailerRowType specified.", () =>
            {
                using (var sr = new StringReader(string.Empty))
                {
                    var ff = new FixedFileFormat<Data>();
                    ff.FileValidation = FileValidation.MustHaveTrailerRow;
                    var fr = new FileReader<Data>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveRows()
        {
            ExpectException.Throws<FileValidationException>("The file must contain at least one row; i.e. cannot be empty.", () =>
            {
                using (var sr = new StringReader(string.Empty))
                {
                    var ff = new DelimitedFileFormat<Data>();
                    ff.FileValidation = FileValidation.MustHaveRows;
                    var fr = new FileReader<Data>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveHeaderRow_WithContent()
        {
            ExpectException.Throws<FileValidationException>("The first record must be identified as a header row.", () =>
            {
                using (var sr = new StringReader("D"))
                {
                    var ff = new FixedFileFormat<Data>("D", 0, 1);
                    ff.SetHeaderRowType<Header>("H");
                    ff.FileValidation = FileValidation.MustHaveHeaderRow;
                    var fr = new FileReader<Data>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveHeaderRow_NoContent()
        {
            // Where no content then the MustHaveHeaderRow is ignored.
            using (var sr = new StringReader(string.Empty))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.FileValidation = FileValidation.MustHaveHeaderRow;
                var fr = new FileReader<Data>(sr, ff);
                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
            }
        }

        [Test]
        public void Read_FileValidation_MustHaveTrailerRow_WithContent()
        {
            ExpectException.Throws<FileValidationException>("The last record must be identified as a trailer row.", () =>
            {
                using (var sr = new StringReader("D"))
                {
                    var ff = new FixedFileFormat<Data>("D", 0, 1);
                    ff.SetTrailerRowType<Trailer>("T");
                    ff.FileValidation = FileValidation.MustHaveTrailerRow;
                    var fr = new FileReader<Data>(sr, ff);
                    fr.Read();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveTrailerRow_NoContent()
        {
            // Where no content then the MustHaveTrailerRow is ignored.
            using (var sr = new StringReader(string.Empty))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveTrailerRow;
                var fr = new FileReader<Data>(sr, ff);
                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
            }
        }

        [Test]
        public void Read_FileValidation_MustHaveAtLeastOneContentRow_WithHeaderContent()
        {
            ExpectException.Throws<FileValidationException>("The file must contain at least one content row.", () =>
            {
                using (var sr = new StringReader("H"))
                {
                    var ff = new FixedFileFormat<Data>("D", 0, 1);
                    ff.SetHeaderRowType<Header>("H");
                    ff.SetTrailerRowType<Trailer>("T");
                    ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                    var fr = new FileReader<Data>(sr, ff);

                    var result = fr.Read();
                    Assert.IsNotNull(result);
                    Assert.AreEqual(FileContentStatus.Header, result.Status);

                    fr.Read();
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveAtLeastOneContentRow_WithTrailerContent()
        {
            ExpectException.Throws<FileValidationException>("The file must contain at least one content row.", () =>
            {
                using (var sr = new StringReader("T"))
                {
                    var ff = new FixedFileFormat<Data>("D", 0, 1);
                    ff.SetHeaderRowType<Header>("H");
                    ff.SetTrailerRowType<Trailer>("T");
                    ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                    var fr = new FileReader<Data>(sr, ff);
                    var result = fr.Read();
                }
            });
        }

        [Test]
        public void Read_FileValidation_MustHaveAtLeastOneContentRow_NoContent()
        {
            // Where no content then the MustHaveTrailerRow is ignored.
            using (var sr = new StringReader(string.Empty))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);
                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
            }
        }

        [Test]
        public void Read_FileValidation_MustHaveAtLeastOneContentRow_WithContent()
        {
            // Where no content then the MustHaveTrailerRow is ignored.
            using (var sr = new StringReader("D"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual("D", ((Data)result.Value).Col1);
                Assert.AreEqual(string.Empty, ((Data)result.Value).Col2);
                Assert.AreEqual(string.Empty, ((Data)result.Value).Col3);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
            }
        }

        #endregion

        #region NoHierarchy

        [Test]
        public void Read_NoHierarchy_HeaderThenContent()
        {
            using (var sr = new StringReader("H\r\nD"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Header, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(1, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.TotalLines);
            }
        }

        [Test]
        public void Read_NoHierarchy_ContentThenTrailer()
        {
            using (var sr = new StringReader("D\r\nT"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(1, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Trailer, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.TotalLines);
            }
        }

        [Test]
        public void Read_NoHierarchy_HeaderContentTrailer()
        {
            using (var sr = new StringReader("H\r\nD\r\nT"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Header, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(1, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Trailer, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(3, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(3, result.TotalLines);
            }
        }

        [Test]
        public void Read_NoHierarchy_IncorrectRecordPosition_HeaderAndTrailer()
        {
            using (var sr = new StringReader("D\r\nT\r\nH"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(1, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("Record identified as Trailer; is not last record in the file.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(2, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("Record identified as Header; is not first record in the file.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(3, result.TotalLines);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(3, result.TotalLines);
            }
        }

        [Test]
        public void Read_NoHierarchy_UnknownRecordIdentifier()
        {
            using (var sr = new StringReader("X"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                ff.FileValidation = FileValidation.MustHaveAtLeastOneContentRow;
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("X", result.Records[0].RecordIdentifier);
                Assert.AreEqual("Record identifier is unknown.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(1, result.TotalLines);
            }
        }

        #endregion

        #region Hierarchy

        private const string AllItemsData = "001A\r\n002B\r\n003C\r\n004D1\r\n005D2\r\n006D3\r\n007D1\r\n008D2\r\n009D3\r\n010E\r\n011F\r\n012G\r\n013H";

        [Test]
        public void Read_Hierarchy_AllItemsWithin()
        {
            using (var sr = new StringReader(AllItemsData))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(13, result.Records.Length);
                Assert.AreEqual(13, result.TotalLines);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D2", 2, false, result.Records[4]);
                AssertFileRecord(6, "D3", 2, false, result.Records[5]);
                AssertFileRecord(7, "D1", 2, false, result.Records[6]);
                AssertFileRecord(8, "D2", 2, false, result.Records[7]);
                AssertFileRecord(9, "D3", 2, false, result.Records[8]);
                AssertFileRecord(10, "E", 2, false, result.Records[9]);
                AssertFileRecord(11, "F", 1, false, result.Records[10]);
                AssertFileRecord(12, "G", 2, false, result.Records[11]);
                AssertFileRecord(13, "H", 3, false, result.Records[12]);

                Assert.IsNotNull(result.Value);
                Assert.IsInstanceOf(typeof(HierarchyDataA), result.Value);

                // Confirm the value has been updated correctly.
                var val = (HierarchyDataA)result.Value;
                Assert.IsNotNull(val.B);
                Assert.IsNotNull(val.C);
                Assert.IsNotNull(val.C.D1);
                Assert.AreEqual(2, val.C.D1.Length);
                Assert.IsNotNull(val.C.D2);
                Assert.AreEqual(2, val.C.D2.Count);
                Assert.IsNotNull(val.C.D3);
                Assert.IsInstanceOf(typeof(HierarchyDataD[]), val.C.D3);
                Assert.AreEqual(2, ((HierarchyDataD[])val.C.D3).Length);
                Assert.IsNotNull(val.F);
                Assert.IsNotNull(val.F.G);
                Assert.IsNotNull(val.F.G.H);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_AllItemsWithin_Volume()
        {
            var tot = 10000;
            var sb = new StringBuilder();
            for (int i = 0; i < tot; i++)
                sb.AppendLine(AllItemsData);

            using (var sr = new StringReader(sb.ToString()))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);
                Assert.AreEqual(tot * 13, fr.ReadToEnd());
            }
        }

        [Test]
        public void Read_Hierarchy_MandatoryPropertyError()
        {
            using (var sr = new StringReader("001A\r\n002F"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(2, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "F", 1, true, result.Records[1]);
                Assert.AreEqual("B is required; no record found.", result.Records[1].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_PropertyTooManyError()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003B"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(3, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "B", 1, true, result.Records[2]);
                Assert.AreEqual("B does not support multiple records; too many provided.", result.Records[2].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_MandatoryProperty()
        {
            using (var sr = new StringReader("001A\r\n002B"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_MandatoryArrayError()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003C"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(3, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, true, result.Records[2]);

                Assert.AreEqual("D1 is required; no record found.", result.Records[2].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_MandatoryArrayTooManyError()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003C\r\n004D1\r\n005D1\r\n006D1\r\n007D1\r\n008D1"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(8, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D1", 2, false, result.Records[4]);
                AssertFileRecord(6, "D1", 2, false, result.Records[5]);
                AssertFileRecord(7, "D1", 2, false, result.Records[6]);
                AssertFileRecord(8, "D1", 2, true, result.Records[7]);

                Assert.AreEqual("D1 must not exceed 4 records(s); too many provided.", result.Records[7].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_MandatoryArray()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003C\r\n004D1\r\n005D1"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(5, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D1", 2, false, result.Records[4]);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_LessThanMinimum()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003C\r\n004D1\r\n005D2\r\n006D3"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(6, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D2", 2, false, result.Records[4]);
                AssertFileRecord(6, "D3", 2, true, result.Records[5]);

                Assert.AreEqual("D1 must have at least 2 records(s); additional required.", result.Records[5].Messages[0].Text);
                Assert.AreEqual("D2 must have at least 2 records(s); additional required.", result.Records[5].Messages[1].Text);
                Assert.AreEqual("D3 must have at least 2 records(s); additional required.", result.Records[5].Messages[2].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_RecordIdentifierUnknown_First()
        {
            using (var sr = new StringReader("001X\r\n002A\r\n003B"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);

                AssertFileRecord(1, "X", -1, true, result.Records[0]);
                Assert.AreEqual("Record identifier is unknown.", result.Records[0].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(2, result.Records.Length);

                AssertFileRecord(2, "A", 0, false, result.Records[0]);
                AssertFileRecord(3, "B", 1, false, result.Records[1]);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_RecordIdentifierUnknown_Middle()
        {
            using (var sr = new StringReader("001A\r\n002X\r\n003B"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(3, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "X", -1, true, result.Records[1]);
                AssertFileRecord(3, "B", 1, false, result.Records[2]);

                Assert.AreEqual("Record identifier is unknown.", result.Records[1].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_RecordIdentifierUnknown_Last()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003X"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(3, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "X", -1, true, result.Records[2]);

                Assert.AreEqual("Record identifier is unknown.", result.Records[2].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_Position_NotPeerOfPreviousError()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003C\r\n004D1\r\n005D1\r\n006G"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(6, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "C", 1, false, result.Records[2]);
                AssertFileRecord(4, "D1", 2, false, result.Records[3]);
                AssertFileRecord(5, "D1", 2, false, result.Records[4]);
                AssertFileRecord(6, "G", -1, true, result.Records[5]);

                Assert.AreEqual("Record identifier is not a valid peer of the previous record.", result.Records[5].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_Position_NotWithinTraversedError()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003F\r\n004G\r\n005H\r\n006E"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(6, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "F", 1, false, result.Records[2]);
                AssertFileRecord(4, "G", 2, false, result.Records[3]);
                AssertFileRecord(5, "H", 3, false, result.Records[4]);
                AssertFileRecord(6, "E", -1, true, result.Records[5]);

                Assert.AreEqual("Record identifier is not valid within current traversed hierarchy.", result.Records[5].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        [Test]
        public void Read_Hierarchy_Position_NotDirectDescendentError()
        {
            using (var sr = new StringReader("001A\r\n002B\r\n003G"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(3, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, false, result.Records[1]);
                AssertFileRecord(3, "G", -1, true, result.Records[2]);

                Assert.AreEqual("Record identifier is not a direct descendent of the previous record.", result.Records[2].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        private void AssertFileRecord(long lineNumber, string recordIdentifier, int level, bool hasErrors, FileRecord fr)
        {
            Assert.AreEqual(lineNumber, fr.LineNumber);
            Assert.AreEqual(recordIdentifier, fr.RecordIdentifier);
            Assert.AreEqual(level, fr.Level);
            Assert.AreEqual(hasErrors, fr.HasErrors);

            if (!hasErrors)
            {
                var hd = (HierarchyData)fr.Value;
                Assert.AreEqual(lineNumber, hd.LineNumber);
                Assert.AreEqual(recordIdentifier, hd.Code);
            }
        }

        [Test]
        public void Read_Hierarchy_NotAClassException()
        {
            ExpectException.Throws<ArgumentException>("Type 'HierarchyNotAClass' Property 'Data' must be a class or collection (FileHierarchyAttribute).", () =>
            {
                using (var sr = new StringReader("X"))
                {
                    var ff = new DelimitedFileFormat<HierarchyNotAClass>(FileFormatBase.TabCharacter);
                    var fr = new FileReader<HierarchyNotAClass>(sr, ff);
                    var result = fr.Read();
                }
            });
        }

        #endregion

        #region ColumnCountValidation

        [Test]
        public void Read_ColumnCountValidation_Fixed_LessAndGreaterThanError()
        {
            using (var sr = new StringReader("ABC       DEFG      HIJKL     \r\nABC       DEFG      \r\nABC       DEFG      HIJKL     MNOPQR    "))
            {
                var ff = new FixedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.ColumnCountValidation = ColumnCountValidation.LessAndGreaterThanError;

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsFalse(result.Records[0].HasMessages);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are less columns '2' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[0].Type);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are more columns '4' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[0].Type);
            }
        }

        [Test]
        public void Read_ColumnCountValidation_Fixed_LessAndGreaterThanWarning()
        {
            using (var sr = new StringReader("ABC       DEFG      HIJKL     \r\nABC       DEFG      \r\nABC       DEFG      HIJKL     MNOPQR    "))
            {
                var ff = new FixedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.ColumnCountValidation = ColumnCountValidation.LessAndGreaterThanWarning;

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsFalse(result.Records[0].HasMessages);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are less columns '2' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[0].Type);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are more columns '4' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[0].Type);
            }
        }

        [Test]
        public void Read_ColumnCountValidation_Delimited_LessAndGreaterThanError()
        {
            using (var sr = new StringReader("ABC,DEFG,HIJKL,\r\nABC,DEFG,\r\nABC,DEFG,HIJKL,MNOPQR\r\n"))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.ColumnCountValidation = ColumnCountValidation.LessAndGreaterThanError;

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsFalse(result.Records[0].HasMessages);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are less columns '2' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[0].Type);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are more columns '4' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[0].Type);
            }
        }

        [Test]
        public void Read_ColumnCountValidation_Delimited_LessAndGreaterThanWarning()
        {
            using (var sr = new StringReader("ABC,DEFG,HIJKL,\r\nABC,DEFG,\r\nABC,DEFG,HIJKL,MNOPQR\r\n"))
            {
                var ff = new DelimitedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);
                ff.ColumnCountValidation = ColumnCountValidation.LessAndGreaterThanWarning;

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsFalse(result.Records[0].HasMessages);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are less columns '2' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[0].Type);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsTrue(result.Records[0].HasMessages);
                Assert.AreEqual("There are more columns '4' within the record than expected '3'.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[0].Type);
            }
        }

        #endregion

        [Test]
        public void Read_LineNumberError()
        {
            using (var sr = new StringReader("001A\r\n003B\r\n003F"))
            {
                var ff = new FixedFileFormat<HierarchyDataA>("A", 3, 2);
                var fr = new FileReader<HierarchyDataA>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(3, result.Records.Length);

                AssertFileRecord(1, "A", 0, false, result.Records[0]);
                AssertFileRecord(2, "B", 1, true, result.Records[1]);
                AssertFileRecord(3, "F", 1, false, result.Records[2]);

                Assert.AreEqual("Line Number is invalid; the value '3' does not match the read line number '2'.", result.Records[1].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.EndOfFile, result.Status);
                Assert.IsFalse(result.HasErrors);
            }
        }

        private const string AllGoodData = "X\ttrue\tQ\t31/12/1999\t32.95\t123.45\t-10\t-100\t-1000\t64\t-128\t-32.999\t99\t999\t9999";

        [Test]
        public void Read_ValueParsing_AllGood()
        {
            using (var sr = new StringReader(AllGoodData))
            {
                var ff = new DelimitedFileFormat<ColumnTypeData>(FileFormatBase.TabCharacter);
                var fr = new FileReader<ColumnTypeData>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);
                Assert.IsNotNull(result.Value);
                Assert.IsInstanceOf(typeof(ColumnTypeData), result.Value);

                Assert.AreEqual(15, result.Records[0].Columns.Length);
                Assert.AreEqual("X", result.Records[0].Columns[0]);
                Assert.AreEqual("true", result.Records[0].Columns[1]);
                Assert.AreEqual("Q", result.Records[0].Columns[2]);
                Assert.AreEqual("31/12/1999", result.Records[0].Columns[3]);
                Assert.AreEqual("32.95", result.Records[0].Columns[4]);
                Assert.AreEqual("123.45", result.Records[0].Columns[5]);
                Assert.AreEqual("-10", result.Records[0].Columns[6]);
                Assert.AreEqual("-100", result.Records[0].Columns[7]);
                Assert.AreEqual("-1000", result.Records[0].Columns[8]);
                Assert.AreEqual("64", result.Records[0].Columns[9]);
                Assert.AreEqual("-128", result.Records[0].Columns[10]);
                Assert.AreEqual("-32.999", result.Records[0].Columns[11]);
                Assert.AreEqual("99", result.Records[0].Columns[12]);
                Assert.AreEqual("999", result.Records[0].Columns[13]);
                Assert.AreEqual("9999", result.Records[0].Columns[14]);

                var val = (ColumnTypeData)result.Value;
                Assert.AreEqual("X", val.String);
                Assert.AreEqual(true, val.Boolean);
                Assert.AreEqual('Q', val.Char);
                Assert.AreEqual(new DateTime(1999, 12, 31), val.DateTime);
                Assert.AreEqual(32.95m, val.Decimal);
                Assert.AreEqual(123.45d, val.Double);
                Assert.AreEqual((short)-10, val.Int16);
                Assert.AreEqual(-100, val.Int32);
                Assert.AreEqual(-1000, val.Int64);
                Assert.AreEqual((byte)64, val.Byte);
                Assert.AreEqual((sbyte)-128, val.SByte);
                Assert.AreEqual(-32.999f, val.Single);
                Assert.AreEqual((ushort)99, val.UInt16);
                Assert.AreEqual(999u, val.UInt32);
                Assert.AreEqual(9999u, val.UInt64);
            }
        }

        [Test]
        public void Read_ValueParsing_AllGood_Volume()
        {
            var tot = 100000;
            var sb = new StringBuilder();
            for (int i = 0; i < tot; i++)
                sb.AppendLine(AllGoodData);

            using (var sr = new StringReader(sb.ToString()))
            {
                var ff = new DelimitedFileFormat<ColumnTypeData>(FileFormatBase.TabCharacter);
                var fr = new FileReader<ColumnTypeData>(sr, ff);
                Assert.AreEqual(tot, fr.ReadToEnd());
            }
        }

        [Test]
        public void Read_ValueParsing_Nullables_AllGood()
        {
            using (var sr = new StringReader("X\ttrue\tQ\t31/12/1999\t32.95\t123.45\t-10\t-100\t-1000\t64\t-128\t-32.999\t99\t999\t9999"))
            {
                var ff = new DelimitedFileFormat<ColumnNullableTypeData>(FileFormatBase.TabCharacter);
                var fr = new FileReader<ColumnNullableTypeData>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);
                Assert.IsNotNull(result.Value);
                Assert.IsInstanceOf(typeof(ColumnNullableTypeData), result.Value);

                Assert.AreEqual(15, result.Records[0].Columns.Length);
                Assert.AreEqual("X", result.Records[0].Columns[0]);
                Assert.AreEqual("true", result.Records[0].Columns[1]);
                Assert.AreEqual("Q", result.Records[0].Columns[2]);
                Assert.AreEqual("31/12/1999", result.Records[0].Columns[3]);
                Assert.AreEqual("32.95", result.Records[0].Columns[4]);
                Assert.AreEqual("123.45", result.Records[0].Columns[5]);
                Assert.AreEqual("-10", result.Records[0].Columns[6]);
                Assert.AreEqual("-100", result.Records[0].Columns[7]);
                Assert.AreEqual("-1000", result.Records[0].Columns[8]);
                Assert.AreEqual("64", result.Records[0].Columns[9]);
                Assert.AreEqual("-128", result.Records[0].Columns[10]);
                Assert.AreEqual("-32.999", result.Records[0].Columns[11]);
                Assert.AreEqual("99", result.Records[0].Columns[12]);
                Assert.AreEqual("999", result.Records[0].Columns[13]);
                Assert.AreEqual("9999", result.Records[0].Columns[14]);

                var val = (ColumnNullableTypeData)result.Value;
                Assert.AreEqual("X", val.String);
                Assert.AreEqual(true, val.Boolean);
                Assert.AreEqual('Q', val.Char);
                Assert.AreEqual(new DateTime(1999, 12, 31), val.DateTime);
                Assert.AreEqual(32.95m, val.Decimal);
                Assert.AreEqual(123.45d, val.Double);
                Assert.AreEqual((short)-10, val.Int16);
                Assert.AreEqual(-100, val.Int32);
                Assert.AreEqual(-1000, val.Int64);
                Assert.AreEqual((byte)64, val.Byte);
                Assert.AreEqual((sbyte)-128, val.SByte);
                Assert.AreEqual(-32.999f, val.Single);
                Assert.AreEqual((ushort)99, val.UInt16);
                Assert.AreEqual(999u, val.UInt32);
                Assert.AreEqual(9999u, val.UInt64);
            }
        }

        [Test]
        public void Read_ValueParsing_AllBad()
        {
            using (var sr = new StringReader("XX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX"))
            {
                var ff = new DelimitedFileFormat<ColumnNullableTypeData>(FileFormatBase.TabCharacter);
                var fr = new FileReader<ColumnNullableTypeData>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);
                Assert.IsNull(result.Value);
                Assert.AreEqual(14, result.Records[0].Messages.Count);

                Assert.AreEqual("Boolean is invalid; the value could not be parsed.", result.Records[0].Messages[0].Text);
                Assert.AreEqual("Char is invalid; the value could not be parsed.", result.Records[0].Messages[1].Text);
                Assert.AreEqual("DateTime is invalid; the value could not be parsed.", result.Records[0].Messages[2].Text);
                Assert.AreEqual("Decimal is invalid; the value could not be parsed.", result.Records[0].Messages[3].Text);
                Assert.AreEqual("Double is invalid; the value could not be parsed.", result.Records[0].Messages[4].Text);
                Assert.AreEqual("Int16 is invalid; the value could not be parsed.", result.Records[0].Messages[5].Text);
                Assert.AreEqual("Int32 is invalid; the value could not be parsed.", result.Records[0].Messages[6].Text);
                Assert.AreEqual("Int64 is invalid; the value could not be parsed.", result.Records[0].Messages[7].Text);
                Assert.AreEqual("Byte is invalid; the value could not be parsed.", result.Records[0].Messages[8].Text);
                Assert.AreEqual("SByte is invalid; the value could not be parsed.", result.Records[0].Messages[9].Text);
                Assert.AreEqual("Single is invalid; the value could not be parsed.", result.Records[0].Messages[10].Text);
                Assert.AreEqual("UInt16 is invalid; the value could not be parsed.", result.Records[0].Messages[11].Text);
                Assert.AreEqual("UInt32 is invalid; the value could not be parsed.", result.Records[0].Messages[12].Text);
                Assert.AreEqual("UInt64 is invalid; the value could not be parsed.", result.Records[0].Messages[13].Text);
            }
        }

        [Test]
        public void Read_ValueParsing_Nullables_AllBad()
        {
            using (var sr = new StringReader("XX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX\tXX"))
            {
                var ff = new DelimitedFileFormat<ColumnTypeData>(FileFormatBase.TabCharacter);
                var fr = new FileReader<ColumnTypeData>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);
                Assert.IsNull(result.Value);
                Assert.AreEqual(14, result.Records[0].Messages.Count);

                Assert.AreEqual("Boolean is invalid; the value could not be parsed.", result.Records[0].Messages[0].Text);
                Assert.AreEqual("Char is invalid; the value could not be parsed.", result.Records[0].Messages[1].Text);
                Assert.AreEqual("DateTime is invalid; the value could not be parsed.", result.Records[0].Messages[2].Text);
                Assert.AreEqual("Decimal is invalid; the value could not be parsed.", result.Records[0].Messages[3].Text);
                Assert.AreEqual("Double is invalid; the value could not be parsed.", result.Records[0].Messages[4].Text);
                Assert.AreEqual("Int16 is invalid; the value could not be parsed.", result.Records[0].Messages[5].Text);
                Assert.AreEqual("Int32 is invalid; the value could not be parsed.", result.Records[0].Messages[6].Text);
                Assert.AreEqual("Int64 is invalid; the value could not be parsed.", result.Records[0].Messages[7].Text);
                Assert.AreEqual("Byte is invalid; the value could not be parsed.", result.Records[0].Messages[8].Text);
                Assert.AreEqual("SByte is invalid; the value could not be parsed.", result.Records[0].Messages[9].Text);
                Assert.AreEqual("Single is invalid; the value could not be parsed.", result.Records[0].Messages[10].Text);
                Assert.AreEqual("UInt16 is invalid; the value could not be parsed.", result.Records[0].Messages[11].Text);
                Assert.AreEqual("UInt32 is invalid; the value could not be parsed.", result.Records[0].Messages[12].Text);
                Assert.AreEqual("UInt64 is invalid; the value could not be parsed.", result.Records[0].Messages[13].Text);
            }
        }

        [Test]
        public void Read_ColumnWidthOverflow()
        {
            using (var sr = new StringReader("ABCDEFGHIJKLMN\tABCDEFGHIJKL\tABCDEFGHIJKLM"))
            {
                var ff = new DelimitedFileFormat<Data>(FileFormatBase.TabCharacter);
                var fr = new FileReader<Data>(sr, ff);

                ff.WidthOverflow = ColumnWidthOverflow.Truncate;

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual(1, result.Records.Length);
                Assert.IsNull(result.Value);
                Assert.AreEqual(3, result.Records[0].Messages.Count);

                Assert.AreEqual("Col1 exceeded 10 characters in length; value was truncated.", result.Records[0].Messages[0].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[0].Type);
                Assert.AreEqual("Col2 must not exceed 10 characters in length.", result.Records[0].Messages[1].Text);
                Assert.AreEqual(MessageType.Error, result.Records[0].Messages[1].Type);
                Assert.AreEqual("Col3 exceeded 10 characters in length; value was truncated.", result.Records[0].Messages[2].Text);
                Assert.AreEqual(MessageType.Warning, result.Records[0].Messages[2].Type);

                Assert.IsNotNull(result.Records[0].Value);
                var val = (Data)result.Records[0].Value;
                Assert.AreEqual("ABCDEFGHIJ", val.Col1);
                Assert.IsNull(val.Col2);
                Assert.AreEqual("ABCDEFGHIJ", val.Col3);
            }
        }

        [Test]
        public void Enumerator_NoStopOnError_WithRecordReadEvent()
        {
            using (var sr = new StringReader("H\r\nD\r\nX\r\nD\r\nT"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                var fr = new FileReader<Data>(sr, ff);

                int hdr = 0;
                int trl = 0;
                int con = 0;
                int eof = 0;
                fr.RecordRead += (s, e) =>
                {
                    switch (e.OperationResult.Status)
                    {
                        case FileContentStatus.Header:
                            hdr++;
                            break;

                        case FileContentStatus.Trailer:
                            trl++;
                            break;

                        case FileContentStatus.Content:
                            con++;
                            Assert.AreEqual(con == 2, e.OperationResult.HasErrors);
                            break;

                        case FileContentStatus.EndOfFile:
                            eof++;
                            break;
                    }
                };

                int recs = 0;
                foreach (var rec in fr)
                {
                    Assert.IsNotNull(rec);
                    Assert.AreEqual("D", rec.Value.Col1);
                    recs++;
                }

                Assert.AreEqual(2, recs);
                Assert.AreEqual(1, hdr);
                Assert.AreEqual(1, trl);
                Assert.AreEqual(3, con);
                Assert.AreEqual(1, eof);
            }
        }

        [Test]
        public void Enumerator_StopOnError_WithRecordReadEvent()
        {
            using (var sr = new StringReader("H\r\nD\r\nX\r\nD\r\nT"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                var fr = new FileReader<Data>(sr, ff, stopOnError: true);

                int hdr = 0;
                int trl = 0;
                int con = 0;
                int eof = 0;
                fr.RecordRead += (s, e) =>
                {
                    switch (e.OperationResult.Status)
                    {
                        case FileContentStatus.Header:
                            hdr++;
                            break;

                        case FileContentStatus.Trailer:
                            trl++;
                            break;

                        case FileContentStatus.Content:
                            con++;
                            Assert.AreEqual(con == 2, e.OperationResult.HasErrors);
                            break;

                        case FileContentStatus.EndOfFile:
                            eof++;
                            break;
                    }
                };

                int recs = 0;
                foreach (var rec in fr)
                {
                    Assert.IsNotNull(rec);
                    Assert.AreEqual("D", rec.Value.Col1);
                    recs++;
                }

                Assert.AreEqual(1, recs);

                foreach (var rec in fr)
                {
                    Assert.IsNotNull(rec);
                    Assert.AreEqual("D", rec.Value.Col1);
                    recs++;
                }

                Assert.AreEqual(2, recs);
                Assert.AreEqual(1, hdr);
                Assert.AreEqual(1, trl);
                Assert.AreEqual(3, con);
                Assert.AreEqual(1, eof);
            }
        }

        [Test]
        public void ReadToEnd_WithRecordReadEvent()
        {
            using (var sr = new StringReader("H\r\nD\r\nX\r\nD\r\nT"))
            {
                var ff = new FixedFileFormat<Data>("D", 0, 1);
                ff.SetHeaderRowType<Header>("H");
                ff.SetTrailerRowType<Trailer>("T");
                var fr = new FileReader<Data>(sr, ff, stopOnError: true);

                int hdr = 0;
                int trl = 0;
                int con = 0;
                int eof = 0;
                fr.RecordRead += (s, e) =>
                {
                    switch (e.OperationResult.Status)
                    {
                        case FileContentStatus.Header:
                            hdr++;
                            break;

                        case FileContentStatus.Trailer:
                            trl++;
                            break;

                        case FileContentStatus.Content:
                            con++;
                            Assert.AreEqual(con == 2, e.OperationResult.HasErrors);
                            break;

                        case FileContentStatus.EndOfFile:
                            eof++;
                            break;
                    }
                };

                fr.ReadToEnd();

                Assert.AreEqual(1, hdr);
                Assert.AreEqual(1, trl);
                Assert.AreEqual(3, con);
                Assert.AreEqual(1, eof);
            }
        }

        public const string WithValidatorData = "John,Smith,19700508,Schmiddy\r\nBruce,,,\r\nKyle,Simpson,22000505\r\nCaleb,Brown,20100317";

        [Test]
        public void Read_WithValidator()
        {
            using (var sr = new StringReader(WithValidatorData))
            {
                var ff = new DelimitedFileFormat<Person>(contentValidator: PersonValidator.Default);
                ff.Converters.Add(new DateTimeConverter { FormatString = "yyyyMMdd" });
                var fr = new FileReader<Person>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                Assert.IsFalse(result.Records[0].HasMessages);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("Last Name is required.", result.Records[0].Messages[0].Text);
                Assert.AreEqual("Birthday is required.", result.Records[0].Messages[1].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("Birthday is invalid; age must be greater than 18.", result.Records[0].Messages[0].Text);

                result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsTrue(result.HasErrors);
                Assert.AreEqual("Birthday is invalid; age must be greater than 18.", result.Records[0].Messages[0].Text);
            }
        }

        [Test]
        public void Read_WithValidator_Volume()
        {
            var tot = 20000;
            var sb = new StringBuilder();
            for (int i = 0; i < tot; i++)
                sb.AppendLine(WithValidatorData);

            using (var sr = new StringReader(sb.ToString()))
            {
                var ff = new DelimitedFileFormat<Person>(contentValidator: PersonValidator.Default);
                ff.Converters.Add(new DateTimeConverter { FormatString = "yyyyMMdd" });
                var fr = new FileReader<Person>(sr, ff);
                Assert.AreEqual(tot * 4, fr.ReadToEnd());
            }
        }

        [Test]
        public void Read_OtherPropertyTypes()
        {
            using (var sr = new StringReader("12:30:59,123+456"))
            {
                var ff = new DelimitedFileFormat<DataWithOtherPropertyTypes>();
                ff.Converters.Add(new TimeSpanConverter());
                ff.Converters.Add(new LongLatConverter());
                var fr = new FileReader<DataWithOtherPropertyTypes>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);
                var val = (DataWithOtherPropertyTypes)result.Value;
                Assert.AreEqual(new TimeSpan(12, 30, 59), val.TimeSpan);
                Assert.AreEqual(123, val.LongLat.Longitude);
                Assert.AreEqual(456, val.LongLat.Latitude);
            }
        }
    }
}
