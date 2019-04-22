using System;

namespace Beef.Mapper
{
    /// <summary>
    /// Represents an attribute that indicates that the property is not to be used for auto-mapping.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MapperIgnoreAttribute : Attribute { }
}
