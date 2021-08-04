using Beef.RefData;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Core.UnitTest.RefData
{
    [TestFixture]
    public class ReferenceDataTest
    {
        private class XyzRd : ReferenceDataBaseInt32
        {
            public override object Clone() => throw new NotImplementedException();
        }

        [Test]
        public void IConvertible()
        {
            var xyz = new XyzRd { Id = 123, Code = "ABC" };

            var r = Convert.ChangeType(xyz, typeof(string));
            Assert.IsInstanceOf<string>(r);
            Assert.AreEqual("ABC", r);
        }
    }
}
