// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile;
using Beef.Core.UnitTest.FlatFile.Entities;
using System;
using NUnit.Framework;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile
{
    [TestFixture]
    public class FileFormatTest
    {
        [Test]
        public void Prop_FileValidation()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(FileValidation.Unspecified, dff.FileValidation);

            dff.FileValidation = FileValidation.MustHaveHeaderRow | FileValidation.MustHaveTrailerRow;
            Assert.IsTrue(dff.FileValidation.HasFlag(FileValidation.MustHaveHeaderRow));
            Assert.IsTrue(dff.FileValidation.HasFlag(FileValidation.MustHaveTrailerRow));
            Assert.IsFalse(dff.FileValidation.HasFlag(FileValidation.MustHaveAtLeastOneContentRow));
            Assert.IsFalse(dff.FileValidation.HasFlag(FileValidation.MustHaveRows));
            Assert.AreEqual(TextQualifierHandling.Strict, dff.TextQualifierHandling);
        }

        #region SetHeaderRowType

        [Test]
        public void SetHeaderRowType_Default()
        {
            var dff = new DelimitedFileFormat<Person>();
            dff.SetHeaderRowType<Header>();
            Assert.AreEqual(typeof(Header), dff.HeaderRowType);
            Assert.IsNull(dff.HeaderRecordIdentifier);
            Assert.IsNull(dff.HeaderValidator);
        }

        [Test]
        public void SetHeaderRowType_Override()
        {
            var dff = new DelimitedFileFormat<Person>("X");
            dff.SetHeaderRowType<Header>("H", HeaderValidator.Default);
            Assert.AreEqual(typeof(Header), dff.HeaderRowType);
            Assert.AreEqual("H", dff.HeaderRecordIdentifier);
            Assert.AreSame(HeaderValidator.Default, dff.HeaderValidator);
        }

        [Test]
        public void SetHeaderRowType_AlreadySetException()
        {
            ExpectException.Throws<InvalidOperationException>("The HeaderRowType cannot be set more than once.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetHeaderRowType<Header>("H", HeaderValidator.Default);
                dff.SetHeaderRowType<Header>("H", HeaderValidator.Default);
            });
        }

        [Test]
        public void SetHeaderRowType_SameAsContentException()
        {
            ExpectException.Throws<ArgumentException>("The HeaderRowType cannot be the same as the ContentRowType.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetHeaderRowType<Person>("H");
            });
        }


        [Test]
        public void SetHeaderRowType_SameAsTrailerException()
        {
            ExpectException.Throws<ArgumentException>("The HeaderRowType cannot be the same as the TrailerRowType.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetTrailerRowType<Trailer>("T");
                dff.SetHeaderRowType<Trailer>("H");
            });
        }

        [Test]
        public void SetHeaderRowType_NoRecIdException()
        {
            ExpectException.Throws<ArgumentNullException>("The record identifier is required where the file is considered hierarchical.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetHeaderRowType<Header>();
            });
        }

        [Test]
        public void SetHeaderRowType_NoHierarchyException()
        {
            ExpectException.Throws<ArgumentException>("The record identifier can not be specified where the file is not considered hierarchical.", () =>
            {
                var dff = new DelimitedFileFormat<Person>();
                dff.SetHeaderRowType<Header>("H");
            });
        }

        [Test]
        public void SetHeaderRowType_SameContentRecIdException()
        {
            ExpectException.Throws<ArgumentException>("The HeaderRecordIdentifier cannot be the same as the ContentRecordIdentifier.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetHeaderRowType<Header>("X");
            });
        }

        [Test]
        public void SetHeaderRowType_SameTrailerRecIdException()
        {
            ExpectException.Throws<ArgumentException>("The HeaderRecordIdentifier cannot be the same as the TrailerRecordIdentifier.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetTrailerRowType<Trailer>("T");
                dff.SetHeaderRowType<Header>("T");
                Assert.Fail();
            });
        }

        #endregion

        #region SetTrailerRowType

        [Test]
        public void SetTrailerRowType_Default()
        {
            var dff = new DelimitedFileFormat<Person>();
            dff.SetTrailerRowType<Trailer>();
            Assert.AreEqual(typeof(Trailer), dff.TrailerRowType);
            Assert.IsNull(dff.TrailerRecordIdentifier);
            Assert.IsNull(dff.TrailerValidator);
        }

        [Test]
        public void SetTrailierRowType_Override()
        {
            var dff = new DelimitedFileFormat<Person>("X");
            dff.SetTrailerRowType<Trailer>("H", TrailerValidator.Default);
            Assert.AreEqual(typeof(Trailer), dff.TrailerRowType);
            Assert.AreEqual("H", dff.TrailerRecordIdentifier);
            Assert.AreSame(TrailerValidator.Default, dff.TrailerValidator);
        }

        [Test]
        public void SetTrailerRowType_AlreadySetException()
        {
            ExpectException.Throws<InvalidOperationException>("The TrailerRowType cannot be set more than once.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetTrailerRowType<Trailer>("T", TrailerValidator.Default);
                dff.SetTrailerRowType<Trailer>("T", TrailerValidator.Default);
            });
        }

        [Test]
        public void SetTrailerRowType_SameAsContentException()
        {
            ExpectException.Throws<InvalidOperationException>("The TrailerRowType cannot be the same as the ContentRowType.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetTrailerRowType<Person>("T");
            });
        }


        [Test]
        public void SetTrailerRowType_SameAsHeaderException()
        {
            ExpectException.Throws<InvalidOperationException>("The TrailerRowType cannot be the same as the HeaderRowType.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetHeaderRowType<Header>("H");
                dff.SetTrailerRowType<Header>("T");
            });
        }

        [Test]
        public void SetTrailerRowType_NoRecIdException()
        {
            ExpectException.Throws<ArgumentNullException>("The record identifier is required where the file is considered hierarchical.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetTrailerRowType<Trailer>();
            });
        }

        [Test]
        public void SetTrailerRowType_NoHierarchyException()
        {
            ExpectException.Throws<ArgumentException>("The record identifier can not be specified where the file is not considered hierarchical.", () =>
            {
                var dff = new DelimitedFileFormat<Person>();
                dff.SetTrailerRowType<Trailer>("T");
            });
        }

        [Test]
        public void SetTrailerRowType_SameContentRecIdException()
        {
            ExpectException.Throws<ArgumentException>("The TrailerRecordIdentifier cannot be the same as the ContentRecordIdentifier.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetTrailerRowType<Trailer>("X");
            });
        }

        [Test]
        public void SetTrailerRowType_SameHeaderRecIdException()
        {
            ExpectException.Throws<ArgumentException>("The TrailerRecordIdentifier cannot be the same as the HeaderRecordIdentifier.", () =>
            {
                var dff = new DelimitedFileFormat<Person>("X");
                dff.SetHeaderRowType<Header>("H");
                dff.SetTrailerRowType<Trailer>("H");
            });
        }

        #endregion

        [Test]
        public void Prop_WidthOverflow()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(ColumnWidthOverflow.Error, dff.WidthOverflow);
            dff.WidthOverflow = ColumnWidthOverflow.Truncate;
            Assert.AreEqual(ColumnWidthOverflow.Truncate, dff.WidthOverflow);
        }

        [Test]
        public void Prop_StringTransform()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(StringTransform.EmptyToNull, dff.StringTransform);
            dff.StringTransform = StringTransform.None;
            Assert.AreEqual(StringTransform.None, dff.StringTransform);
        }

        [Test]
        public void Prop_StringTrim()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(StringTrim.End, dff.StringTrim);
            dff.StringTrim = StringTrim.Both;
            Assert.AreEqual(StringTrim.Both, dff.StringTrim);
        }

        [Test]
        public void Prop_Converters()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.IsNotNull(dff.Converters);
        }

        [Test]
        public void Prop_ColumnCountValidation()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(ColumnCountValidation.None, dff.ColumnCountValidation);
            dff.ColumnCountValidation = ColumnCountValidation.LessAndGreaterThanError;
            Assert.AreEqual(ColumnCountValidation.LessAndGreaterThanError, dff.ColumnCountValidation);
        }
    }
}
