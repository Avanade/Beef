using Beef.Mapper.Converters;
using System;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Provides mapping to/from <see cref="MapCoordinates"/> and a <see cref="string"/>.
    /// </summary>
    public class MapCoordinatesToStringConverter : CustomConverter<MapCoordinates, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapCoordinatesToStringConverter"/> class.
        /// </summary>
        public MapCoordinatesToStringConverter() : base(mc => mc.Longitude + "," + mc.Latitude, t =>
        {
            if (string.IsNullOrEmpty(t))
                return null;

            var parts = t.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return null;

            if (!decimal.TryParse(parts[0], out var longitude))
                return null;

            if (!decimal.TryParse(parts[1], out var latitude))
                return null;

            return new MapCoordinates { Longitude = longitude, Latitude = latitude };
        }) { }
    }
}