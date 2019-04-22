// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile;

namespace Beef.Core.UnitTest.FlatFile.Entities
{
    public class IsLineNumberNotInteger
    {
        [FileColumn(IsLineNumber = true)]
        public string LineNumber { get; set; }
    }
}
