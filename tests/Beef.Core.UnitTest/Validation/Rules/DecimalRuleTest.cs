// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using NUnit.Framework;
using Beef.Core.UnitTest.Validation.Entities;
using Beef.Entities;
using Beef.Validation.Rules;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.Validation.Rules
{
    [TestFixture]
    public class DecimalRuleTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Validate_AllowNegatives()
        {
            var v1 = await (123).Validate().Numeric().RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (-123).Validate().Numeric().RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not be negative.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await (-123).Validate().Numeric(true).RunAsync();
            Assert.IsFalse(v1.HasError);

            var v2 = await (123m).Validate().Numeric().RunAsync();
            Assert.IsFalse(v2.HasError);

            v2 = await (-123m).Validate().Numeric().RunAsync();
            Assert.IsTrue(v2.HasError);
            Assert.AreEqual(1, v2.Messages.Count);
            Assert.AreEqual("Value must not be negative.", v2.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v2.Messages[0].Type);
            Assert.AreEqual("Value", v2.Messages[0].Property);

            v2 = await (-123m).Validate().Numeric(true).RunAsync();
            Assert.IsFalse(v2.HasError);
        }

        [Test]
        public async Task Validate_MaxDigits()
        {
            var v1 = await (123).Validate().Numeric(maxDigits: 5).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (12345).Validate().Numeric(maxDigits: 5).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (123456).Validate().Numeric(maxDigits: 5).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not exceed 5 digits in total.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            var v2 = await (12.34m).Validate().Numeric(maxDigits: 5).RunAsync();
            Assert.IsFalse(v2.HasError);

            v2 = await (12.345m).Validate().Numeric(maxDigits: 5).RunAsync();
            Assert.IsFalse(v2.HasError);

            v2 = await (1.23456m).Validate().Numeric(maxDigits: 5).RunAsync();
            Assert.IsTrue(v2.HasError);
            Assert.AreEqual(1, v2.Messages.Count);
            Assert.AreEqual("Value must not exceed 5 digits in total.", v2.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v2.Messages[0].Type);
            Assert.AreEqual("Value", v2.Messages[0].Property);
        }

        [Test]
        public async Task Validate_DecimalPlaces()
        {
            var v1 = await (12.3m).Validate().Numeric(decimalPlaces: 2).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (123.400m).Validate().Numeric(decimalPlaces: 2).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (0.123m).Validate().Numeric(decimalPlaces: 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value exceeds the maximum specified number of decimal places (2).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public async Task Validate_MaxDigits_And_DecimalPlaces()
        {
            var v1 = await (12.3m).Validate().Numeric(maxDigits: 5, decimalPlaces: 2).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (123.400m).Validate().Numeric(maxDigits: 5, decimalPlaces: 2).RunAsync();
            Assert.IsFalse(v1.HasError);

            v1 = await (0.123m).Validate().Numeric(maxDigits: 5, decimalPlaces: 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value exceeds the maximum specified number of decimal places (2).", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);

            v1 = await (1234.0m).Validate().Numeric(maxDigits: 5, decimalPlaces: 2).RunAsync();
            Assert.IsTrue(v1.HasError);
            Assert.AreEqual(1, v1.Messages.Count);
            Assert.AreEqual("Value must not exceed 5 digits in total.", v1.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, v1.Messages[0].Type);
            Assert.AreEqual("Value", v1.Messages[0].Property);
        }

        [Test]
        public void CalcIntegralLength()
        {
            Assert.AreEqual(0, DecimalRuleHelper.CalcIntegerPartLength(0m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcIntegerPartLength(0.0000001m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcIntegerPartLength(0.9999999m));
            Assert.AreEqual(1, DecimalRuleHelper.CalcIntegerPartLength(1.0000001m));
            Assert.AreEqual(1, DecimalRuleHelper.CalcIntegerPartLength(9.9999999m));
            Assert.AreEqual(2, DecimalRuleHelper.CalcIntegerPartLength(10.0000001m));
            Assert.AreEqual(2, DecimalRuleHelper.CalcIntegerPartLength(99.9999999m));
            Assert.AreEqual(29, DecimalRuleHelper.CalcIntegerPartLength(decimal.MaxValue));

            Assert.AreEqual(0, DecimalRuleHelper.CalcIntegerPartLength(-0.0000001m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcIntegerPartLength(-0.9999999m));
            Assert.AreEqual(1, DecimalRuleHelper.CalcIntegerPartLength(-1.0000001m));
            Assert.AreEqual(1, DecimalRuleHelper.CalcIntegerPartLength(-9.9999999m));
            Assert.AreEqual(2, DecimalRuleHelper.CalcIntegerPartLength(-10.0000001m));
            Assert.AreEqual(2, DecimalRuleHelper.CalcIntegerPartLength(-99.9999999m));
            Assert.AreEqual(29, DecimalRuleHelper.CalcIntegerPartLength(decimal.MinValue));
        }

        [Test]
        public void CalcDecimalPlaces()
        {
            Assert.AreEqual(0, DecimalRuleHelper.CalcFractionalPartLength(0m));
            Assert.AreEqual(7, DecimalRuleHelper.CalcFractionalPartLength(0.0000001m));
            Assert.AreEqual(4, DecimalRuleHelper.CalcFractionalPartLength(0.0001000m));
            Assert.AreEqual(7, DecimalRuleHelper.CalcFractionalPartLength(1.0000001m));
            Assert.AreEqual(3, DecimalRuleHelper.CalcFractionalPartLength(450.678m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcFractionalPartLength(1500m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcFractionalPartLength(decimal.MaxValue));
            Assert.AreEqual(4, DecimalRuleHelper.CalcFractionalPartLength(long.MaxValue + 1.0001m));

            Assert.AreEqual(0, DecimalRuleHelper.CalcFractionalPartLength(0m));
            Assert.AreEqual(7, DecimalRuleHelper.CalcFractionalPartLength(-0.0000001m));
            Assert.AreEqual(4, DecimalRuleHelper.CalcFractionalPartLength(-0.0001000m));
            Assert.AreEqual(7, DecimalRuleHelper.CalcFractionalPartLength(-1.0000001m));
            Assert.AreEqual(3, DecimalRuleHelper.CalcFractionalPartLength(-450.678m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcFractionalPartLength(-1500m));
            Assert.AreEqual(0, DecimalRuleHelper.CalcFractionalPartLength(decimal.MinValue));
            Assert.AreEqual(4, DecimalRuleHelper.CalcFractionalPartLength(long.MinValue - 1.0001m));
        }

        [Test]
        public void CheckMaxDigits()
        {
            Assert.IsTrue(DecimalRuleHelper.CheckMaxDigits(0m, 5));
            Assert.IsTrue(DecimalRuleHelper.CheckMaxDigits(12345m, 5));
            Assert.IsTrue(DecimalRuleHelper.CheckMaxDigits(123.45m, 5));
            Assert.IsTrue(DecimalRuleHelper.CheckMaxDigits(1.2345m, 5));

            Assert.IsFalse(DecimalRuleHelper.CheckMaxDigits(123456m, 5));
            Assert.IsFalse(DecimalRuleHelper.CheckMaxDigits(123.456m, 5));
            Assert.IsFalse(DecimalRuleHelper.CheckMaxDigits(1.23456m, 5));
        }

        [Test]
        public void CheckDecimalPlaces()
        {
            Assert.IsTrue(DecimalRuleHelper.CheckDecimalPlaces(0m, 2));
            Assert.IsTrue(DecimalRuleHelper.CheckDecimalPlaces(1.1m, 2));
            Assert.IsTrue(DecimalRuleHelper.CheckDecimalPlaces(1.12m, 2));
            Assert.IsFalse(DecimalRuleHelper.CheckDecimalPlaces(1.123m, 2));
            Assert.IsFalse(DecimalRuleHelper.CheckDecimalPlaces(1.1234m, 2));
        }
    }
}
