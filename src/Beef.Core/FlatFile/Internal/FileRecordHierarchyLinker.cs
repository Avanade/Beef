// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.FlatFile.Internal
{
    /// <summary>
    /// Represents a class for linking/managing the record hierarchy.
    /// </summary>
    internal class FileRecordHierarchyLinker
    {
        private readonly Dictionary<string, List<FileRecordHierarchyLinker>> _children = new Dictionary<string, List<FileRecordHierarchyLinker>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRecordHierarchyLinker"/> class.
        /// </summary>
        /// <param name="frhi">The <see cref="FileRecordHierarchyLinker"/>.</param>
        /// <param name="parent">The parent <see cref="FileRecordHierarchyLinker"/>.</param>
        /// <param name="index">The <see cref="Index"/> for the record.</param>
        internal FileRecordHierarchyLinker(FileRecordHierarchyItem frhi, FileRecordHierarchyLinker parent, int index)
        {
            RecordIdentifier = frhi.RecordIdentifier;
            Parent = parent;
            Index = index;

            foreach (var child in frhi.Children)
            {
                _children.Add(child.Key, new List<FileRecordHierarchyLinker>());
            }
        }

        /// <summary>
        /// Gets the record identifier.
        /// </summary>
        public string RecordIdentifier { get; private set; }

        /// <summary>
        /// Gets the parent <see cref="FileRecordHierarchyLinker"/>.
        /// </summary>
        public FileRecordHierarchyLinker Parent { get; private set; }

        /// <summary>
        /// Gets the index for the record (from a collection context). 
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Adds the <paramref name="record"/> as a child.
        /// </summary>
        /// <param name="frhi">The <see cref="FileRecordHierarchyItem"/> configuration.</param>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <returns>The corresponding <see cref="FileRecordHierarchyLinker"/>.</returns>
        public FileRecordHierarchyLinker AddChild(FileRecordHierarchyItem frhi, FileRecord record)
        {
            var list = GetChildList(record.RecordIdentifier);
            var item = new FileRecordHierarchyLinker(frhi, this, list.Count);
            list.Add(item);
            return item;
        }

        /// <summary>
        /// Gets the children list for the specified record identifier.
        /// </summary>
        private List<FileRecordHierarchyLinker> GetChildList(string recordIdentifier)
        {
            List<FileRecordHierarchyLinker> list;
            if (!_children.TryGetValue(recordIdentifier, out list))
                throw new InvalidOperationException("Attempting to retrieve a child with a record identifier that does not exist");

            return list;
        }

        /// <summary>
        /// Gets an array of the child values for a specified record identifier.
        /// </summary>
        /// <param name="recordIdentifier">The record identifier.</param>
        /// <returns>A corresponding value <see cref="object"/> array.</returns>
        public object[] GetChildValues(string recordIdentifier)
        {
            return GetChildList(recordIdentifier).Select(x => x.Value).ToArray();
        }

        /// <summary>
        /// Gets the count of the children with the specified record identifier.
        /// </summary>
        /// <param name="recordIdentifier">The record identifier.</param>
        /// <returns>The current count of children for the underlying record identifier.</returns>
        public int GetChildCount(string recordIdentifier)
        {
            return GetChildList(recordIdentifier).Count;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }
    }
}
