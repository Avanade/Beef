using Beef.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Core.UnitTest.Entities
{
    [TestFixture]
    public class UniqueKeyTest
    {
        [Test]
        public void DefaultCtor()
        {
            var uk = new UniqueKey();
            Assert.AreEqual(0, uk.Args.Length);
        }
    }
}
