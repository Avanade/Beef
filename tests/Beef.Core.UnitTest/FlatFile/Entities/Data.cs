// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class Data
    {
        [FileColumn(Width = 10)]
        public string Col1 { get; set; }

        [FileColumn(Width = 10, WidthOverflow = ColumnWidthOverflow.Error, StringTransform = Beef.Entities.StringTransform.NullToEmpty, StringTrim = Beef.Entities.StringTrim.None)]
        public string Col2 { get; set; }

        [FileColumn(Width = 10, WidthOverflow = ColumnWidthOverflow.Truncate, StringTransform = Beef.Entities.StringTransform.NullToEmpty, StringTrim = Beef.Entities.StringTrim.None)]
        public string Col3 { get; set; }
    }
}
