// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile.Reflectors;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beef.FlatFile.Internal
{
    /// <summary>
    /// Represents a file record hierarchy item.
    /// </summary>
    internal sealed class FileRecordHierarchyItem
    {
        /// <summary>
        /// Gets the underlying hierarchy for the specified <see cref="FileFormatBase"/>.
        /// </summary>
        /// <param name="fileFormat">The <see cref="FileFormatBase"/>.</param>
        /// <returns>A dictionary containing the full hierarchy.</returns>
        public static Dictionary<string, FileRecordHierarchyItem> GetHierarchy(FileFormatBase fileFormat)
        {
            var dict = new Dictionary<string, FileRecordHierarchyItem>();
            Get(fileFormat, dict, new FileRecordHierarchyItem { RecordIdentifier = fileFormat.ContentRecordIdentifier, Level = 0, Parent = null, RecordReflector = fileFormat.GetFileRecordReflector(fileFormat.ContentRowType) }, null);
            return dict;
        }

        /// <summary>
        /// Gets the underlying hierarchy for an item.
        /// </summary>
        private static void Get(FileFormatBase fileFormat, Dictionary<string, FileRecordHierarchyItem> dict, FileRecordHierarchyItem item, PropertyInfo pi)
        {
            if (dict.ContainsKey(item.RecordIdentifier))
                throw new InvalidOperationException(string.Format("Type '{0}' property '{1}' FileHierarchyAttribute has a duplicate Record Identifier '{2}'; must be unique within hierarchy).",
                    pi.DeclaringType.Name, pi.Name, item.RecordIdentifier));

            if (fileFormat.HeaderRecordIdentifier != null && item.RecordIdentifier == fileFormat.HeaderRecordIdentifier)
                throw new InvalidOperationException(string.Format("Type '{0}' property '{1}' FileHierarchyAttribute has a duplicate Record Identifier '{2}'; must be different to the Header Record Identifier).",
                    pi.DeclaringType.Name, pi.Name, item.RecordIdentifier));

            if (fileFormat.TrailerRecordIdentifier != null && item.RecordIdentifier == fileFormat.TrailerRecordIdentifier)
                throw new InvalidOperationException(string.Format("Type '{0}' property '{1}' FileHierarchyAttribute has a duplicate Record Identifier '{2}'; must be different to the Trailer Record Identifier).",
                    pi.DeclaringType.Name, pi.Name, item.RecordIdentifier));

            dict.Add(item.RecordIdentifier, item);

            if (item.RecordReflector.Children == null || item.RecordReflector.Children.Length == 0)
                return;

            foreach (var fhr in item.RecordReflector.Children)
            {
                var frhh = new FileRecordHierarchyItem
                {
                    RecordIdentifier = fhr.RecordIdentifier,
                    Level = item.Level + 1,
                    Parent = item,
                    RecordReflector = fileFormat.GetFileRecordReflector(fhr.PropertyType),
                    HierarchyReflector = fhr
                };

                Get(fileFormat, dict, frhh, fhr.PropertyInfo);
                item.Children.Add(frhh.RecordIdentifier, frhh);
            }
        }

        /// <summary>
        /// Initializes a new instance for the <see cref="FileRecordHierarchyItem"/> class.
        /// </summary>
        private FileRecordHierarchyItem() { }

        /// <summary>
        /// Gets the record identifier for this item instance.
        /// </summary>
        public string RecordIdentifier { get; private set; }

        /// <summary>
        /// Gets or sets the hierarchy level; where zero represents the root.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="FileRecordHierarchyItem"/>; where <c>null</c> represents the root.
        /// </summary>
        public FileRecordHierarchyItem Parent { get; private set; }

        /// <summary>
        /// Gets the child <see cref="FileRecordHierarchyItem"/> dictionary;
        /// </summary>
        public Dictionary<string, FileRecordHierarchyItem> Children { get; private set; } = new Dictionary<string, FileRecordHierarchyItem>();

        /// <summary>
        /// Gets or sets the <see cref="FileRecordReflector"/> for this item instance.
        /// </summary>
        public FileRecordReflector RecordReflector { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="FileHierarchyReflector"/> for this item instance; where <c>null</c> represents the root.
        /// </summary>
        public FileHierarchyReflector HierarchyReflector { get; private set; }

        /// <summary>
        /// Indicates whether the file record hierarch item is the root.
        /// </summary>
        public bool IsRoot
        {
            get { return Parent == null; }
        }
    }
}
