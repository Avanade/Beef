// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper.Converters;
using System;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Represents a database CDC <see cref="OperationType"/> mapper.
    /// </summary>
    public class CdcOperationTypeConverter : Singleton<CdcOperationTypeConverter>, IPropertyMapperConverter<OperationType, int>
    {
        /// <summary>
        /// Gets the source value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.SrceType => typeof(OperationType);

        /// <summary>
        /// Gets the destination value <see cref="Type"/>.
        /// </summary>
        Type IPropertyMapperConverter.DestType => typeof(int);

        /// <summary>
        /// Gets the underlying source <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.SrceUnderlyingType => typeof(OperationType);

        /// <summary>
        /// Gets the underlying destination <see cref="Type"/> allowing for nullables.
        /// </summary>
        Type IPropertyMapperConverter.DestUnderlyingType => typeof(int);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value">The <see cref="OperationType"/>.</param>
        /// <returns><inheritdoc/></returns>
        /// <remarks>This method is not supported.</remarks>
        public int ConvertToDest(OperationType value) => throw new NotSupportedException();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value">The <see cref="OperationType"/>.</param>
        /// <returns><inheritdoc/></returns>
        /// <remarks>This method is not supported.</remarks>
        public object? ConvertToDest(object? value) => throw new NotSupportedException();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value">The database value.</param>
        /// <returns>The corresponding <see cref="OperationType"/>.</returns>
        public OperationType ConvertToSrce(int value) => value switch
        {
            1 => OperationType.Delete,
            2 => OperationType.Create,
            4 => OperationType.Update,
            _ => throw new InvalidOperationException($"Unable to convert OperationType as value '{value}' is unknown.")
        };

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="value">The database value.</param>
        /// <returns>The corresponding <see cref="OperationType"/>.</returns>
        public object? ConvertToSrce(object? value) => ConvertToSrce(value == null ? throw new InvalidOperationException("A null value cannot be converted.") : (int)value);
    }
}