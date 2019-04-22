// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class BothColumnAndHierarchyAttributes
    {
        [FileColumn]
        [FileHierarchy("X")]
        public string Data { get; set; }
    }
}
