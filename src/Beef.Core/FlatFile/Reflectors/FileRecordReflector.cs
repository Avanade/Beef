// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Reflection;
using Beef.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.FlatFile.Reflectors
{
    /// <summary>
    /// Represents a file record (class) reflector.
    /// </summary>
    public sealed class FileRecordReflector
    {
        private readonly Dictionary<string, int> _childrenIndexes = new Dictionary<string, int>();
        private object _validator;
        private MethodInfo _validateMI;
        private ValidationArgs _validationArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileColumnReflector"/> class.
        /// </summary>
        internal FileRecordReflector(Type type, FileFormatBase ff)
        {
            Type = type;
            TypeInfo = type.GetTypeInfo();
            if (!TypeInfo.IsClass)
                throw new ArgumentException($"Type '{type.Name}' must be a class.", nameof(type));

            // Get all the property/column metadata.
            FileFormat = ff;
            int i = 0;
            var columns = new List<FileColumnReflector>();
            var children = new List<FileHierarchyReflector>();

            foreach (var pi in TypeReflector.GetProperties(Type))
            {
                i++;
                var fca = pi.GetCustomAttribute<FileColumnAttribute>();
                var fha = pi.GetCustomAttribute<FileHierarchyAttribute>();

                if (fca != null && fha != null)
                    throw new InvalidOperationException($"Type '{type.Name}' property '{pi.Name}' cannot specify both a FileColumnAttribute and FileHierarchyAttribute.");

                if (fca != null)
                    columns.Add(new FileColumnReflector(i, fca, pi, FileFormat));

                if (fha != null)
                {
                    var fhr = new FileHierarchyReflector(i, fha, pi, ff);
                    if (children.SingleOrDefault(x => x.RecordIdentifier == fhr.RecordIdentifier) != null)
                        throw new InvalidOperationException($"Type '{type.Name}' property '{pi.Name}' FileHierarchyAttribute has a duplicate Record Identifier '{fhr.RecordIdentifier}' (must be unique within Type).");

                    children.Add(new FileHierarchyReflector(i, fha, pi, ff));
                }
            }

            // Order the Columns and Children by Order and Index for usage.
            Columns = columns.OrderBy(x => x.Order).ThenBy(x => x.Index).ToList();
            Children = children.OrderBy(x => x.Order).ThenBy(x => x.Index).ToList();

            for (int j = 0; j < Children.Count; j++)
            {
                _childrenIndexes.Add(Children[j].RecordIdentifier, j);
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/>.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Reflection.TypeInfo"/>.
        /// </summary>
        public TypeInfo TypeInfo { get; private set; }

        /// <summary>
        /// Gets the corresponding <see cref="FileColumnReflector"/> array.
        /// </summary>
        public List<FileColumnReflector> Columns { get; private set; }

        /// <summary>
        /// Gets the corresponding children <see cref="FileHierarchyReflector"/> array.
        /// </summary>
        public List<FileHierarchyReflector> Children { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileFormatBase"/>.
        /// </summary>
        public FileFormatBase FileFormat { get; private set; }

        /// <summary>
        /// Gets the <see cref="Children"/> index for a given record identifier.
        /// </summary>
        /// <param name="recordIdentifier">The record identifier.</param>
        /// <returns>The corresponding index where found; otherwise, -1.</returns>
        public int GetChildIndex(string recordIdentifier)
        {
            if (_childrenIndexes.ContainsKey(recordIdentifier))
                return _childrenIndexes[recordIdentifier];

            return -1;
        }

        /// <summary>
        /// Creates a instance of the <see cref="Type"/>.
        /// </summary>
        /// <returns>An instance.</returns>
        public object CreateInstance()
        {
            return Activator.CreateInstance(Type);
        }

        /// <summary>
        /// Sets the validator <see cref="Type"/>.
        /// </summary>
        /// <param name="validatorType">The validator <see cref="Type"/>.</param>
        public void SetValidatorType(Type validatorType)
        {
            Type gt = typeof(Validator<,>).MakeGenericType(new Type[] { Type, validatorType });
            if (!validatorType.GetTypeInfo().IsSubclassOf(gt))
                throw new ArgumentException($"Validator Type '{Type.Name}' must be a subclass of '{gt.FullName}'.", nameof(validatorType));

            SetValidator(gt.GetProperty("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null));
        }

        /// <summary>
        /// Sets the validator instance.
        /// </summary>
        /// <param name="validator">The validator instance.</param>
        public void SetValidator(object validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            Type gt = typeof(ValidatorBase<>).MakeGenericType(new Type[] { Type });
            if (!validator.GetType().GetTypeInfo().IsSubclassOf(gt))
                throw new ArgumentException($"Validator '{Type.Name}' must be a subclass of '{gt.FullName}'.", nameof(validator));

            _validator = validator;
            _validateMI = validator.GetType().GetMethod("Validate", new Type[] { Type, typeof(ValidationArgs) });
            _validationArgs = new ValidationArgs() { ShallowValidation = true };
        }

        /// <summary>
        /// Validates the value using the configured (see <see cref="SetValidatorType(Type)"/>) validator.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <remarks>Validation will not occur if the record <see cref="FileRecord.HasErrors"/>.</remarks>
        public void Validate(FileRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            if (record.HasErrors || record.Value == null || _validateMI == null)
                return;

            var vc = (IValidationContext)_validateMI.Invoke(_validator, new object[] { record.Value, _validationArgs });
            if (vc.Messages != null && vc.Messages.Count > 0)
                record.Messages.AddRange(vc.Messages);
        }
    }
}
