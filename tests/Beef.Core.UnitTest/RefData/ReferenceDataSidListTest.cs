// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.RefData
{
    [TestFixture]
    public class ReferenceDataSidListTest
    {
        private class XyzRd : ReferenceDataBaseInt32
        {
            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Ctor_Default()
        {
            var x = new ReferenceDataSidList<XyzRd, string>();
            Assert.IsNotNull(x);
            Assert.AreEqual(0, x.Count);
        }
    }
}
