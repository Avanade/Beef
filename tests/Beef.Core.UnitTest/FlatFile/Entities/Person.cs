// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using System;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class Person
    {
        [FileColumn(Width = 30, WidthOverflow = ColumnWidthOverflow.Truncate)]
        public string FirstName { get; set; }

        [FileColumn(Width = 30, WidthOverflow = ColumnWidthOverflow.Truncate)]
        public string LastName { get; set; }

        [FileColumn()]
        public DateTime Birthday { get; set; }

        [FileColumn(Width = 30, WidthOverflow = ColumnWidthOverflow.Truncate)]
        public string Nickname { get; set; }
    }
}
