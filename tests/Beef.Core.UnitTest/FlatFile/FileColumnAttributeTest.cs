// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using Beef.Entities;
using Beef.Core.UnitTest.FlatFile.Entities;
using System;
using System.IO;
using NUnit.Framework;
using Beef.Test.NUnit;

namespace Beef.Core.UnitTest.FlatFile
{
    [TestFixture]
    public class FileColumnAttributeTest
    {
         [Test]
        public void Ctor_Default()
        {
            var fca = new FileColumnAttribute();
            Assert.IsNull(fca.Name);
            Assert.IsNull(fca.Text);
            Assert.AreEqual(-1, fca.Order);
            Assert.AreEqual(0, fca.Width);
            Assert.AreEqual(ColumnWidthOverflow.Error, fca.WidthOverflow);
            Assert.IsFalse(fca.HasWidthOverflowBeenSet);
            Assert.IsFalse(fca.IsMandatory);
            Assert.IsFalse(fca.IsLineNumber);
            Assert.AreEqual(StringTrim.End, fca.StringTrim);
            Assert.IsFalse(fca.HasStringTrimBeenSet);
            Assert.AreEqual(StringTransform.EmptyToNull, fca.StringTransform);
            Assert.IsFalse(fca.HasStringTransformBeenSet);
            Assert.IsNull(fca.TextValueConverterKey);
            Assert.IsNull(fca.WriteFormatString);
        }

        [Test]
        public void Ctor_WithProperties()
        {
            var fca = new FileColumnAttribute()
            {
                Name = "Name",
                Text = "Text",
                Order = 1,
                Width = 2,
                IsMandatory = true,
                IsLineNumber = true,
                TextValueConverterKey = "Key",
                WriteFormatString = "000"
            };

            Assert.AreEqual("Name", fca.Name);
            Assert.AreEqual("Text", fca.Text);
            Assert.AreEqual(1, fca.Order);
            Assert.AreEqual(2, fca.Width);
            Assert.AreEqual(ColumnWidthOverflow.Error, fca.WidthOverflow);
            Assert.IsFalse(fca.HasWidthOverflowBeenSet);
            Assert.IsTrue(fca.IsMandatory);
            Assert.IsTrue(fca.IsLineNumber);
            Assert.AreEqual(StringTrim.End, fca.StringTrim);
            Assert.IsFalse(fca.HasStringTrimBeenSet);
            Assert.AreEqual(StringTransform.EmptyToNull, fca.StringTransform);
            Assert.IsFalse(fca.HasStringTransformBeenSet);
            Assert.AreEqual("Key", fca.TextValueConverterKey);
            Assert.AreEqual("000", fca.WriteFormatString);
        }

        [Test]
        public void Prop_WidthOverflow()
        {
            var fca = new FileColumnAttribute();
            Assert.AreEqual(ColumnWidthOverflow.Error, fca.WidthOverflow);
            Assert.IsFalse(fca.HasWidthOverflowBeenSet);

            fca.WidthOverflow = ColumnWidthOverflow.Truncate;
            Assert.AreEqual(ColumnWidthOverflow.Truncate, fca.WidthOverflow);
            Assert.IsTrue(fca.HasWidthOverflowBeenSet);
        }

        [Test]
        public void Prop_StringTrim()
        {
            var fca = new FileColumnAttribute();
            Assert.AreEqual(StringTrim.End, fca.StringTrim);
            Assert.IsFalse(fca.HasStringTrimBeenSet);

            fca.StringTrim = StringTrim.Both;
            Assert.AreEqual(StringTrim.Both, fca.StringTrim);
            Assert.IsTrue(fca.HasStringTrimBeenSet);
        }

        [Test]
        public void Prop_StringTransform()
        {
            var fca = new FileColumnAttribute();
            Assert.AreEqual(StringTransform.EmptyToNull, fca.StringTransform);
            Assert.IsFalse(fca.HasStringTransformBeenSet);

            fca.StringTransform = StringTransform.None;
            Assert.AreEqual(StringTransform.None, fca.StringTransform);
            Assert.IsTrue(fca.HasStringTransformBeenSet);
        }

        [Test]
        public void Execute_BothColumnAndHierarchyAttributesException()
        {
            ExpectException.Throws<InvalidOperationException>("Type 'BothColumnAndHierarchyAttributes' property 'Data' cannot specify both a FileColumnAttribute and FileHierarchyAttribute.", () =>
            {
                using (var sr = new StringReader("X"))
                {
                    var ff = new DelimitedFileFormat<BothColumnAndHierarchyAttributes>(FileFormatBase.TabCharacter);
                    var fr = new FileReader<BothColumnAndHierarchyAttributes>(sr, ff);

                    var result = fr.Read();
                }
            });
        }

        [Test]
        public void Execute_DuplicateRecordIdentifierException()
        {
            ExpectException.Throws<InvalidOperationException>("Type 'DuplicateRecordIdentifier' property 'C' FileHierarchyAttribute has a duplicate Record Identifier 'A' (must be unique within Type).", () =>
            {
                using (var sr = new StringReader("X"))
                {
                    var ff = new DelimitedFileFormat<DuplicateRecordIdentifier>(FileFormatBase.TabCharacter);
                    var fr = new FileReader<DuplicateRecordIdentifier>(sr, ff);

                    var result = fr.Read();
                }
            });
        }

        [Test]
        public void Execute_IsLineNumberNotIntegerException()
        {
            ExpectException.Throws<InvalidOperationException>("FileColumnAttribute has IsLineNumber set to true; the underlying property type must be either Int32 or Int64.", () =>
            {
                using (var sr = new StringReader("X"))
                {
                    var ff = new DelimitedFileFormat<IsLineNumberNotInteger>(FileFormatBase.TabCharacter);
                    var fr = new FileReader<IsLineNumberNotInteger>(sr, ff);

                    var result = fr.Read();
                    Assert.Fail();
                }
            });
        }

        [Test]
        public void Execute_StringTrimTransform_DelimitedFormat()
        {
            using (var sr = new StringReader(" ABC \t ABC \t"))
            {
                var ff = new DelimitedFileFormat<Data>(FileFormatBase.TabCharacter);
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);

                var val = (Data)result.Value;
                Assert.AreEqual(" ABC", val.Col1);
                Assert.AreEqual(" ABC ", val.Col2);
                Assert.AreEqual(string.Empty, val.Col3);
            }
        }

        [Test]
        public void Execute_StringTrimTransform_FixedFormat()
        {
            // Column separators:             <         <         <
            using (var sr = new StringReader(" ABC       ABC      "))
            {
                var ff = new FixedFileFormat<Data>();
                var fr = new FileReader<Data>(sr, ff);

                var result = fr.Read();
                Assert.IsNotNull(result);
                Assert.AreEqual(FileContentStatus.Content, result.Status);
                Assert.IsFalse(result.HasErrors);

                var val = (Data)result.Value;
                Assert.AreEqual(" ABC", val.Col1);
                Assert.AreEqual(" ABC      ", val.Col2);
                Assert.AreEqual(string.Empty, val.Col3);
            }
        }
    }
}
