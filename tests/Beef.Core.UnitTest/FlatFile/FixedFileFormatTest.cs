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
    public class FixedFileFormatTest
    {
        [Test]
        public void Ctor_Default()
        {
            var fff = new FixedFileFormat<Person>();
            Assert.IsNull(fff.HierarchyColumnPosition);
            Assert.IsNull(fff.HierarchyColumnLength);
            Assert.AreEqual(FileFormatBase.SpaceCharacter, fff.PaddingChar);

            // FileFormat base configuration.
            Assert.AreEqual(FileValidation.Unspecified, fff.FileValidation);
            Assert.IsFalse(fff.IsHierarchical);
            Assert.AreEqual(typeof(Person), fff.ContentRowType);
            Assert.IsNull(fff.ContentRecordIdentifier);
            Assert.IsNull(fff.ContentValidator);
            Assert.IsNull(fff.HeaderRowType);
            Assert.IsNull(fff.HeaderRecordIdentifier);
            Assert.IsNull(fff.HeaderValidator);
            Assert.IsNull(fff.TrailerRowType);
            Assert.IsNull(fff.TrailerRecordIdentifier);
            Assert.IsNull(fff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, fff.StringTransform);
            Assert.AreEqual(StringTrim.End, fff.StringTrim);
        }

        [Test]
        public void Ctor_ContentValidator()
        {
            var fff = new FixedFileFormat<Person>(PersonValidator.Default);
            Assert.IsNull(fff.HierarchyColumnPosition);
            Assert.IsNull(fff.HierarchyColumnLength);
            Assert.AreEqual(FileFormatBase.SpaceCharacter, fff.PaddingChar);

            // FileFormat base configuration.
            Assert.AreEqual(FileValidation.Unspecified, fff.FileValidation);
            Assert.IsFalse(fff.IsHierarchical);
            Assert.AreEqual(typeof(Person), fff.ContentRowType);
            Assert.IsNull(fff.ContentRecordIdentifier);
            Assert.AreSame(PersonValidator.Default, fff.ContentValidator);
            Assert.IsNull(fff.HeaderRowType);
            Assert.IsNull(fff.HeaderRecordIdentifier);
            Assert.IsNull(fff.HeaderValidator);
            Assert.IsNull(fff.TrailerRowType);
            Assert.IsNull(fff.TrailerRecordIdentifier);
            Assert.IsNull(fff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, fff.StringTransform);
            Assert.AreEqual(StringTrim.End, fff.StringTrim);
        }

        [Test]
        public void Ctor_Hierarchy_NoContentValidator()
        {
            var fff = new FixedFileFormat<Person>("X", 0, 1);
            Assert.AreEqual(0, fff.HierarchyColumnPosition);
            Assert.AreEqual(1, fff.HierarchyColumnLength);
            Assert.AreEqual(FileFormatBase.SpaceCharacter, fff.PaddingChar);

            // FileFormat base configuration.
            Assert.AreEqual(FileValidation.Unspecified, fff.FileValidation);
            Assert.IsTrue(fff.IsHierarchical);
            Assert.AreEqual(typeof(Person), fff.ContentRowType);
            Assert.AreEqual("X", fff.ContentRecordIdentifier);
            Assert.IsNull(fff.ContentValidator);
            Assert.IsNull(fff.HeaderRowType);
            Assert.IsNull(fff.HeaderRecordIdentifier);
            Assert.IsNull(fff.HeaderValidator);
            Assert.IsNull(fff.TrailerRowType);
            Assert.IsNull(fff.TrailerRecordIdentifier);
            Assert.IsNull(fff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, fff.StringTransform);
            Assert.AreEqual(StringTrim.End, fff.StringTrim);
        }

        [Test]
        public void Ctor_Hierarchy_ContentValidator()
        {
            var fff = new FixedFileFormat<Person>("X", 0, 1, PersonValidator.Default);
            Assert.AreEqual(0, fff.HierarchyColumnPosition);
            Assert.AreEqual(1, fff.HierarchyColumnLength);
            Assert.AreEqual(FileFormatBase.SpaceCharacter, fff.PaddingChar);

            // FileFormat base configuration.
            Assert.AreEqual(FileValidation.Unspecified, fff.FileValidation);
            Assert.IsTrue(fff.IsHierarchical);
            Assert.AreEqual(typeof(Person), fff.ContentRowType);
            Assert.AreEqual("X", fff.ContentRecordIdentifier);
            Assert.AreSame(PersonValidator.Default, fff.ContentValidator);
            Assert.IsNull(fff.HeaderRowType);
            Assert.IsNull(fff.HeaderRecordIdentifier);
            Assert.IsNull(fff.HeaderValidator);
            Assert.IsNull(fff.TrailerRowType);
            Assert.IsNull(fff.TrailerRecordIdentifier);
            Assert.IsNull(fff.TrailerValidator);
            Assert.AreEqual(StringTransform.EmptyToNull, fff.StringTransform);
            Assert.AreEqual(StringTrim.End, fff.StringTrim);
        }

        [Test]
        public void Ctor_Hierarchy_ColumnPositionNegativeException()
        {
            ExpectException.Throws<ArgumentException>("Hierarchy column position must be greater than or equal to zero.",
                () => new FixedFileFormat<Person>("X", -1, 1, PersonValidator.Default));
        }

        [Test]
        public void Ctor_Hierarchy_ColumnLengthZeroException()
        {
            ExpectException.Throws<ArgumentException>("Hierarchy column length must be greater than or equal to one.",
                () => new FixedFileFormat<Person>("X", 0, 0, PersonValidator.Default));
        }

        [Test]
        public void Ctor_Hierarchy_ColumnLengthNegativeException()
        {
            ExpectException.Throws<ArgumentException>("Hierarchy column length must be greater than or equal to one.", 
                () => new FixedFileFormat<Person>("X", 0, -1, PersonValidator.Default));
        }

        [Test]
        public void Prop_PaddingChar()
        {
            var fff = new FixedFileFormat<Person>();
            Assert.AreEqual(FileFormatBase.SpaceCharacter, fff.PaddingChar);
            fff.PaddingChar = '*';
            Assert.AreEqual('*', fff.PaddingChar);
        }
    }
}
