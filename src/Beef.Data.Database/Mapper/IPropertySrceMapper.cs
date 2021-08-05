// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper;
using System;

namespace Beef.Data.Database.Mapper
{
    /// <summary>
    /// Provides property mapper capabilities for a source entity property.
    /// </summary>
    /// <typeparam name="TSrce">The source entity <see cref="Type"/>.</typeparam>
    public interface IPropertySrceMapper<TSrce> : IPropertyMapperBase
    {
        /// <summary>
        /// Invokes the underlying <b>when</b> clauses to determine whether the source to destination mapping should occur.
        /// </summary>
        /// <param name="entity">The entity value.</param>
        /// <returns><c>true</c> indicates that the mapping should occur; otherwise, <c>false</c>.</returns>
        bool MapSrceToDestWhen(TSrce entity);

        /// <summary>
        /// Gets the source property value.
        /// </summary>
        /// <param name="entity">The source entity value.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object? GetSrceValue(TSrce entity, OperationTypes operationType);

        /// <summary>
        /// Sets the source property value.
        /// </summary>
        /// <param name="entity">The source entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetSrceValue(TSrce entity, object? value, OperationTypes operationType);

        /// <summary>
        /// Maps the source to the destination updating an existing object.
        /// </summary>
        /// <param name="sourceEntity">The source entity.</param>
        /// <param name="destinationEntity">The destination entity.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        void MapToDest(TSrce sourceEntity, object destinationEntity, OperationTypes operationType);
    }
}