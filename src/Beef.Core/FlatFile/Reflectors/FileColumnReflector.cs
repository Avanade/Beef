// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile.Converters;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Beef.FlatFile.Reflectors
{
    /// <summary>
    /// Represents a file column reflector.
    /// </summary>
    public sealed class FileColumnReflector
    {
        private string _text;
        private readonly ITextValueConverter _valueConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileColumnReflector"/> class.
        /// </summary>
        /// <param name="index">The <see cref="Index"/>.</param>
        /// <param name="fc">The <see cref="FileColumn"/>.</param>
        /// <param name="pi">The <see cref="PropertyInfo"/>.</param>
        /// <param name="ff">The <see cref="FileFormat"/>.</param>
        internal FileColumnReflector(int index, FileColumnAttribute fc, PropertyInfo pi, FileFormatBase ff)
        {
            Index = index;
            Order = fc.Order < 0 ? int.MaxValue : fc.Order;
            Name = string.IsNullOrEmpty(fc.Name) ? pi.Name : fc.Name;
            FileColumn = fc;
            PropertyInfo = pi;
            FileFormat = ff;

            // Get the property type being mindful of nullables.
            if (PropertyInfo.PropertyType.GetTypeInfo().IsGenericType && PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                PropertyTypeCode = GetTypeCode(PropertyInfo.PropertyType.GetGenericArguments()[0]);
                PropertyIsNullable = true;
            }
            else
                PropertyTypeCode = GetTypeCode(PropertyInfo.PropertyType);

            _valueConverter = FileFormat.Converters.Get(PropertyInfo.PropertyType, FileColumn.TextValueConverterKey, FileColumn.TextValueConverterType);
            if (!string.IsNullOrEmpty(FileColumn.TextValueConverterKey) && _valueConverter == null)
                throw new InvalidOperationException($"FileColumnAttribute has TextValueConverterKey of '{FileColumn.TextValueConverterKey}' is not found within the FileFormat.Converters.");

            if (FileColumn.IsLineNumber && PropertyTypeCode != TypeCode.Int32 && PropertyTypeCode != TypeCode.Int64)
                throw new InvalidOperationException("FileColumnAttribute has IsLineNumber set to true; the underlying property type must be either Int32 or Int64.");
        }

        /// <summary>
        /// Gets the <see cref="TypeCode"/> for the <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The <see cref="TypeCode"/>.</returns>
        public static TypeCode GetTypeCode(Type type)
        {
            if (type == null)
                return TypeCode.Empty;

            if (type == typeof(string))
                return TypeCode.String;
            else if (type == typeof(Int16))
                return TypeCode.Int16;
            else if (type == typeof(Int32))
                return TypeCode.Int32;
            else if (type == typeof(Int64))
                return TypeCode.Int64;
            else if (type == typeof(Boolean))
                return TypeCode.Boolean;
            else if (type == typeof(DateTime))
                return TypeCode.DateTime;
            else if (type == typeof(Single))
                return TypeCode.Single;
            else if (type == typeof(Decimal))
                return TypeCode.Decimal;
            else if (type == typeof(Double))
                return TypeCode.Double;
            else if (type == typeof(Byte))
                return TypeCode.Byte;
            else if (type == typeof(SByte))
                return TypeCode.SByte;
            else if (type == typeof(Char))
                return TypeCode.Char;
            else if (type == typeof(UInt16))
                return TypeCode.UInt16;
            else if (type == typeof(UInt32))
                return TypeCode.UInt32;
            else if (type == typeof(UInt64))
                return TypeCode.UInt64;
            else
                return TypeCode.Object;
        }

        /// <summary>
        /// Gets the index (sequence) within the defining class.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets the specified order.
        /// </summary>
        /// <remarks>Uses <see cref="FileColumnAttribute"/> <see cref="FileColumnAttribute.Order"/> where greater than or equal to zero; otherwise, negative value default
        /// to <see cref="int.MaxValue"/> so that they appear after any specified value (in <see cref="Index"/> order).</remarks>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the column name.
        /// </summary>
        /// <remarks>The name wil be updated using the following logic until a non-null value is found; a) <see cref="FileColumnAttribute"/> <see cref="FileColumnAttribute.Name"/>,
        /// then b) the property name.</remarks>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the column text.
        /// </summary>
        /// <remarks>The text will be updated using the following logic until a non-null value is found; a) <see cref="FileColumnAttribute"/> <see cref="FileColumnAttribute.Text"/>,
        /// b) corresponding <see cref="DisplayAttribute"/> <see cref="DisplayAttribute.Name"/>, then c) a sentence case version of the property name.</remarks>
        public string Text
        {
            get
            {
                // Lazy generate to minimise performance impact.
                if (_text != null)
                    return _text;

                if (!string.IsNullOrEmpty(FileColumn.Text))
                    _text = FileColumn.Text;
                else
                {
                    // Either get the friendly text from a corresponding DisplayTextAttribute or split the PascalCase member name into friendlier sentence case text.
                    DisplayAttribute ca = PropertyInfo.GetCustomAttribute<DisplayAttribute>(true);
                    _text = ca == null ? CodeGen.CodeGenerator.ToSentenceCase(PropertyInfo.Name) : ca.Name;
                }

                return _text;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileColumnAttribute"/>.
        /// </summary>
        public FileColumnAttribute FileColumn { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Reflection.PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the property <see cref="TypeCode"/>.
        /// </summary>
        public TypeCode PropertyTypeCode { get; private set; }

        /// <summary>
        /// Indicates whether the property is declared as <see cref="Nullable{T}"/>.
        /// </summary>
        public bool PropertyIsNullable { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileFormatBase"/>.
        /// </summary>
        public FileFormatBase FileFormat { get; private set; }

        /// <summary>
        /// Sets the property value with the specified <see cref="string"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/> (<see cref="FileRecord.Messages"/> will be added).</param>
        /// <param name="str">The <see cref="string"/> from the file record.</param>
        /// <param name="value">The object whose property value will be set.</param>
        /// <returns>Indicates whether the value was updated successfully.</returns>
        public bool SetValue(FileRecord record, string str, object value)
        {
            Check.NotNull(record, nameof(record));

            if (FileColumn.IsMandatory && (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str)))
                return CreateValueError(record, "{0} is required.", str);

            if (!StringWidthCorrector(record, ref str))
                return false;

            object val = null;
            if (PropertyIsNullable && string.IsNullOrEmpty(str))
            { }
            else
            {

                if (_valueConverter != null)
                {
                    if (!_valueConverter.TryParse(str, out val))
                        return CreateValueError(record, "{0} is invalid; the value could not be parsed.", str);
                }
                else if (!TryNativeParse(str, out val))
                    return CreateValueError(record, "{0} is invalid; the value could not be parsed.", str);
            }

            var hasError = false;
            if (FileColumn.IsLineNumber)
            {
                long lineNumber;
                if (PropertyTypeCode == TypeCode.Int32)
                    lineNumber = (int)val;
                else
                    lineNumber = (long)val;

                if (record.LineNumber.CompareTo(lineNumber) != 0)
                    hasError = CreateValueError(record, "{0} is invalid; the value '{2}' does not match the read line number '{3}'.", str, val, record.LineNumber);
            }

            if (val != null)
                PropertyInfo.SetValue(value, val);

            return hasError;
        }

        /// <summary>
        /// Create the error message for the set value.
        /// </summary>
        private bool CreateValueError(FileRecord record, string format, string str, params object[] values)
        {
            CreateErrorMessage(record, format, str, values);
            return false;
        }

        /// <summary>
        /// Execute the native parser for the type.
        /// </summary>
        private bool TryNativeParse(string str, out object value)
        {
            value = null;

            switch (PropertyTypeCode)
            {
                case TypeCode.String:
                    value = str;
                    return true;

                case TypeCode.Boolean:
                    bool bval;
                    if (!Boolean.TryParse(str, out bval))
                        return false;

                    value = bval;
                    return true;

                case TypeCode.Char:
                    char cval;
                    if (!Char.TryParse(str, out cval))
                        return false;

                    value = cval;
                    return true;

                case TypeCode.DateTime:
                    DateTime dtval;
                    if (!DateTime.TryParse(str, out dtval))
                        return false;

                    value = dtval;
                    return true;

                case TypeCode.Decimal:
                    decimal mval;
                    if (!Decimal.TryParse(str, out mval))
                        return false;

                    value = mval;
                    return true;

                case TypeCode.Double:
                    double dval;
                    if (!Double.TryParse(str, out dval))
                        return false;

                    value = dval;
                    return true;

                case TypeCode.Int16:
                    short i16val;
                    if (!Int16.TryParse(str, out i16val))
                        return false;

                    value = i16val;
                    return true;

                case TypeCode.Int32:
                    int i32val;
                    if (!Int32.TryParse(str, out i32val))
                        return false;

                    value = i32val;
                    return true;

                case TypeCode.Int64:
                    long i64val;
                    if (!Int64.TryParse(str, out i64val))
                        return false;

                    value = i64val;
                    return true;

                case TypeCode.Byte:
                    byte byval;
                    if (!Byte.TryParse(str, out byval))
                        return false;

                    value = byval;
                    return true;

                case TypeCode.SByte:
                    sbyte sbyval;
                    if (!SByte.TryParse(str, out sbyval))
                        return false;

                    value = sbyval;
                    return true;

                case TypeCode.Single:
                    float sival;
                    if (!Single.TryParse(str, out sival))
                        return false;

                    value = sival;
                    return true;

                case TypeCode.UInt16:
                    ushort ui16val;
                    if (!UInt16.TryParse(str, out ui16val))
                        return false;

                    value = ui16val;
                    return true;

                case TypeCode.UInt32:
                    uint ui32val;
                    if (!UInt32.TryParse(str, out ui32val))
                        return false;

                    value = ui32val;
                    return true;

                case TypeCode.UInt64:
                    ulong ui64val;
                    if (!UInt64.TryParse(str, out ui64val))
                        return false;

                    value = ui64val;
                    return true;

                default:
                    throw new InvalidOperationException($"Type '{PropertyInfo.PropertyType.Name}' is unexpected; no ITextValueConverter or default native Parser has been defined.");
            }
        }

        /// <summary>
        /// Gets the property value formatted as a <see cref="string"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/> (<see cref="FileRecord.Messages"/> will be added).</param>
        /// <param name="str">The property formatted as a <see cref="string"/>.</param>
        /// <returns>Indicates whether the value was retrieved successfully.</returns>
        public bool GetValue(FileRecord record, out string str)
        {
            Check.NotNull(record, nameof(record));
            object val;
            if (FileColumn.IsLineNumber)
            {
                if (PropertyTypeCode == TypeCode.Int32)
                    val = (int)record.LineNumber;
                else
                    val = record.LineNumber;
            }
            else
                val = PropertyInfo.GetValue(record.Value);

            if (val == null)
            {
                str = null;
                if (FileColumn.IsMandatory)
                    return CreateValueError(record, "{0} is required.", str);

                return true;
            }

            if (_valueConverter != null && string.IsNullOrEmpty(FileColumn.WriteFormatString))
            {
                if (!_valueConverter.TryFormat(val, out str))
                    return CreateValueError(record, "{0} is invalid; the value could not be formatted.", str);
            }
            else if (!TryNativeFormat(val, out str))
                return CreateValueError(record, "{0} is invalid; the value could not be formatted.", str);

            return StringWidthCorrector(record, ref str);
        }

        /// <summary>
        /// Checks the <see cref="string"/> <see cref="FileColumnAttribute.Width"/> and either corrects or errors based on configuration
        /// (see <see cref="FileColumnAttribute.WidthOverflow"/>).
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/> (<see cref="FileRecord.Messages"/> will be added).</param>
        /// <param name="str">The <see cref="string"/> value.</param>
        /// <returns>Indicates whether the value is considered valid.</returns>
        public bool StringWidthCorrector(FileRecord record, ref string str)
        {
            if (string.IsNullOrEmpty(str))
                return true;

            if (FileColumn.Width > 0 && str.Length > FileColumn.Width)
            {
                if ((FileColumn.HasWidthOverflowBeenSet && FileColumn.WidthOverflow == ColumnWidthOverflow.Error) || (!FileColumn.HasWidthOverflowBeenSet && FileFormat.WidthOverflow == ColumnWidthOverflow.Error))
                    return CreateValueError(record, "{0} must not exceed {2} characters in length.", str, FileColumn.Width);
                else
                    CreateWarningMessage(record, "{0} exceeded {2} characters in length; value was truncated.", str, FileColumn.Width);

                str = str.Substring(0, FileColumn.Width);
            }

            return true;
        }

        /// <summary>
        /// Execute the native formatter for the type.
        /// </summary>
        private bool TryNativeFormat(object value, out string str)
        {
            str = null;
            switch (PropertyTypeCode)
            {
                case TypeCode.String:
                    str = (string)value;
                    return true;

                case TypeCode.Boolean:
                    str = ((bool)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Char:
                    str = ((char)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.DateTime:
                    str = ((DateTime)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Decimal:
                    str = ((decimal)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Double:
                    str = ((double)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Int16:
                    str = ((Int16)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Int32:
                    str = ((Int32)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Int64:
                    str = ((Int64)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Byte:
                    str = ((Byte)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.SByte:
                    str = ((SByte)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.Single:
                    str = ((Single)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.UInt16:
                    str = ((UInt16)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.UInt32:
                    str = ((UInt32)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                case TypeCode.UInt64:
                    str = ((UInt64)value).ToString(FileColumn.WriteFormatString, System.Globalization.CultureInfo.InvariantCulture);
                    return true;

                default:
                    throw new InvalidOperationException($"Type '{PropertyInfo.PropertyType.Name}' is unexpected; no ITextValueConverter or default native Formatter has been defined.");
            }
        }

        /// <summary>
        /// Create an <see cref="MessageType.Error"/> <see cref="MessageItem"/> and adds to the <paramref name="record"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="str">The column value.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem CreateErrorMessage(FileRecord record, string format, string str, params object[] values)
        {
            var mi = MessageItem.CreateErrorMessage(PropertyInfo.Name, format, (new string[] { Text, str }).Concat(values).ToArray());
            Check.NotNull(record, nameof(record)).Messages.Add(mi);
            return mi;
        }

        /// <summary>
        /// Create a <see cref="MessageType.Warning"/> <see cref="MessageItem"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="str">The column value.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem CreateWarningMessage(FileRecord record, string format, string str, params object[] values)
        {
            var mi = MessageItem.CreateMessage(PropertyInfo.Name, MessageType.Warning, format, (new string[] { Text, str }).Concat(values).ToArray());
            Check.NotNull(record, nameof(record)).Messages.Add(mi);
            return mi;
        }

        /// <summary>
        /// Create an <see cref="MessageType.Info"/> <see cref="MessageItem"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="str">The column value.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem CreateInfoMessage(FileRecord record, string format, string str, params object[] values)
        {
            var mi = MessageItem.CreateMessage(PropertyInfo.Name, MessageType.Info, format, (new string[] { Text, str }).Concat(values).ToArray());
            Check.NotNull(record, nameof(record)).Messages.Add(mi);
            return mi;
        }
    }
}
