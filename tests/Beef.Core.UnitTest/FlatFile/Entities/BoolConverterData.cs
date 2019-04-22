// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class BoolConverterData
    {
        [FileColumn]
        public bool BoolA { get; set; }

        [FileColumn(TextValueConverterKey = "Bool")]
        public bool BoolB { get; set; }
    }
}
