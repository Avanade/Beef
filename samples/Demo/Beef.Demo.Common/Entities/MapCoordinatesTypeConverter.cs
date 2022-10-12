using System;
using System.ComponentModel;
using System.Globalization;

namespace Beef.Demo.Common.Entities
{
    /// <summary>
    /// Provides <see cref="MapCoordinates"/> to <see cref="string"/> <see cref="Type"/> conversion.
    /// </summary>
    public class MapCoordinatesTypeConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                var parts = str.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    return null!;

                if (!decimal.TryParse(parts[0], out var longitude))
                    return null!;

                if (!decimal.TryParse(parts[1], out var latitude))
                    return null!;

                return new MapCoordinates { Longitude = longitude, Latitude = latitude };
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is MapCoordinates mc)
                return $"{mc.Longitude},{mc.Latitude}";

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}