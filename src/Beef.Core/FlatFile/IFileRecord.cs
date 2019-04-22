// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.FlatFile
{
    /// <summary>
    /// Enables additional processing to be performed for a <see cref="FileRecord"/> after the internal logic has completed.
    /// </summary>
    public interface IFileRecord
    {
        /// <summary>
        /// Enables further processing to be performed for a <b>read</b>.
        /// </summary>
        /// <param name="fileFormat">The <see cref="FileFormatBase"/>.</param>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <remarks></remarks>
        void OnRead(FileFormatBase fileFormat, FileRecord record);

        /// <summary>
        /// Enables further processing to be performed before a <b>write</b>.
        /// </summary>
        /// <param name="fileFormat">The <see cref="FileFormatBase"/>.</param>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <remarks>Update either the <see cref="FileRecord.Columns"/> (which will be automatically formatted as per <paramref name="fileFormat"/>), or update the
        /// <see cref="FileRecord.LineData"/> directly which will bypass the automatic formatting.</remarks>
        void OnWrite(FileFormatBase fileFormat, FileRecord record);
    }
}
