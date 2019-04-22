// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    // Hierarchy structure:
    // A
    // -B
    // -C
    // --D1 Array[]
    // --D2 List<>
    // --D3 IEnumerable<>
    // --E
    // -F
    // --G
    // ---H

    public abstract class HierarchyData
    {
        [FileColumn(Order = 1, Width = 3, WriteFormatString = "000", IsLineNumber = true)]
        public int LineNumber { get; set; }

        [FileColumn(Order = 2, Width = 2)]
        public string Code { get; set; }
    }

    public class HierarchyDataA : HierarchyData
    {
        [FileHierarchy("B", IsMandatory = true)]
        public HierarchyDataB B { get; set; }

        [FileHierarchy("C")]
        public HierarchyDataC C { get; set; }

        [FileHierarchy("F")]
        public HierarchyDataF F { get; set; }
    }

    public class HierarchyDataB : HierarchyData
    {
    }

    public class HierarchyDataC : HierarchyData
    {
        [FileHierarchy("D1", MinCount = 2, MaxCount = 4, IsMandatory = true)]
        public HierarchyDataD[] D1 { get; set; }

        [FileHierarchy("D2", MinCount = 2)]
        public List<HierarchyDataD> D2 { get; set; }

        [FileHierarchy("D3", MinCount = 2)]
        public IEnumerable<HierarchyDataD> D3 { get; set; }

        [FileHierarchy("E")]
        public HierarchyDataE E { get; set; }
    }

    public class HierarchyDataD : HierarchyData
    {
    }

    public class HierarchyDataE : HierarchyData
    {
    }

    public class HierarchyDataF : HierarchyData
    {
        [FileHierarchy("G")]
        public HierarchyDataG G { get; set; }
    }

    public class HierarchyDataG : HierarchyData
    {
        [FileHierarchy("H")]
        public HierarchyDataH H { get; set; }
    }

    public class HierarchyDataH : HierarchyData
    {
    }
}
