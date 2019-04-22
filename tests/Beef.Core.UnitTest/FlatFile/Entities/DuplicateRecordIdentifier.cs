// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class DuplicateRecordIdentifier
    {
        [FileHierarchy("A")]
        public Data A { get; set; }

        [FileHierarchy("B")]
        public Data B { get; set; }

        [FileHierarchy("A")]
        public Data C { get; set; }
    }
}
