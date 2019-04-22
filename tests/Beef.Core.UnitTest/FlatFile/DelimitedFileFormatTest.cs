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
    public class DelimitedFileFormatTest
    {
        [Test]
        public void Ctor_Default()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(FileFormatBase.CommaCharater, dff.Delimiter);
            Assert.AreEqual(FileFormatBase.DoubleQuoteCharacter, dff.TextQualifier);
            Assert.IsNull(dff.HierarchyColumnIndex);
            Assert.AreEqual(TextQualifierHandling.Strict, dff.TextQualifierHandling);

            // FileFormat base configuration.
            Assert.AreEqual(FileValidation.Unspecified, dff.FileValidation);
            Assert.IsFalse(dff.IsHierarchical);
            Assert.AreEqual(typeof(Person), dff.ContentRowType);
            Assert.IsNull(dff.ContentRecordIdentifier);
            Assert.IsNull(dff.ContentValidator);
            Assert.IsNull(dff.HeaderRowType);
            Assert.IsNull(dff.HeaderRecordIdentifier);
            Assert.IsNull(dff.HeaderValidator);
            Assert.IsNull(dff.TrailerRowType);
            Assert.IsNull(dff.TrailerRecordIdentifier);
            Assert.IsNull(dff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, dff.StringTransform);
            Assert.AreEqual(StringTrim.End, dff.StringTrim);
        }

        [Test]
        public void Ctor_Delimiter_Override()
        {
            var dff = new DelimitedFileFormat<Person>('x', 'y', PersonValidator.Default);
            Assert.IsNotNull(dff);
            Assert.AreEqual('x', dff.Delimiter);
            Assert.AreEqual('y', dff.TextQualifier);
            Assert.IsNull(dff.HierarchyColumnIndex);
            Assert.AreEqual(TextQualifierHandling.Strict, dff.TextQualifierHandling);

            // FileFormat base configuration.
            Assert.AreEqual(FileValidation.Unspecified, dff.FileValidation);
            Assert.IsFalse(dff.IsHierarchical);
            Assert.AreEqual(typeof(Person), dff.ContentRowType);
            Assert.IsNull(dff.ContentRecordIdentifier);
            Assert.AreSame(PersonValidator.Default, dff.ContentValidator);
            Assert.IsNull(dff.HeaderRowType);
            Assert.IsNull(dff.HeaderRecordIdentifier);
            Assert.IsNull(dff.HeaderValidator);
            Assert.IsNull(dff.TrailerRowType);
            Assert.IsNull(dff.TrailerRecordIdentifier);
            Assert.IsNull(dff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, dff.StringTransform);
            Assert.AreEqual(StringTrim.End, dff.StringTrim);
        }

        [Test]
        public void Ctor_Delimiter_DelimiterMinValueException()
        {
            ExpectException.Throws<ArgumentException>("The delimiter can not be set to FileFormatBase.NoCharacter as a valid value is required.", () =>
                new DelimitedFileFormat<Person>(Char.MinValue, 'y', PersonValidator.Default));
        }

        [Test]
        public void Ctor_Delimiter_DelimiterSameException()
        {
            ExpectException.Throws<ArgumentException>("The delimiter and the text qualifier characters must not be the same.", () =>
                new DelimitedFileFormat<Person>(',', ',', PersonValidator.Default));
        }

        [Test]
        public void Ctor_Hierarchy_RecordIdentifierOnly()
        {
            var dff = new DelimitedFileFormat<Person>("X");
            Assert.IsNotNull(dff);
            Assert.AreEqual(',', dff.Delimiter);
            Assert.AreEqual('"', dff.TextQualifier);
            Assert.AreEqual(0, dff.HierarchyColumnIndex);

            // FileFormat base configuration.
            Assert.AreEqual(TextQualifierHandling.Strict, dff.TextQualifierHandling);
            Assert.AreEqual(FileValidation.Unspecified, dff.FileValidation);
            Assert.IsTrue(dff.IsHierarchical);
            Assert.AreEqual(typeof(Person), dff.ContentRowType);
            Assert.AreEqual("X", dff.ContentRecordIdentifier);
            Assert.IsNull(dff.ContentValidator);
            Assert.IsNull(dff.HeaderRowType);
            Assert.IsNull(dff.HeaderRecordIdentifier);
            Assert.IsNull(dff.HeaderValidator);
            Assert.IsNull(dff.TrailerRowType);
            Assert.IsNull(dff.TrailerRecordIdentifier);
            Assert.IsNull(dff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, dff.StringTransform);
            Assert.AreEqual(StringTrim.End, dff.StringTrim);
        }

        [Test]
        public void Ctor_Hierarchy_Override()
        {
            var dff = new DelimitedFileFormat<Person>("X", 1, 'x', 'y', PersonValidator.Default);
            Assert.IsNotNull(dff);
            Assert.AreEqual('x', dff.Delimiter);
            Assert.AreEqual('y', dff.TextQualifier);
            Assert.AreEqual(1, dff.HierarchyColumnIndex);

            // FileFormat base configuration.
            Assert.AreEqual(TextQualifierHandling.Strict, dff.TextQualifierHandling);
            Assert.AreEqual(FileValidation.Unspecified, dff.FileValidation);
            Assert.IsTrue(dff.IsHierarchical);
            Assert.AreEqual(typeof(Person), dff.ContentRowType);
            Assert.AreEqual("X", dff.ContentRecordIdentifier);
            Assert.AreSame(PersonValidator.Default, dff.ContentValidator);
            Assert.IsNull(dff.HeaderRowType);
            Assert.IsNull(dff.HeaderRecordIdentifier);
            Assert.IsNull(dff.HeaderValidator);
            Assert.IsNull(dff.TrailerRowType);
            Assert.IsNull(dff.TrailerRecordIdentifier);
            Assert.IsNull(dff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, dff.StringTransform);
            Assert.AreEqual(StringTrim.End, dff.StringTrim);
        }

        [Test]
        public void Ctor_Hierarchy_NullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DelimitedFileFormat<Person>(null));
        }

        [Test]
        public void Ctor_Hierarchy_IndexException()
        {
            ExpectException.Throws<ArgumentException>("Hierarchy column index must be zero or greater.",
                () => new DelimitedFileFormat<Person>("X", -1));
        }

        [Test]
        public void Ctor_Hierarchy_DelimiterMinValueException()
        {
            ExpectException.Throws<ArgumentException>("The delimiter can not be set to FileFormatBase.NoCharacter as a valid value is required.",
                () => new DelimitedFileFormat<Person>("X", 0, char.MinValue));
        }

        [Test]
        public void Ctor_Hierarchy_DelimiterSameException()
        {
            ExpectException.Throws<ArgumentException>("The delimiter and the text qualifier characters must not be the same.",
                () => new DelimitedFileFormat<Person>("X", 0, ',', ','));
        }

        [Test]
        public void Prop_TextQualifierHandling()
        {
            var dff = new DelimitedFileFormat<Person>();
            Assert.AreEqual(TextQualifierHandling.Strict, dff.TextQualifierHandling);
            dff.TextQualifierHandling = TextQualifierHandling.LooseAllow;
            Assert.AreEqual(TextQualifierHandling.LooseAllow, dff.TextQualifierHandling);
        }
    }
}
