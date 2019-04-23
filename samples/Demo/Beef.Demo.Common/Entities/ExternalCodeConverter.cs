using Beef.Mapper.Converters;
using Beef.RefData;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Represents an <b>ExternalCode</b> converter.
    /// <typeparam name="TSrceProperty">The source property <see cref="System.Type"/>.</typeparam>
    /// </summary>
    public class ExternalCodeConverter<TSrceProperty> : ReferenceDataMappingConverter<ExternalCodeConverter<TSrceProperty>, TSrceProperty, string>
        where TSrceProperty : ReferenceDataBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeConverter{TSrceProperty}"/> class.
        /// </summary>
        public ExternalCodeConverter() : base("ExternalCode") { }
    }
}
