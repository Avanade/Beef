// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class OverrideRuleTest
    {
        [Test]
        public void Validate_Value()
        {
            Assert.Throws<InvalidOperationException>(() => 123.Validate().Override(456).Run());
        }
    }
}
