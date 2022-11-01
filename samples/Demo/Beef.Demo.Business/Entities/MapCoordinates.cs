using System;
using System.ComponentModel;

namespace Beef.Demo.Business.Entities
{
    [TypeConverter(typeof(MapCoordinatesTypeConverter))]
    public partial class MapCoordinates { }
}