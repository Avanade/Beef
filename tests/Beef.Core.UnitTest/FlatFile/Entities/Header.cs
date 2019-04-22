// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class Header
    {
        [FileColumn(Width = 3)]
        public string Col { get; set; }
    }
}
