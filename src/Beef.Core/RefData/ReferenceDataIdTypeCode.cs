// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.RefData
{
    /// <summary>
    /// Represents the supported <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> code options.
    /// </summary>
    public enum ReferenceDataIdTypeCode
    {
        /// <summary>
        /// Unknown <see cref="Type"/>, if any.
        /// </summary>
        Unknown,

        /// <summary>
        /// <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> is <see cref="Int32"/>.
        /// </summary>
        Int32,

        /// <summary>
        /// <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> is <see cref="Guid"/>.
        /// </summary>
        Guid
    }
}
