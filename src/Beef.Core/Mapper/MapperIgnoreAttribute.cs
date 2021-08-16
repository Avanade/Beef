// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper
{
    /// <summary>
    /// Represents an attribute that indicates that the property is not to be used for <b>Beef.Data.Database.DatabaseMapper</b> mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MapperIgnoreAttribute : Attribute { }
}