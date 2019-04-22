using System;

namespace Beef.Mapper
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
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The property value.</returns>
        object GetSrceValue(TSrce entity, OperationTypes operationType);

        /// <summary>
        /// Sets the source property value.
        /// </summary>
        /// <param name="entity">The source entity value.</param>
        /// <param name="value">The property value.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        void SetSrceValue(TSrce entity, object value, OperationTypes operationType);
    }
}
