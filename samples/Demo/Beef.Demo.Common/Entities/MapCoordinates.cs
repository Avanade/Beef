using System;
using System.ComponentModel;

namespace Beef.Demo.Common.Entities
{
    [TypeConverter(typeof(MapCoordinatesTypeConverter))]
    public partial class MapCoordinates : IFormattable
    {
        public string ToString(string format, IFormatProvider formatProvider) => $"{Longitude.ToString(formatProvider)},{Latitude.ToString(formatProvider)}";
    }
}