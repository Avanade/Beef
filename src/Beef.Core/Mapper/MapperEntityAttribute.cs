// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper
{
    /// <summary>
    /// Represents an attribute for defining entity characteristics for <b>Beef.Data.Database.DatabaseMapper</b> mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MapperEntityAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapperEntityAttribute"/> class.
        /// </summary>
        /// <param name="name">The entity name.</param>
        public MapperEntityAttribute(string name) => Name = !string.IsNullOrEmpty(name) ? name : throw new ArgumentNullException(nameof(name));

        /// <summary>
        /// Gets the entity name.
        /// </summary>
        public string Name { get; private set; }
    }
}