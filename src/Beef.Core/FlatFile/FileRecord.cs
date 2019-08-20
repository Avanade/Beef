// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Diagnostics;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the details for the file record.
    /// </summary>
    [DebuggerDisplay("LineNumber = {LineNumber}, RecordIdentifier = {RecordIdentifier}, Level = {Level}, HasErrors = {HasErrors}")]
    public class FileRecord
    {
        private MessageItemCollection _messages = null;

        /// <summary>
        /// Gets or sets the line number for the record.
        /// </summary>
        public long LineNumber { get; internal set; }

        /// <summary>
        /// Gets or sets the line data for the record.
        /// </summary>
        public string LineData { get; internal set; }

        /// <summary>
        /// Gets or sets the corresponding columns where applicable to format.
        /// </summary>
        public string[] Columns { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy record identifier.
        /// </summary>
        public string RecordIdentifier { get; internal set; }

        /// <summary>
        /// Indicates whether the record has errors (<see cref="Messages"/> contains items with <see cref="MessageType.Error"/>).
        /// </summary>
        public bool HasErrors
        {
            get
            {
                if (_messages == null)
                    return false;
                else
                    return _messages.ContainsType(MessageType.Error);
            }
        }

        /// <summary>
        /// Indicates whether any messages exist (see <see cref="Messages"/>).
        /// </summary>
        public bool HasMessages
        {
            get { return _messages != null && _messages.Count > 0; }
        }

        /// <summary>
        /// Gets the underlying <see cref="MessageItemCollection"/> (uses lazy initialization therefore use <see cref="HasMessages"/> before reading/querying).
        /// </summary>
        public MessageItemCollection Messages
        {
            get
            {
                if (_messages == null)
                    _messages = new MessageItemCollection();

                return _messages;
            }
        }

        /// <summary>
        /// Gets or sets the corresponding deserialized value.
        /// </summary>
        public object Value { get; internal set; }

        /// <summary>
        /// Gets or sets the level within the hierarchy (less than zero indicates that it <see cref="IsOrphaned"/>.
        /// </summary>
        public int Level { get; internal set; } = -1;

        /// <summary>
        /// Indicates whether it is the root record (<see cref="Level"/> is equal to zero).
        /// </summary>
        public bool IsRoot
        {
            get { return Level == 0; }
        }

        /// <summary>
        /// Indicates whether the record is orphaned (i.e. unknown record identifier or misplaced record within hierarchy).
        /// </summary>
        public bool IsOrphaned
        {
            get { return Level < 0; }
        }
    }
}
