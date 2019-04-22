// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile.Converters;
using System;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the attribute for defining a file column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FileColumnAttribute : Attribute
    {
        private ColumnWidthOverflow? _widthOverflow;
        private StringTransform? _stringTransform;
        private StringTrim? _stringTrim;

        /// <summary>
        /// Gets or sets the column name; where not specified the property name will be the default.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the column text used for messages; where not specified the property name (converted to sentence case) will be used as the default.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the column order (defaults to -1).
        /// </summary>
        /// <remarks>Where not specified (less than zero) will default to the order defined within the class (after those that have been specified with an Order).</remarks>
        public int Order { get; set; } = -1;

        /// <summary>
        /// Gets or sets the column width (defaults to zero).
        /// </summary>
        /// <remarks>Where not specified (zero or less) this indicates an unspecified width.</remarks>
        public int Width { get; set; } = 0;

        /// <summary>
        /// Indicates whether the column is mandatory; i.e. value must have a non-empty value (defaults to <c>false</c>).
        /// </summary>
        public bool IsMandatory { get; set; } = false;

        /// <summary>
        /// Indicates whether the column is a line number (either automatically checked for a read or updated for a write).
        /// </summary>
        /// <remarks>The underlying column type must be either <see cref="Int32"/> or <see cref="Int64"/>.</remarks>
        public bool IsLineNumber { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="ColumnWidthOverflow"/> when reading/writing to/from file.
        /// </summary>
        /// <remarks>See <see cref="HasWidthOverflowBeenSet"/> to determine whether the value has been specifically set;
        /// this allows the default (<see cref="FileFormatBase.WidthOverflow"/>) to be used where not set.</remarks>
        public ColumnWidthOverflow WidthOverflow
        {
            get { return _widthOverflow ?? ColumnWidthOverflow.Error; }
            set { _widthOverflow = value; }
        }

        /// <summary>
        /// Indicates whether the <see cref="WidthOverflow"/> value has been set/updated.
        /// </summary>
        public bool HasWidthOverflowBeenSet
        {
            get { return _widthOverflow.HasValue; }
        }

        /// <summary>
        /// Gets or sets the value <see cref="StringTransform"/> when reading/writing to/from file.
        /// </summary>
        /// <remarks>See <see cref="HasStringTransformBeenSet"/> to determine whether the value has been specifically set;
        /// this allows the default (<see cref="FileFormatBase.StringTransform"/>) to be used where not set.</remarks>
        public StringTransform StringTransform
        {
            get { return _stringTransform ?? StringTransform.EmptyToNull; }
            set { _stringTransform = value; }
        }

        /// <summary>
        /// Indicates whether the <see cref="StringTransform"/> value has been set/updated.
        /// </summary>
        public bool HasStringTransformBeenSet
        {
            get { return _stringTransform.HasValue; }
        }

        /// <summary>
        /// Gets or sets the value <see cref="StringTrim"/> when reading/writing to/from file.
        /// </summary>
        /// <remarks>See <see cref="HasStringTrimBeenSet"/> to determine whether the value has been specifically set;
        /// this allows the default (<see cref="FileFormatBase.StringTrim"/>) to be used where not set.</remarks>
        public StringTrim StringTrim
        {
            get { return _stringTrim ?? StringTrim.End; }
            set { _stringTrim = value; }
        }

        /// <summary>
        /// Indicates whether the <see cref="StringTrim"/> value has been set/updated.
        /// </summary>
        public bool HasStringTrimBeenSet
        {
            get { return _stringTrim.HasValue; }
        }

        /// <summary>
        /// Gets or sets the key to retrieve the text value converter (see <see cref="ITextValueConverter"/>) used for both file read and write.
        /// </summary>
        public string TextValueConverterKey { get; set; }

        /// <summary>
        /// Gets or sets the text value converter (see <see cref="ITextValueConverter"/>) <see cref="Type"/> (must have a default constructor).
        /// </summary>
        public Type TextValueConverterType { get; set; }

        /// <summary>
        /// Gets or sets the write (output) format string (overrides any default converters and specified <see cref="TextValueConverterKey"/>).
        /// </summary>
        public string WriteFormatString { get; set; }
    }
}
