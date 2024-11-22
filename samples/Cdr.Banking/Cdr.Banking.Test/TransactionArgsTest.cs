using Cdr.Banking.Business.Entities;
using Cdr.Banking.Business.Validation;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.Expectations;

namespace Cdr.Banking.Test
{
    [TestFixture]
    public class TransactionArgsTest
    {
        [Test]
        public async Task A110_ArgsValidator_Empty()
        {
            var ta = new TransactionArgs();
            var r = await GenericTester.Create().Validation().WithAsync(async () => await new TransactionArgsValidator().ValidateAsync(ta));

            Assert.Multiple(() =>
            {
                Assert.That(r!.Result.HasErrors, Is.False);
                Assert.That(ta.FromDate!.Value.Date, Is.EqualTo(DateTime.UtcNow.Date.AddDays(-90)));
                Assert.That(ta.ToDate, Is.Null);
                Assert.That(ta.MinAmount, Is.Null);
                Assert.That(ta.MaxAmount, Is.Null);
                Assert.That(ta.Text, Is.Null);
            });
        }

        [Test]
        public async Task A120_ArgsValidator_ToDateOnly()
        {
            var ta = new TransactionArgs { ToDate = new DateTime(2020, 03, 01) };
            var r = await GenericTester.Create().Validation().WithAsync(async () => await new TransactionArgsValidator().ValidateAsync(ta));

            Assert.Multiple(() =>
            {
                Assert.That(r!.Result.HasErrors, Is.False);
                Assert.That(ta.FromDate, Is.EqualTo(new DateTime(2020, 03, 01).AddDays(-90)));
                Assert.That(ta.ToDate, Is.EqualTo(new DateTime(2020, 03, 01)));
                Assert.That(ta.MinAmount, Is.Null);
                Assert.That(ta.MaxAmount, Is.Null);
                Assert.That(ta.Text, Is.Null);
            });
        }

        [Test]
        public async Task A130_ArgsValidator_ValidSame()
        {
            var ta = new TransactionArgs { FromDate = new DateTime(2020, 03, 01), ToDate = new DateTime(2020, 03, 01), MinAmount = 100m, MaxAmount = 100m, Text = "Best Buy" };
            var r = await GenericTester.Create().Validation().WithAsync(async () => await new TransactionArgsValidator().ValidateAsync(ta));

            Assert.Multiple(() =>
            {
                Assert.That(r!.Result.HasErrors, Is.False);
                Assert.That(ta.FromDate, Is.EqualTo(new DateTime(2020, 03, 01)));
                Assert.That(ta.ToDate, Is.EqualTo(new DateTime(2020, 03, 01)));
                Assert.That(ta.MinAmount, Is.EqualTo(100m));
                Assert.That(ta.MaxAmount, Is.EqualTo(100m));
                Assert.That(ta.Text, Is.EqualTo("Best Buy"));
            });
        }

        [Test]
        public async Task A140_ArgsValidator_ValidDiff()
        {
            var ta = new TransactionArgs { FromDate = new DateTime(2020, 03, 01), ToDate = new DateTime(2020, 04, 01), MinAmount = 100m, MaxAmount = 120m, Text = "Best Buy" };
            var r = await GenericTester.Create().Validation().WithAsync(async () => await new TransactionArgsValidator().ValidateAsync(ta));

            Assert.Multiple(() =>
            {
                Assert.That(r!.Result.HasErrors, Is.False);
                Assert.That(ta.FromDate, Is.EqualTo(new DateTime(2020, 03, 01)));
                Assert.That(ta.ToDate, Is.EqualTo(new DateTime(2020, 04, 01)));
                Assert.That(ta.MinAmount, Is.EqualTo(100m));
                Assert.That(ta.MaxAmount, Is.EqualTo(120m));
                Assert.That(ta.Text, Is.EqualTo("Best Buy"));
            });
        }

        [Test]
        public async Task A150_ArgsValidator_Invalid()
        {
            var ta = new TransactionArgs { FromDate = new DateTime(2020, 03, 01), ToDate = new DateTime(2020, 02, 01), MinAmount = 100m, MaxAmount = 80m, Text = "Best*Buy" };
            await GenericTester.Create()
                .ConfigureServices(sc => sc.AddValidationTextProvider())
                .ExpectErrors(
                    "Oldest time must be less than or equal to Newest time.",
                    "Min Amount must be less than or equal to Max Amount.",
                    "Text contains invalid or non-supported wildcard selection.")
                .Validation().WithAsync(async () => await new TransactionArgsValidator().ValidateAsync(ta));
        }
    }
}