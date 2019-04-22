// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using System;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class ColumnTypeData
    {
        [FileColumn()]
        public string String { get; set; }

        [FileColumn()]
        public bool Boolean { get; set; }

        [FileColumn()]
        public char Char { get; set; }

        [FileColumn(Text = "DateTime")]
        public DateTime DateTime { get; set; }

        [FileColumn()]
        public Decimal Decimal { get; set; }

        [FileColumn()]
        public Double Double { get; set; }

        [FileColumn()]
        public Int16 Int16 { get; set; }

        [FileColumn()]
        public Int32 Int32 { get; set; }

        [FileColumn()]
        public Int64 Int64 { get; set; }

        [FileColumn()]
        public Byte Byte { get; set; }

        [FileColumn(Text = "SByte")]
        public SByte SByte { get; set; }

        [FileColumn()]
        public Single Single { get; set; }

        [FileColumn(Text = "UInt16")]
        public UInt16 UInt16 { get; set; }

        [FileColumn(Text = "UInt32")]
        public UInt32 UInt32 { get; set; }

        [FileColumn(Text = "UInt64")]
        public UInt64 UInt64 { get; set; }
    }

    public class ColumnNullableTypeData
    {
        [FileColumn()]
        public string String { get; set; }

        [FileColumn()]
        public bool? Boolean { get; set; }

        [FileColumn()]
        public char? Char { get; set; }

        [FileColumn(Text = "DateTime")]
        public DateTime? DateTime { get; set; }

        [FileColumn()]
        public Decimal? Decimal { get; set; }

        [FileColumn()]
        public Double? Double { get; set; }

        [FileColumn()]
        public Int16? Int16 { get; set; }

        [FileColumn()]
        public Int32? Int32 { get; set; }

        [FileColumn()]
        public Int64? Int64 { get; set; }

        [FileColumn()]
        public Byte? Byte { get; set; }

        [FileColumn(Text = "SByte")]
        public SByte? SByte { get; set; }

        [FileColumn()]
        public Single? Single { get; set; }

        [FileColumn(Text = "UInt16")]
        public UInt16? UInt16 { get; set; }

        [FileColumn(Text = "UInt32")]
        public UInt32? UInt32 { get; set; }

        [FileColumn(Text = "UInt64")]
        public UInt64? UInt64 { get; set; }
    }
}
