using System.ComponentModel;

namespace Beef.Demo.Common.Entities
{
    [TypeConverter(typeof(MapCoordinatesTypeConverter))]
    public partial class MapCoordinates { }
}