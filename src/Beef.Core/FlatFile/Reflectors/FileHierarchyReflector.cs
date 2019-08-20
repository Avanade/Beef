// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Beef.FlatFile.Reflectors
{
    /// <summary>
    /// Represents a file hierarchy reflector.
    /// </summary>
    public sealed class FileHierarchyReflector
    {
        private string _text;
        private readonly ComplexTypeReflector _collTypeReflector;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHierarchyReflector"/> class.
        /// </summary>
        /// <param name="index">The <see cref="Index"/>.</param>
        /// <param name="fh">The <see cref="FileHierarchy"/>.</param>
        /// <param name="pi">The <see cref="PropertyInfo"/>.</param>
        /// <param name="ff">The <see cref="FileFormat"/>.</param>
        internal FileHierarchyReflector(int index, FileHierarchyAttribute fh, PropertyInfo pi, FileFormatBase ff)
        {
            Index = index;
            Order = fh.Order < 0 ? int.MaxValue : fh.Order;
            FileHierarchy = fh;
            PropertyInfo = pi;
            FileFormat = ff;

            // Where an intrinsic type then we have an issue.
            if (FileColumnReflector.GetTypeCode(PropertyInfo.PropertyType) != TypeCode.Object)
                throw new ArgumentException(string.Format("Type '{0}' Property '{1}' must be a class or collection (FileHierarchyAttribute).", PropertyInfo.DeclaringType.Name, PropertyInfo.Name), nameof(pi));

            // Determine the collection type.
            _collTypeReflector = ComplexTypeReflector.Create(PropertyInfo);

            // Load/cache the corresponding property type.
            var frr = (_collTypeReflector.ComplexTypeCode == ComplexTypeCode.Object) ? FileFormat.GetFileRecordReflector(PropertyInfo.PropertyType) : FileFormat.GetFileRecordReflector(_collTypeReflector.ItemType);
            if (fh.ValidationType != null)
                frr.SetValidatorType(fh.ValidationType);
        }

        /// <summary>
        /// Gets the record identifier.
        /// </summary>
        public string RecordIdentifier
        {
            get { return FileHierarchy.RecordIdentifier; }
        }

        /// <summary>
        /// Gets the index (sequence) within the defining class.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets or sets the column order.
        /// </summary>
        /// <remarks>Where not specified (less than zero) will default to the order defined within the class (after those that have been specified with an Order).</remarks>
        public int Order { get; set; } = -1;

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

                if (!string.IsNullOrEmpty(FileHierarchy.Text))
                    _text = FileHierarchy.Text;
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
        /// Gets the <see cref="FileHierarchyAttribute"/>.
        /// </summary>
        public FileHierarchyAttribute FileHierarchy { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Reflection.PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileFormatBase"/>.
        /// </summary>
        public FileFormatBase FileFormat { get; private set; }

        /// <summary>
        /// Gets the underlying property <see cref="Type"/>.
        /// </summary>
        public Type PropertyType
        {
            get
            {
                if (_collTypeReflector.ComplexTypeCode == ComplexTypeCode.Object)
                    return PropertyInfo.PropertyType;
                else
                    return _collTypeReflector.ItemType;
            }
        }

        /// <summary>
        /// Indicates whether the underlying property is a collection.
        /// </summary>
        public bool IsCollection
        {
            get { return _collTypeReflector.ComplexTypeCode != ComplexTypeCode.Object; }
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="obj">The object whose property value will be returned.</param>
        /// <returns>The property value.</returns>
        public object GetValue(object obj)
        {
            return PropertyInfo.GetValue(obj);
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="obj">The object whose property value will be set.</param>
        /// <param name="value">The property value(s) to set.</param>
        public void SetValue(object obj, object[] value)
        {
            _collTypeReflector.SetValue(obj, value);
        }

        /// <summary>
        /// Create an <see cref="MessageType.Error"/> <see cref="MessageItem"/> and adds to the <paramref name="record"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem CreateErrorMessage(FileRecord record, string format, params object[] values)
        {
            var mi = MessageItem.CreateErrorMessage(PropertyInfo.Name, format, (new string[] { Text, null }).Concat(values).ToArray());
            record.Messages.Add(mi);
            return mi;
        }

        /// <summary>
        /// Create a <see cref="MessageType.Warning"/> <see cref="MessageItem"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem CreateWarningMessage(FileRecord record, string format, params object[] values)
        {
            var mi = MessageItem.CreateMessage(PropertyInfo.Name, MessageType.Warning, format, (new string[] { Text, null }).Concat(values).ToArray());
            record.Messages.Add(mi);
            return mi;
        }

        /// <summary>
        /// Create an <see cref="MessageType.Info"/> <see cref="MessageItem"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="format">The composite format string.</param>
        /// <param name="values">The values that form part of the message text.</param>
        /// <returns>The <see cref="MessageItem"/>.</returns>
        public MessageItem CreateInfoMessage(FileRecord record, string format, params object[] values)
        {
            var mi = MessageItem.CreateMessage(PropertyInfo.Name, MessageType.Info, format, (new string[] { Text, null }).Concat(values).ToArray());
            record.Messages.Add(mi);
            return mi;
        }
    }
}
