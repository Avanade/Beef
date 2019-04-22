// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using Beef.FlatFile.Converters;
using System;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class DataWithOtherPropertyTypes
    {
        [FileColumn]
        public TimeSpan TimeSpan { get; set; }

        [FileColumn]
        public LongLat LongLat { get; set; }
    }

    public class LongLat
    {
        public int Longitude { get; set; }

        public int Latitude { get; set; }
    }

    public class LongLatConverter : ITextValueConverter<LongLat>
    {
        public bool TryFormat(LongLat value, out string result)
        {
            result = value.Longitude + "+" + value.Latitude;
            return true;
        }

        public bool TryFormat(object value, out string result)
        {
            var v = (LongLat)value;
            result = v.Longitude + "+" + v.Latitude;
            return true;
        }

        public bool TryParse(string str, out LongLat result)
        {
            result = new LongLat();
            if (string.IsNullOrEmpty(str))
                return true;

            var parts = str.Split('+');
            if (parts == null || parts.Length != 2)
                return false;

            result.Longitude = int.Parse(parts[0]);
            result.Latitude = int.Parse(parts[1]);

            return true;
        }

        public bool TryParse(string str, out object result)
        {
            LongLat v = null;
            var r = TryParse(str, out v);
            result = v;
            return r;
        }
    }
}
