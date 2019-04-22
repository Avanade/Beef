// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using System;
using System.Linq;

namespace Beef.Data.OData.UnitTest
{
    [TestFixture]
    public class ODataQueryableTest
    {
        private class ODataTest : ODataBase
        {
            public ODataTest() : base(@"http://blah.blah") { }
        }

        private class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Dob { get; set; }
            public decimal Salary { get; set; }
            public bool IsAwesome { get; set; }
            public Guid RecCode { get; set; }
        }

        [Test]
        public void Where_None()
        {
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$top=100", new ODataTest().CreateQuery<Person>(ODataArgs.Create()).GetODataQuery());
        }

        [Test]
        public void Where_StringIsNull()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.Name == null);
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(Name eq null)&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_StringEqual()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.Name == "abc");
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(Name eq 'abc')&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_StringNotEqual()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.Name != "abc");
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(Name ne 'abc')&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_WildcardEqual()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).WhereWildcard(x => x.Name, "abc");
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=((Name ne null) and (toupper(Name) eq 'ABC'))&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_WildcardStartsWith()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).WhereWildcard(x => x.Name, "ab*");
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=((Name ne null) and startswith(toupper(Name),'AB'))&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_WildcardEndsWith()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).WhereWildcard(x => x.Name, "*bc");
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=((Name ne null) and endswith(toupper(Name),'BC'))&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_WildcardContains()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).WhereWildcard(x => x.Name, "*b*");
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=((Name ne null) and contains(toupper(Name),'B'))&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_IntGreaterThan()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.Id > 20);
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(Id gt 20)&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_DateTimeGreaterThanEqual()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.Dob >= new DateTime(2000,01,01));
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(Dob ge 2000-01-01T00:00:00.0000000)&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_DecimalLessThanEqual()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.Salary <= 99999.99m);
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(Salary le 99999.99)&$top=100", GetODataQuery(q));
        }

        [Test]
        public void Where_GuidEqual()
        {
            var q = new ODataTest().CreateQuery<Person>(ODataArgs.Create()).Where(x => x.RecCode == new Guid("D2798DAB-B288-489E-B2BB-04E594F79088"));
            Assert.AreEqual("?$select=Id,Name,Dob,Salary,IsAwesome,RecCode&$filter=(RecCode eq guid'd2798dab-b288-489e-b2bb-04e594f79088')&$top=100", GetODataQuery(q));
        }

        private string GetODataQuery(IQueryable<Person> q)
        {
            var part = ((ODataQueryable<Person>)q).GetODataQuery();
            return part;
        }
    }
}
