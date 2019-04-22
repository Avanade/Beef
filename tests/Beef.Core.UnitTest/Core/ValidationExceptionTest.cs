// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.Core
{
    [TestFixture]
    public class ValidationExceptionTest
    {
        [Test]
        public void Ctor_NullMessages()
        {
            var vex = new ValidationException("Blah", (IEnumerable<MessageItem>)null);
            Assert.IsNotNull(vex);
            Assert.AreEqual("Blah", vex.Message);
            Assert.AreEqual(0, vex.Messages.Count);
        }
    }
}