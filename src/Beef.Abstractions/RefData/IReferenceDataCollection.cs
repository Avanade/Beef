// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Collections;

namespace Beef.RefData
{
    /// <summary>
    /// Provides <b>GetById</b> and <see cref="GetByCode"/> functionality for a <see cref="ReferenceDataBase"/> collection (see <see cref="ReferenceDataCollectionBase{ReferenceDataBase}"/>).
    /// </summary>
    public interface IReferenceDataCollection : IEnumerable, IETag
    {
        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? GetById(int id);

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? GetById(long id);

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? GetById(Guid id);

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Id"/>.
        /// </summary>
        /// <param name="id">The specified <see cref="ReferenceDataBase.Id"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? GetById(string? id);

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified <see cref="ReferenceDataBase.Code"/>.
        /// </summary>
        /// <param name="code">The specified <see cref="ReferenceDataBase.Code"/>.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? GetByCode(string? code);

        /// <summary>
        /// Gets the <see cref="ReferenceDataBase"/> for the specified mapping (<see cref="ReferenceDataBase.SetMapping{T}(string, T)"/>) name and value.
        /// </summary>
        /// <param name="name">The mapping name.</param>
        /// <param name="value">The mapping value.</param>
        /// <returns>The <see cref="ReferenceDataBase"/> where found; otherwise, null.</returns>
        ReferenceDataBase? GetByMappingValue(string name, IComparable value);
    }
}