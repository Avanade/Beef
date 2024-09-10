using Cdr.Banking.Api;
using Cdr.Banking.Common.Agents;
using Cdr.Banking.Common.Entities;
using CoreEx.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using UnitTestEx;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace Cdr.Banking.Test
{
    [TestFixture, NonParallelizable]
    public class AccountTest : UsingApiTester<Startup>
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ApiTester.UseUser("jessica");
            Assert.That(TestSetUp.Default.SetUp(), Is.True);
        }

        #region GetAccounts

        [Test]
        public void B110_GetAccounts_User1()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(null)).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(3));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890", "45678901" }));
        }

        [Test]
        public void B120_GetAccounts_User2()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .WithUser("jenny")
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(null)).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(1));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "23456789" }));
        }

        [Test]
        public void B130_GetAccounts_User3_None()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .WithUser("jason")
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(null)).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Is.Empty);
            });
        }

        [Test]
        public void B140_GetAccounts_User4_Auth()
        {
            Agent<AccountAgent, AccountCollectionResult>()
                .WithUser("john")
                .ExpectStatusCode(HttpStatusCode.Unauthorized)
                .Run(a => a.GetAccountsAsync(null));
        }

        [Test]
        public void B210_GetAccounts_OpenStatus()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(new AccountArgs { OpenStatus = "OPEN" })).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(2));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890" }));
        }

        [Test]
        public void B220_GetAccounts_ProductCategory()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(new AccountArgs { ProductCategory = "CRED_AND_CHRG_CARDS" })).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(1));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "34567890" }));
        }

        [Test]
        public void B230_GetAccounts_IsOwned()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(new AccountArgs { IsOwned = true })).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(2));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890" }));
        }

        [Test]
        public void B240_GetAccounts_NotIsOwned()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(new AccountArgs { IsOwned = false })).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(1));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "45678901" }));
        }

        [Test]
        public void B310_GetAccounts_Page1()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(null, PagingArgs.CreatePageAndSize(1, 2))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(2));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890" }));
        }

        [Test]
        public void B320_GetAccounts_Page2()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(null, PagingArgs.CreatePageAndSize(2, 2))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(1));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "45678901" }));
        }

        [Test]
        public void B330_GetAccounts_Page3()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsAsync(null, PagingArgs.CreatePageAndSize(3, 2))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Is.Empty);
            });
        }

        #endregion

        #region GetAccountsQuery

        [Test]
        public void B510_GetAccountsQuery_User1()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsQueryAsync(null)).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(3));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890", "45678901" }));
        }

        [Test]
        public void B520_GetAccountsQuery_OpenStatus()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsQueryAsync(QueryArgs.Create("openstatus eq 'open'"))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(2));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890" }));
        }

        [Test]
        public void B521_GetAccountsQuery_OpenStatus_All()
        {
            Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectError("Field 'openstatus' with value 'all' is invalid: Value not valid for filtering.")
                .Run(a => a.GetAccountsQueryAsync(QueryArgs.Create("openstatus eq 'all'")));
        }

        [Test]
        public void B230_GetAccountsQuery_ProductCategory()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsQueryAsync(QueryArgs.Create("productcategory eq 'CRED_AND_CHRG_CARDS'"))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(1));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "34567890" }));
        }

        [Test]
        public void B240_GetAccountsQuery_IsOwned()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsQueryAsync(QueryArgs.Create("isowned eq true"))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(2));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "12345678", "34567890" }));
        }

        [Test]
        public void B250_GetAccountsQuery_NotIsOwned()
        {
            var v = Agent<AccountAgent, AccountCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAccountsQueryAsync(QueryArgs.Create("isowned eq false"))).Value;

            Assert.That(v, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(v!.Items, Is.Not.Null);
                Assert.That(v.Items, Has.Count.EqualTo(1));
            });
            Assert.That(v.Items.Select(x => x.Id).ToArray(), Is.EqualTo(new string[] { "45678901" }));
        }

        #endregion

        #region GetDetail

        [Test]
        public void C110_GetDetail_NotFound()
        {
            Agent<AccountAgent, AccountDetail?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetDetailAsync("00000000"));
        }

        [Test]
        public void C120_GetDetail_Found()
        {
            Agent<AccountAgent, AccountDetail?>()
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
                .Run(a => a.GetDetailAsync("12345678"));
        }

        [Test]
        public void C130_GetDetail_Found_NoAuth()
        {
            Agent<AccountAgent, AccountDetail?>()
                .WithUser("jenny")
                .ExpectStatusCode(HttpStatusCode.Forbidden)
                .Run(a => a.GetDetailAsync("12345678"));
        }

        [Test]
        public void C140_GetDetail_NoAuth()
        {
            Agent<AccountAgent, AccountDetail?>()
                .WithUser("john")
                .ExpectStatusCode(HttpStatusCode.Unauthorized)
                .Run(a => a.GetDetailAsync("12345678"));
        }

        #endregion

        #region GetBalance

        [Test]
        public void D110_GetBalance_Found()
        {
            var v = this.Agent<AccountAgent, Balance?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetBalanceAsync("12345678")).Value;

            Assert.That(v, Is.Not.Null);
        }

        [Test]
        public void D120_GetBalance_NotFound()
        {
            Agent<AccountAgent, Balance?>()
                .WithUser("jenny")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetBalanceAsync("00000000"));
        }

        [Test]
        public void D130_GetBalance_NotFound_Auth()
        {
            // Try with a known id that is valid for another user.
            Agent<AccountAgent, Balance?>()
                .WithUser("jenny")
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetBalanceAsync("12345678"));
        }

        [Test]
        public void D140_GetBalance_NoAuth()
        {
            Agent<AccountAgent, Balance?>()
                .WithUser("john")
                .ExpectStatusCode(HttpStatusCode.Unauthorized)
                .Run(a => a.GetBalanceAsync("00000000"));
        }

        #endregion

        #region GetStatement

        [Test]
        public void E110_GetStatement_Found()
        {
            Agent<AccountAgent>()
                .WithUser("jenny")
                .Run(a => a.GetStatementAsync("23456789"))
                .AssertOK()
                .AssertContentTypePlainText()
                .AssertContent("Statement for Account '23456789'.");
        }

        #endregion
    }
}