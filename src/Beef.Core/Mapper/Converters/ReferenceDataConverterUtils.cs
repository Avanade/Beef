// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Provides utility capabilities for the Reference Data Converters.
    /// </summary>
    internal static class ReferenceDataConverterUtils
    {
        /// <summary>
        /// Checks the <see cref="ReferenceDataBase.IsValid"/> indicator and throws an exception accordingly.
        /// </summary>
        /// <param name="refType">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <param name="refData">The <see cref="ReferenceDataBase"/> value.</param>
        /// <returns>The <paramref name="refData"/>.</returns>
        public static ReferenceDataBase CheckIsValid(Type refType, ReferenceDataBase refData)
        {
            if (refData == null || !refData.IsValid)
                throw new InvalidOperationException($"The '{refType.Name}' ReferenceData instance is not valid and is unable to be converted.");

            return refData;
        }

        /// <summary>
        /// Checks the <see cref="ReferenceDataBase"/> is converted (i.e. is not <c>null</c>) and throws an exception accordingly.
        /// </summary>
        /// <param name="refType">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <param name="refData">The <see cref="ReferenceDataBase"/> value.</param>
        /// <param name="value">The value converting from.</param>
        /// <returns>The <paramref name="refData"/>.</returns>
        public static ReferenceDataBase CheckConverted(Type refType, ReferenceDataBase? refData, IComparable value)
            => refData ?? throw new InvalidOperationException($"The value '{value}' is unable to be converted to a valid '{refType.Name}' ReferenceData instance.");
    }
}