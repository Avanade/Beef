namespace Beef.CodeGen.OpenApi
{
    /// <summary>
    /// Provides the OpenAPI name type.
    /// </summary>
    public enum OpenApiNameType
    {
        /// <summary>
        /// Represents the entity name.
        /// </summary>
        Entity,

        /// <summary>
        /// Represents the property name.
        /// </summary>
        Property,

        /// <summary>
        /// Represents the operation name.
        /// </summary>
        Operation,

        /// <summary>
        /// Represents the parameter name.
        /// </summary>
        Parameter,

        /// <summary>
        /// Represents the enum name.
        /// </summary>
        Enum,

        /// <summary>
        /// Represents the enum value.
        /// </summary>
        EnumValue
    }
}