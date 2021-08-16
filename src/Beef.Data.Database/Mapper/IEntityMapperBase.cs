// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef.Data.Database.Mapper
{
    /// <summary>
    /// Enables the base entity mapping capabilities. 
    /// </summary>
    public interface IEntityMapperBase
    {
        /// <summary>
        /// Gets the source <see cref="Type"/>.
        /// </summary>
        Type SrceType { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mappings.
        /// </summary>
        IEnumerable<IPropertyMapperBase> Mappings { get; }

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by source property name.
        /// </summary>
        /// <param name="name">The source property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase? GetBySrcePropertyName(string name);

        /// <summary>
        /// Gets the <see cref="IPropertyMapperBase"/> mapping by destination property name.
        /// </summary>
        /// <param name="name">The destination property name.</param>
        /// <returns>The <see cref="IPropertyMapperBase"/> where found; otherwise, <c>null</c>.</returns>
        IPropertyMapperBase? GetByDestPropertyName(string name);
    }
}