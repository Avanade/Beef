// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.RefData
{
    /// <summary>
    /// Represents the supported <see cref="ReferenceDataBase"/> <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> code options.
    /// </summary>
    public enum ReferenceDataIdTypeCode
    {
#pragma warning disable CA1720 // Identifier contains type name; by-design, as these relate exactly to the named types.
        /// <summary>
        /// Unknown <see cref="Type"/>, if any.
        /// </summary>
        Unknown,

        /// <summary>
        /// <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> is <see cref="int"/>.
        /// </summary>
        Int32,

        /// <summary>
        /// <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> is <see cref="long"/>.
        /// </summary>
        Int64,

        /// <summary>
        /// <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> is <see cref="System.Guid"/>.
        /// </summary>
        Guid,

        /// <summary>
        /// <see cref="ReferenceDataBase.Id"/> <see cref="Type"/> is <see cref="string"/>.
        /// </summary>
        String
#pragma warning restore CA1720
    }
}