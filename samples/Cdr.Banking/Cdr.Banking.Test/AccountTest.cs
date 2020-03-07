using Beef.Entities;
using Beef.Test.NUnit;
using Cdr.Banking.Common.Agents;
using Cdr.Banking.Common.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;

namespace Cdr.Banking.Test
{
    [TestFixture, NonParallelizable]
    public class AccountTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => TestSetUp.Reset();

        #region GetAccounts

        [Test, TestSetUp("jessica")]
        public void B110_GetAccounts_User1()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(null)).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(3, v.Result.Count);
            Assert.AreEqual(new string[] { "12345678", "34567890", "45678901" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jenny")]
        public void B120_GetAccounts_User2()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(null)).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "23456789" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jason")]
        public void B130_GetAccounts_User3_None()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(null)).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp("john")]
        public void B140_GetAccounts_User4_Auth()
        {
            AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run((a) => a.Agent.GetAccountsAsync(null));
        }

        [Test, TestSetUp("jessica")]
        public void B210_GetAccounts_OpenStatus()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(new AccountArgs { OpenStatus = "OPEN" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "12345678", "34567890" }, v.Result.Select(x => x.Id).ToArray());
        }


        [Test, TestSetUp("jessica")]
        public void B220_GetAccounts_ProductCategory()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(new AccountArgs { ProductCategory = "CRED_AND_CHRG_CARDS" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "34567890" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B230_GetAccounts_IsOwned()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(new AccountArgs { IsOwned = true })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "12345678", "34567890" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B240_GetAccounts_NotIsOwned()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(new AccountArgs { IsOwned = false })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "45678901" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B310_GetAccounts_Page1()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(null, PagingArgs.CreatePageAndSize(1, 2))).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "12345678", "34567890" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B320_GetAccounts_Page2()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(null, PagingArgs.CreatePageAndSize(2, 2))).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "45678901" }, v.Result.Select(x => x.Id).ToArray());
        }

        [Test, TestSetUp("jessica")]
        public void B330_GetAccounts_Page3()
        {
            var v = AgentTester.Create<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAccountsAsync(null, PagingArgs.CreatePageAndSize(3, 2))).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        #endregion

        #region GetDetail

        [Test, TestSetUp("jessica")]
        public void C110_GetDetail_NotFound()
        {
            AgentTester.Create<AccountAgent, AccountDetail>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetDetailAsync("00000000"));
        }

        [Test, TestSetUp("jessica")]
        public void C120_GetDetail_Found()
        {
            AgentTester.Create<AccountAgent, AccountDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new AccountDetail
                {
                    Id = "12345678",
                    Bsb = "020780",
                    AccountNumber = "12345678",
                    CreditCard = new CreditCardAccount { MinPaymentAmount = 100m, PaymentDueAmount = 326.59m, PaymentDueDate = new DateTime(2020, 06, 01) },
                    CreationDate = new DateTime(1985, 03, 18),
                    DisplayName = "Savings",
                    Nickname = "Save",
                    OpenStatus = "OPEN",
                    IsOwned = true,
                    MaskedNumber = "XXXXXX78",
                    ProductCategory = "TRANS_AND_SAVINGS_ACCOUNTS"
                })
                .Run((a) => a.Agent.GetDetailAsync("12345678"));
        }

        [Test, TestSetUp("jenny")]
        public void C130_GetDetail_Found_NoAuth()
        {
            AgentTester.Create<AccountAgent, AccountDetail>()
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run((a) => a.Agent.GetDetailAsync("12345678"));
        }

        [Test, TestSetUp("john")]
        public void C140_GetDetail_NoAuth()
        {
            AgentTester.Create<AccountAgent, AccountDetail>()
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run((a) => a.Agent.GetDetailAsync("12345678"));
        }

        #endregion

        #region GetBalance

        [Test, TestSetUp("jessica")]
        public void D110_GetBalance_Found()
        {
            var v = AgentTester.Create<AccountAgent, Balance>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetBalanceAsync("12345678")).Value;

            Assert.IsNotNull(v);
        }

        [Test, TestSetUp("jenny")]
        public void D120_GetBalance_NotFound()
        {
            AgentTester.Create<AccountAgent, Balance>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetBalanceAsync("00000000"));
        }

        [Test, TestSetUp("jenny")]
        public void D130_GetBalance_NotFound_Auth()
        {
            // Try with a known id that is valid for another user.
            AgentTester.Create<AccountAgent, Balance>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetBalanceAsync("12345678"));
        }

        [Test, TestSetUp("john")]
        public void D140_GetBalance_NoAuth()
        {
            AgentTester.Create<AccountAgent, Balance>()
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run((a) => a.Agent.GetBalanceAsync("00000000"));
        }

        #endregion
    }
}