// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Core.UnitTest.Validation.Entities;
using Beef.Validation;
using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using Beef.Validation.Rules;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class ValidatorTest
    {
        [Test]
        public void Create_NewValidator()
        {
            var r = Validator<TestData>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory().String(10))
                .HasProperty(x => x.CountB, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10))
                .Validate(new TestData { CountB = 0 });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);

            Assert.AreEqual(2, r.Messages.Count);
            Assert.AreEqual("Text is required.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            Assert.AreEqual("Count B must be greater than 10.", r.Messages[1].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[1].Type);
            Assert.AreEqual("CountB", r.Messages[1].Property);
        }

        [Test]
        public void Create_NewValidator_WithIncludeBase()
        {
            var v = Validator<TestDataBase>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory().String(10));

            var r = Validator<TestData>.Create()
                .IncludeBase(v)
                .HasProperty(x => x.CountB, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10))
                .Validate(new TestData { CountB = 0 });

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);

            Assert.AreEqual(2, r.Messages.Count);
            Assert.AreEqual("Text is required.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            Assert.AreEqual("Count B must be greater than 10.", r.Messages[1].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[1].Type);
            Assert.AreEqual("CountB", r.Messages[1].Property);
        }

        [Test]
        public void Ruleset_UsingValidatorClass()
        {
            var r = TestItemValidator.Default.Validate(new TestItem { Code = "A", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = TestItemValidator.Default.Validate(new TestItem { Code = "A", Text = "A" });
            Assert.IsFalse(r.HasErrors);

            r = TestItemValidator.Default.Validate(new TestItem { Code = "B", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = TestItemValidator.Default.Validate(new TestItem { Code = "B", Text = "B" });
            Assert.IsFalse(r.HasErrors);
        }

        public void Ruleset_UsingInline()
        {
            var v = Validator<TestItem>.Create()
                .HasRuleSet(x => x.Value.Code == "A", y =>
                {
                    y.Property(x => x.Text).Mandatory().Must(x => x.Text == "A");
                })
                .HasRuleSet(x => x.Value.Code == "B", (y) =>
                {
                    y.Property(x => x.Text).Mandatory().Must(x => x.Text == "B");
                });

            var r = v.Validate(new TestItem { Code = "A", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Text is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = v.Validate(new TestItem { Code = "A", Text = "A" });
            Assert.IsFalse(r.HasErrors);

            r = v.Validate(new TestItem { Code = "B", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Text is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = v.Validate(new TestItem { Code = "B", Text = "B" });
            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public void CheckJsonNamesUsage()
        {
            var v = Validator<TestData>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory())
                .HasProperty(x => x.DateA, p => p.Mandatory())
                .HasProperty(x => x.DateA, p => p.Mandatory());

            var r = v.Validate(new TestData(), new ValidationArgs { UseJsonNames = true });
        }

        [Test]
        public void Override_OnValidate_WithCheckPredicate()
        {
            var r = TestItemValidator2.Default.Validate(new TestItem(), new ValidationArgs { UseJsonNames = true });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(2, r.Messages.Count);

            Assert.AreEqual("Code is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("code", r.Messages[0].Property);

            Assert.AreEqual("Description must not exceed 10 item(s).", r.Messages[1].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[1].Type);
            Assert.AreEqual("Text", r.Messages[1].Property);
        }

        [Test]
        public void Inline_OnValidate_WithWhen()
        {
            var r = Validator<TestItem>.Create()
                .Additional(context =>
                {
                    context.Check(x => x.Text, true, ValidatorStrings.MaxCountFormat, 10);
                    context.Check(x => x.Text, true, ValidatorStrings.MaxCountFormat, 10);
                }).Validate(new TestItem());

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);

            Assert.AreEqual("Description must not exceed 10 item(s).", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);
        }

        [Test]
        public void Multi_Common_Validator()
        {
            var cv1 = CommonValidator<string>.Create(v => v.String(5).Must(x => x.Value != "XXXXX"));
            var cv2 = CommonValidator<string>.Create(v => v.String(2).Must(x => x.Value != "YYY"));

            var vx = Validator<TestItem>.Create()
                .HasProperty(x => x.Code, p => p.Common(cv2))
                .HasProperty(x => x.Text, p => p.Common(cv1));

            var r = vx.Validate(new TestItem { Code = "YYY", Text = "XXXXX" });

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(2, r.Messages.Count);
        }

        [Test]
        public void Entity_SubEntity_Mandatory()
        {
            var r = Validator<TestEntity>.Create()
                .HasProperty(x => x.Items, (p) => p.Mandatory())
                .HasProperty(x => x.Item, (p) => p.Mandatory())
                .Validate(new TestEntity { Items = null });

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(2, r.Messages.Count);
        }
        
        public class TestItemValidator : Validator<TestItem, TestItemValidator>
        {
            public TestItemValidator()
            {
                RuleSet(x => x.Value.Code == "A", () => 
                {
                    Property(x => x.Text).Mandatory().Must(x => x.Text == "A");
                });

                RuleSet(x => x.Value.Code == "B", () =>
                {
                    Property(x => x.Text).Mandatory().Must(x => x.Text == "B");
                });
            }
        }

        public class TestItemValidator2 : Validator<TestItem, TestItemValidator2>
        {
            protected override void OnValidate(ValidationContext<TestItem> context)
            {
                if (!context.HasError(x => x.Code))
                    context.AddError(x => x.Code, ValidatorStrings.InvalidFormat);

                if (!context.HasError(x => x.Code))
                    Assert.Fail();

                context.Check(x => x.Text, (v) => string.IsNullOrEmpty(v), ValidatorStrings.MaxCountFormat, 10);
                context.Check(x => x.Text, (v) => throw new NotFoundException(), ValidatorStrings.MaxCountFormat, 10);
            }
        }

        public class TestEntity
        {
            public List<TestItem> Items { get; set; } = new List<TestItem>();

            public TestItem Item { get; set; }
        }

        public class TestItem
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Description")]
            public string Text { get; set; }
        }

        [Test]
        public void Create_NewValidator_CollectionDuplicate()
        {
            var e = new TestEntity();
            e.Items.Add(new TestItem { Code = "ABC", Text = "Abc" });
            e.Items.Add(new TestItem { Code = "DEF", Text = "Abc" });
            e.Items.Add(new TestItem { Code = "ABC", Text = "Def" });
            e.Items.Add(new TestItem { Code = "XYZ", Text = "Xyz" });

            var v = Validator<TestItem>.Create();

            var r = Validator<TestEntity>.Create()
                .HasProperty(x => x.Items, p => p.Collection(item: new CollectionRuleItem<TestItem>(v).DuplicateCheck(y => y.Code)))
                .Validate(e);

            Assert.IsNotNull(r);
            Assert.IsTrue(r.HasErrors);

            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Items contains duplicates; Code value 'ABC' specified more than once.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Items", r.Messages[0].Property);
        }

        [Test]
        public void ThrowValidationException_NoArgs()
        {
            try
            {
                Validator<TestItem>.ThrowValidationException(x => x.Code, "Some text.");
                Assert.Fail();
            }
            catch (ValidationException vex)
            {
                Assert.AreEqual("Some text.", vex.Messages[0].Text);
                Assert.AreEqual(MessageType.Error, vex.Messages[0].Type);
                Assert.AreEqual("Code", vex.Messages[0].Property);
            }
        }

        [Test]
        public void ThrowValidationException_WithArgs()
        {
            try
            {
                Validator<TestItem>.ThrowValidationException(x => x.Code, "{0} {1} {2} Stuff.", "XXX", "ZZZ");
                Assert.Fail();
            }
            catch (ValidationException vex)
            {
                Assert.AreEqual("Code XXX ZZZ Stuff.", vex.Messages[0].Text);
                Assert.AreEqual(MessageType.Error, vex.Messages[0].Type);
                Assert.AreEqual("Code", vex.Messages[0].Property);
            }
        }

        private class TestInject
        {
            public string Text { get; set; }
            public object Value { get; set; }
        }

        public class TestInjectChild
        {
            public int Code { get; set; }
        }

        [Test]
        public void ManualProperty_Inject()
        {
            var vx = Validator<TestInject>.Create()
                .HasProperty(x => x.Text, p => p.Mandatory())
                .HasProperty(x => x.Value, p => p.Mandatory().Custom(TestInjectValueValidate))
                .Validate(new TestInject { Text = "X", Value = new TestInjectChild { Code = 5 } });

            Assert.AreEqual(1, vx.Messages.Count);
            Assert.AreEqual("Code must be greater than 10.", vx.Messages[0].Text);
            Assert.AreEqual("Value.Code", vx.Messages[0].Property);
        }

        private void TestInjectValueValidate(PropertyContext<TestInject, object> context)
        {
            var vxc = Validator<TestInjectChild>.Create()
                .HasProperty(x => x.Code, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10));

            var type = vxc.GetType();
            var mi = type.GetMethod("Validate");
            var vc = mi.Invoke(vxc, new object[] { context.Value, context.CreateValidationArgs() });
            context.Parent.MergeResult((Beef.Validation.IValidationContext)vc);
        }

        [Test]
        public void Entity_ValueOverrideAndDefault()
        {
            var vc = CommonValidator<decimal>.Create(v => v.Default(100));

            var ti = new TestData { Text = "ABC", CountA = 1 };

            var vx = Validator<TestData>.Create()
                .HasProperty(x => x.Text, p => p.Override("XYZ"))
                .HasProperty(x => x.CountA, p => p.Default(x => 10))
                .HasProperty(x => x.CountB, p => p.Default(x => 20))
                .HasProperty(x => x.AmountA, p => p.Common(vc))
                .Validate(ti);

            Assert.IsFalse(vx.HasErrors);
            Assert.AreEqual(0, vx.Messages.Count);
            Assert.AreEqual("XYZ", ti.Text);
            Assert.AreEqual(1, ti.CountA);
            Assert.AreEqual(20, ti.CountB);
            Assert.AreEqual(100, ti.AmountA);
        }
    }
}