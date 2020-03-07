using Beef.Test.NUnit;
using Cdr.Banking.Business.Validation;
using Cdr.Banking.Common.Agents;
using Cdr.Banking.Common.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;

namespace Cdr.Banking.Test
{
    public class TransactionTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => TestSetUp.Reset();

        #region ArgsValidator

        [Test]
        public void A110_ArgsValidator_Empty()
        {
            var ta = new TransactionArgs();
            var r = TransactionArgsValidator.Default.Validate(ta);
            Assert.IsFalse(r.HasErrors);
            Assert.AreEqual(DateTime.Now.Date.AddDays(-90), ta.FromDate!.Value.Date);
            Assert.IsNull(ta.ToDate);
            Assert.IsNull(ta.MinAmount);
            Assert.IsNull(ta.MaxAmount);
            Assert.IsNull(ta.Text);
        }

        [Test]
        public void A120_ArgsValidator_ToDateOnly()
        {
            var ta = new TransactionArgs { ToDate = new DateTime(2020, 03, 01) };
            var r = TransactionArgsValidator.Default.Validate(ta);
            Assert.IsFalse(r.HasErrors);
            Assert.AreEqual(new DateTime(2020, 03, 01).AddDays(-90), ta.FromDate);
            Assert.AreEqual(new DateTime(2020, 03, 01), ta.ToDate);
            Assert.IsNull(ta.MinAmount);
            Assert.IsNull(ta.MaxAmount);
            Assert.IsNull(ta.Text);
        }

        [Test]
        public void A130_ArgsValidator_ValidSame()
        {
            var ta = new TransactionArgs { FromDate = new DateTime(2020, 03, 01), ToDate = new DateTime(2020, 03, 01), MinAmount = 100m, MaxAmount = 100m, Text = "Best Buy" };
            var r = TransactionArgsValidator.Default.Validate(ta);
            Assert.IsFalse(r.HasErrors);
            Assert.AreEqual(new DateTime(2020, 03, 01), ta.FromDate);
            Assert.AreEqual(new DateTime(2020, 03, 01), ta.ToDate);
            Assert.AreEqual(100m, ta.MinAmount);
            Assert.AreEqual(100m, ta.MaxAmount);
            Assert.AreEqual("Best Buy", ta.Text);
        }

        [Test]
        public void A140_ArgsValidator_ValidDiff()
        {
            var ta = new TransactionArgs { FromDate = new DateTime(2020, 03, 01), ToDate = new DateTime(2020, 04, 01), MinAmount = 100m, MaxAmount = 120m, Text = "Best Buy" };
            var r = TransactionArgsValidator.Default.Validate(ta);
            Assert.IsFalse(r.HasErrors);
            Assert.AreEqual(new DateTime(2020, 03, 01), ta.FromDate);
            Assert.AreEqual(new DateTime(2020, 04, 01), ta.ToDate);
            Assert.AreEqual(100m, ta.MinAmount);
            Assert.AreEqual(120m, ta.MaxAmount);
            Assert.AreEqual("Best Buy", ta.Text);
        }

        [Test]
        public void A150_ArgsValidator_Invalid()
        {
            ExpectValidationException.Throws(() =>
            {
                var ta = new TransactionArgs { FromDate = new DateTime(2020, 03, 01), ToDate = new DateTime(2020, 02, 01), MinAmount = 100m, MaxAmount = 80m, Text = "Best*Buy" };
                TransactionArgsValidator.Default.Validate(ta).ThrowOnError();
            },
            "Oldest time must be less than or equal to Newest time.",
            "Min Amount must be less than or equal to Max Amount.",
            "Text contains invalid or non-supported wildcard selection.");
        }

        #endregion

        #region GetTransactions

        [Test, TestSetUp("jessica")]
        public void B110_GetTransactions_FromDate()
        {
            var v = AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetTransactionsAsync("12345678", new TransactionArgs { FromDate = new DateTime(2019, 04, 01) })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(3, v.Result.Count);
            Assert.AreEqual(new string[] { "X0007", "X0003", "X0001" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B120_GetTransactions_DateRange()
        {
            var v = AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetTransactionsAsync("12345678", new TransactionArgs { FromDate = new DateTime(2019, 04, 01), ToDate = new DateTime(2019, 07, 01) })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "X0003", "X0001" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B130_GetTransactions_MinAmount()
        {
            var v = AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetTransactionsAsync("12345678", new TransactionArgs { FromDate = new DateTime(2019, 04, 01), MinAmount = 0 })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "X0003" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B140_GetTransactions_MaxAmount()
        {
            var v = AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetTransactionsAsync("12345678", new TransactionArgs { FromDate = new DateTime(2019, 04, 01), MaxAmount = 0 })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "X0007", "X0001" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jenny")]
        public void B150_GetTransactions_Text()
        {
            var v = AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetTransactionsAsync("23456789", new TransactionArgs { FromDate = new DateTime(2019, 04, 01), Text = "usb" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "X0006", "X0002" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jenny")]
        public void B160_GetTransactions_AccountAuth()
        {
            AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run((a) => a.Agent.GetTransactionsAsync("12345678", new TransactionArgs { FromDate = new DateTime(2019, 04, 01) }));
        }

        [Test, TestSetUp("john")]
        public void B170_GetTransactions_Auth()
        {
           AgentTester.Create<TransactionAgent, TransactionCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run((a) => a.Agent.GetTransactionsAsync("12345678", new TransactionArgs { FromDate = new DateTime(2019, 04, 01) }));
        }

        #endregion
    }
}