// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the attribute for defining a record hierarchy; i.e. related child-record or child-records (collection).
    /// </summary>
    /// <remarks>The following auto-determining collection logic is applied in the following order; a) <see cref="Array"/>, 
    /// b) collection that implements <see cref="ICollection{T}"/>, or c) <see cref="IEnumerable{T}"/>.</remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FileHierarchyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileHierarchyAttribute"/> class.
        /// </summary>
        /// <param name="recordIdentifier">The record identifier.</param>
        public FileHierarchyAttribute(string recordIdentifier)
        {
            if (string.IsNullOrEmpty(recordIdentifier))
                throw new ArgumentNullException(nameof(recordIdentifier));

            RecordIdentifier = recordIdentifier;
        }

        /// <summary>
        /// Gets the record identifier.
        /// </summary>
        public string RecordIdentifier { get; private set; }

        /// <summary>
        /// Gets or sets the text used for messages; where not specified the property name (converted to sentence case) will be the default.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the column order (defaults to -1).
        /// </summary>
        /// <remarks>Where not specified (less than zero) will default to the order defined within the class (after those that have been specified with an Order).</remarks>
        public int Order { get; set; } = -1;

        /// <summary>
        /// Indicates whether the related child-record is mandatory (defaults to <c>false</c>).
        /// </summary>
        /// <remarks>This will ensure that child-record must be provided with the parent; otherwise, the parent record is considered invalid.</remarks>
        public bool IsMandatory { get; set; } = false;

        /// <summary>
        /// Defines the minimum count of child-records (assuming the related child-<see cref="Type"/> is a supported collection).
        /// </summary>
        public int MinCount { get; set; } = 0;

        /// <summary>
        /// Defines the maximum count of child-records (assuming the related child-<see cref="Type"/> is a supported collection).
        /// </summary>
        /// <remarks>Zero indicates that there is no maximum.</remarks>
        public int MaxCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the child-record <see cref="Type"/> (optional) overriding the auto-determining logic where specified.
        /// </summary>
        public Type ChildType { get; set; }

        /// <summary>
        /// Gets or sets the underlying validation type for the child-record instance.
        /// </summary>
        public Type ValidationType { get; set; }
    }
}
