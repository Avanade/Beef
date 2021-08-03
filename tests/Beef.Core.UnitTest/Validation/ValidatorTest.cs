// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Core.UnitTest.Validation.Entities;
using Beef.Validation;
using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using Beef.Validation.Rules;
using System.Threading.Tasks;
using System;

namespace Beef.Core.UnitTest.Validation
{
    [TestFixture]
    public class ValidatorTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Beef.TextProvider.SetTextProvider(new DefaultTextProvider());

        [Test]
        public async Task Create_NewValidator()
        {
            var r = await Validator.Create<TestData>()
                .HasProperty(x => x.Text, p => p.Mandatory().String(10))
                .HasProperty(x => x.CountB, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10))
                .ValidateAsync(new TestData { CountB = 0 });

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
        public async Task Create_NewValidator_WithIncludeBase()
        {
            var v = Validator.Create<TestDataBase>()
                .HasProperty(x => x.Text, p => p.Mandatory().String(10));

            var r = await Validator.Create<TestData>()
                .IncludeBase(v)
                .HasProperty(x => x.CountB, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10))
                .ValidateAsync(new TestData { CountB = 0 });

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
        public async Task Ruleset_UsingValidatorClass()
        {
            var r = await new TestItemValidator().ValidateAsync(new TestItem { Code = "A", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = await new TestItemValidator().ValidateAsync(new TestItem { Code = "A", Text = "A" });
            Assert.IsFalse(r.HasErrors);

            r = await new TestItemValidator().ValidateAsync(new TestItem { Code = "B", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = await new TestItemValidator().ValidateAsync(new TestItem { Code = "B", Text = "B" });
            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Ruleset_UsingInline()
        {
            var v = Validator.Create<TestItem>()
                .HasRuleSet(x => x.Value.Code == "A", y =>
                {
                    y.Property(x => x.Text).Mandatory().Must(x => x.Text == "A");
                })
                .HasRuleSet(x => x.Value.Code == "B", (y) =>
                {
                    y.Property(x => x.Text).Mandatory().Must(x => x.Text == "B");
                });

            var r = await v.ValidateAsync(new TestItem { Code = "A", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = await v.ValidateAsync(new TestItem { Code = "A", Text = "A" });
            Assert.IsFalse(r.HasErrors);

            r = await v.ValidateAsync(new TestItem { Code = "B", Text = "X" });
            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);

            r = await v.ValidateAsync(new TestItem { Code = "B", Text = "B" });
            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task CheckJsonNamesUsage()
        {
            var v = Validator.Create<TestData>()
                .HasProperty(x => x.Text, p => p.Mandatory())
                .HasProperty(x => x.DateA, p => p.Mandatory())
                .HasProperty(x => x.DateA, p => p.Mandatory());

            var r = await v.ValidateAsync(new TestData(), new ValidationArgs { UseJsonNames = true });
        }

        [Test]
        public async Task Override_OnValidate_WithCheckPredicate()
        {
            var r = await new TestItemValidator2().ValidateAsync(new TestItem(), new ValidationArgs { UseJsonNames = true });
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
        public async Task Inline_OnValidate_WithWhen()
        {
            var r = await Validator.Create<TestItem>()
                .Additional(context =>
                {
                    context.Check(x => x.Text, true, ValidatorStrings.MaxCountFormat, 10);
                    context.Check(x => x.Text, true, ValidatorStrings.MaxCountFormat, 10);
                    return Task.CompletedTask;
                }).ValidateAsync(new TestItem());

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);

            Assert.AreEqual("Description must not exceed 10 item(s).", r.Messages[0].Text);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Text", r.Messages[0].Property);
        }

        [Test]
        public async Task Multi_Common_Validator()
        {
            var cv1 = CommonValidator.Create<string>(v => v.String(5).Must(x => x.Value != "XXXXX"));
            var cv2 = CommonValidator.Create<string>(v => v.String(2).Must(x => x.Value != "YYY"));

            var vx = Validator.Create<TestItem>()
                .HasProperty(x => x.Code, p => p.Common(cv2))
                .HasProperty(x => x.Text, p => p.Common(cv1));

            var r = await vx.ValidateAsync(new TestItem { Code = "YYY", Text = "XXXXX" });

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(2, r.Messages.Count);
        }

        [Test]
        public async Task Entity_SubEntity_Mandatory()
        {
            var r = await Validator.Create<TestEntity>()
                .HasProperty(x => x.Items, (p) => p.Mandatory())
                .HasProperty(x => x.Item, (p) => p.Mandatory())
                .ValidateAsync(new TestEntity { Items = null });

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(2, r.Messages.Count);
        }
        
        public class TestItemValidator : Validator<TestItem>
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

        public class TestItemValidator2 : Validator<TestItem>
        {
            protected override Task OnValidateAsync(ValidationContext<TestItem> context)
            {
                if (!context.HasError(x => x.Code))
                    context.AddError(x => x.Code, ValidatorStrings.InvalidFormat);

                if (!context.HasError(x => x.Code))
                    Assert.Fail();

                context.Check(x => x.Text, (v) => string.IsNullOrEmpty(v), ValidatorStrings.MaxCountFormat, 10);
                context.Check(x => x.Text, (v) => throw new NotFoundException(), ValidatorStrings.MaxCountFormat, 10);
                return Task.CompletedTask;
            }
        }

        public class TestEntity
        {
            public List<TestItem> Items { get; set; } = new List<TestItem>();

            public TestItem Item { get; set; }

            public Dictionary<string, string> Dict { get; set; }

            public Dictionary<string, TestItem> Dict2 { get; set; }
        }

        public class TestItem
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [System.ComponentModel.DataAnnotations.Display(Name = "Description")]
            public string Text { get; set; }
        }

        [Test]
        public async Task Create_NewValidator_CollectionDuplicate()
        {
            var e = new TestEntity();
            e.Items.Add(new TestItem { Code = "ABC", Text = "Abc" });
            e.Items.Add(new TestItem { Code = "DEF", Text = "Abc" });
            e.Items.Add(new TestItem { Code = "ABC", Text = "Def" });
            e.Items.Add(new TestItem { Code = "XYZ", Text = "Xyz" });

            var v = Validator.Create<TestItem>();

            var r = await Validator.Create<TestEntity>()
                .HasProperty(x => x.Items, p => p.Collection(item: CollectionRuleItem.Create(v).DuplicateCheck(y => y.Code)))
                .ValidateAsync(e);

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
                Validator.Create<TestItem>().ThrowValidationException(x => x.Code, "Some text.");
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
                Validator.Create<TestItem>().ThrowValidationException(x => x.Code, "{0} {1} {2} Stuff.", "XXX", "ZZZ");
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
        public async Task ManualProperty_Inject()
        {
            var vx = await Validator.Create<TestInject>()
                .HasProperty(x => x.Text, p => p.Mandatory())
                .HasProperty(x => x.Value, p => p.Mandatory().Custom(TestInjectValueValidate))
                .ValidateAsync(new TestInject { Text = "X", Value = new TestInjectChild { Code = 5 } });

            Assert.AreEqual(1, vx.Messages.Count);
            Assert.AreEqual("Code must be greater than 10.", vx.Messages[0].Text);
            Assert.AreEqual("Value.Code", vx.Messages[0].Property);
        }

        private void TestInjectValueValidate(PropertyContext<TestInject, object> context)
        {
            var vxc = Validator.Create<TestInjectChild>()
                .HasProperty(x => x.Code, p => p.Mandatory().CompareValue(CompareOperator.GreaterThan, 10));

            var type = vxc.GetType();
            var mi = type.GetMethod("ValidateAsync");
            var vc = ((Task<Beef.Validation.ValidationContext<TestInjectChild>>)mi.Invoke(vxc, new object[] { context.Value, context.CreateValidationArgs() })).GetAwaiter().GetResult();
            context.Parent.MergeResult(vc);
        }

        [Test]
        public async Task Entity_ValueOverrideAndDefault()
        {
            var vc = CommonValidator.Create<decimal>(v => v.Default(100));

            var ti = new TestData { Text = "ABC", CountA = 1 };

            var vx = await Validator.Create<TestData>()
                .HasProperty(x => x.Text, p => p.Override("XYZ"))
                .HasProperty(x => x.CountA, p => p.Default(x => 10))
                .HasProperty(x => x.CountB, p => p.Default(x => 20))
                .HasProperty(x => x.AmountA, p => p.Common(vc))
                .ValidateAsync(ti);

            Assert.IsFalse(vx.HasErrors);
            Assert.AreEqual(0, vx.Messages.Count);
            Assert.AreEqual("XYZ", ti.Text);
            Assert.AreEqual(1, ti.CountA);
            Assert.AreEqual(20, ti.CountB);
            Assert.AreEqual(100, ti.AmountA);
        }

        public class Employee
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime Birthdate { get; set; }
            public decimal Salary { get; set; }
            public int WorkingYears { get; set; }
        }

        public class EmployeeValidator : Validator<Employee> 
        {
            public EmployeeValidator()
            {
                Property(x => x.FirstName).Mandatory().String(100);
                Property(x => x.LastName).Mandatory().String(100);
                Property(x => x.Birthdate).Mandatory().CompareValue(CompareOperator.LessThanEqual, DateTime.UtcNow, "today");
                Property(x => x.Salary).Mandatory().Numeric(allowNegatives: false, maxDigits: 10, decimalPlaces: 2);
                Property(x => x.WorkingYears).Numeric(allowNegatives: false).CompareValue(CompareOperator.LessThanEqual, 50);
            }
        }

        [Test]
        public void Entity_ValueCachePerfSync()
        {
            InstantiateValidators();
        }

        private void InstantiateValidators()
        {
            for (int i = 0; i < 1000; i++)
            {
                _ = new EmployeeValidator();
            }
        }

        [Test]
        public void Entity_ValueCachePerfAsync()
        {
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() => InstantiateValidators());
            }

            Task.WaitAll(tasks);
        }

        [Test]
        public async Task Coll_Validator_MaxCount()
        {
            var vxc = Validator.CreateCollection<List<TestItem>, TestItem>(minCount: 1, maxCount: 2, item: CollectionRuleItem.Create(new TestItemValidator()));
            var tc = new List<TestItem> { new TestItem { Code = "A", Text = "aaa" }, new TestItem { Code = "B", Text = "bbb" }, new TestItem { Code= "C", Text = "ccc" } };

            var r = await vxc.ValidateAsync(tc);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(3, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual("Value[0].Text", r.Messages[0].Property);
            Assert.AreEqual(MessageType.Error, r.Messages[1].Type);
            Assert.AreEqual("Description is invalid.", r.Messages[1].Text);
            Assert.AreEqual("Value[1].Text", r.Messages[1].Property);
            Assert.AreEqual(MessageType.Error, r.Messages[2].Type);
            Assert.AreEqual("Value must not exceed 2 item(s).", r.Messages[2].Text);
            Assert.AreEqual("Value", r.Messages[2].Property);
        }

        [Test]
        public async Task Coll_Validator_MinCount()
        {
            var vxc = Validator.CreateCollection<List<TestItem>, TestItem>(minCount: 3, item: CollectionRuleItem.Create(new TestItemValidator()));
            var tc = new List<TestItem> { new TestItem { Code = "A", Text = "A" }, new TestItem { Code = "B", Text = "B" } };

            var r = await vxc.ValidateAsync(tc);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value must have at least 3 item(s).", r.Messages[0].Text);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public async Task Coll_Validator_Duplicate()
        {
            var vxc = Validator.CreateCollection<List<TestItem>, TestItem>(item: CollectionRuleItem.Create(new TestItemValidator()).DuplicateCheck(x => x.Code));
            var tc = new List<TestItem> { new TestItem { Code = "A", Text = "A" }, new TestItem { Code = "A", Text = "A" } };

            var r = await vxc.ValidateAsync(tc);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value contains duplicates; Code value 'A' specified more than once.", r.Messages[0].Text);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public async Task Coll_Validator_OK()
        {
            var vxc = Validator.CreateCollection<List<TestItem>, TestItem>(minCount: 1, maxCount: 2, item: CollectionRuleItem.Create(new TestItemValidator()).DuplicateCheck(x => x.Code));
            var tc = new List<TestItem> { new TestItem { Code = "A", Text = "A" }, new TestItem { Code = "B", Text = "B" } };

            var r = await vxc.ValidateAsync(tc);

            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Coll_Validator_Int_OK()
        {
            var vxc = Validator.CreateCollection<List<int>, int>(minCount: 1, maxCount: 5);
            var ic = new List<int> { 1, 2, 3, 4, 5 };

            var r = await vxc.ValidateAsync(ic);

            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Coll_Validator_Int_Error()
        {
            var vxc = Validator.CreateCollection<List<int>, int>(minCount: 1, maxCount: 3);
            var ic = new List<int> { 1, 2, 3, 4, 5 };

            var r = await vxc.ValidateAsync(ic);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value must not exceed 3 item(s).", r.Messages[0].Text);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public async Task Dict_Validator_MaxCount()
        {
            var vxd = Validator.CreateDictionary<Dictionary<string, TestItem>, string, TestItem>(minCount: 1, maxCount: 2, item: DictionaryRuleItem.Create<string, TestItem>(value: new TestItemValidator()));
            var tc = new Dictionary<string, TestItem> { { "k1", new TestItem { Code = "A", Text = "aaa" } }, { "k2", new TestItem { Code = "B", Text = "bbb" } }, { "k3", new TestItem { Code = "C", Text = "ccc" } } };

            var r = await vxd.ValidateAsync(tc);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(3, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Description is invalid.", r.Messages[0].Text);
            Assert.AreEqual("Value[k1].Text", r.Messages[0].Property);
            Assert.AreEqual(MessageType.Error, r.Messages[1].Type);
            Assert.AreEqual("Description is invalid.", r.Messages[1].Text);
            Assert.AreEqual("Value[k2].Text", r.Messages[1].Property);
            Assert.AreEqual(MessageType.Error, r.Messages[2].Type);
            Assert.AreEqual("Value must not exceed 2 item(s).", r.Messages[2].Text);
            Assert.AreEqual("Value", r.Messages[2].Property);
        }

        [Test]
        public async Task Dict_Validator_MinCount()
        {
            var vxd = Validator.CreateDictionary<Dictionary<string, TestItem>, string, TestItem>(minCount: 3, item: DictionaryRuleItem.Create<string, TestItem>(value: new TestItemValidator()));
            var tc = new Dictionary<string, TestItem> { { "k1", new TestItem { Code = "A", Text = "A" } }, { "k2", new TestItem { Code = "B", Text = "B" } } };

            var r = await vxd.ValidateAsync(tc);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value must have at least 3 item(s).", r.Messages[0].Text);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public async Task Dict_Validator_OK()
        {
            var vxd = Validator.CreateDictionary<Dictionary<string, TestItem>, string, TestItem>(minCount: 2, item: DictionaryRuleItem.Create<string, TestItem>(value: new TestItemValidator()));
            var tc = new Dictionary<string, TestItem> { { "k1", new TestItem { Code = "A", Text = "A" } }, { "k2", new TestItem { Code = "B", Text = "B" } } };

            var r = await vxd.ValidateAsync(tc);

            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Dict_Validator_Int_OK()
        {
            var vxd = Validator.CreateDictionary<Dictionary<string, int>, string, int>(minCount: 1, maxCount: 5);
            var id = new Dictionary<string, int> { { "k1", 1 }, { "k2", 2 }, { "k3", 3 }, { "k4", 4 }, { "k5", 5 } };

            var r = await vxd.ValidateAsync(id);

            Assert.IsFalse(r.HasErrors);
        }

        [Test]
        public async Task Dict_Validator_Int_Error()
        {
            var vxd = Validator.CreateDictionary<Dictionary<string, int>, string, int>(minCount: 1, maxCount: 3);
            var id = new Dictionary<string, int> { { "k1", 1 }, { "k2", 2 }, { "k3", 3 }, { "k4", 4 }, { "k5", 5 } };

            var r = await vxd.ValidateAsync(id);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Value must not exceed 3 item(s).", r.Messages[0].Text);
            Assert.AreEqual("Value", r.Messages[0].Property);
        }

        [Test]
        public async Task Dict_Validator_KeyError()
        {
            var kv = CommonValidator.Create<string>(x => x.Text("Key").Mandatory().String(2));

            var vxd = Validator.CreateDictionary<Dictionary<string, TestItem>, string, TestItem>(minCount: 2, item: DictionaryRuleItem.Create(key: kv, value: new TestItemValidator()));
            var tc = new Dictionary<string, TestItem> { { "k1", new TestItem { Code = "A", Text = "A" } }, { "k2x", new TestItem { Code = "B", Text = "B" } } };

            var r = await vxd.ValidateAsync(tc);

            Assert.IsTrue(r.HasErrors);
            Assert.AreEqual(1, r.Messages.Count);
            Assert.AreEqual(MessageType.Error, r.Messages[0].Type);
            Assert.AreEqual("Key must not exceed 2 characters in length.", r.Messages[0].Text);
            Assert.AreEqual("Value[k2x]", r.Messages[0].Property);
        }

        [Test]
        public async Task Validator_Perf()
        {
            var ev = new EmployeeValidator();
            var v = new Employee { FirstName = "Speedy", LastName = "Fasti", Birthdate = new DateTime(1999, 10, 22), Salary = 51000m, WorkingYears = 20 };

            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 100000; i++)
            {
                var r = await ev.ValidateAsync(v).ConfigureAwait(false);
                Assert.IsFalse(r.HasErrors);
            }

            sw.Stop();
            System.Console.WriteLine($"100K validations - elapsed: {sw.ElapsedMilliseconds} (ms)");
        }
    }
}