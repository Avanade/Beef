// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using Beef.WebApi;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.WebApi
{
    [TestFixture]
    public class WebApiArgTest
    {
        private class XyzRd : ReferenceDataBaseInt32
        {
            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ToString_Url()
        {
            Assert.AreEqual("Value", new WebApiArg<string>("Foo", "Value").ToString());
            Assert.AreEqual("XXX", new WebApiArg<XyzRd>("Bar", new XyzRd { Id = 8, Code = "XXX", Description = "Abc" }).ToString());
        }
    }
}
