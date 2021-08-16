// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Json;
using Beef.RefData;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.Json
{
    [TestFixture]
    public class JsonEntityMergeTest
    {
        public class ReferData : ReferenceDataBaseInt32
        {
            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public static implicit operator ReferData(string code) => ConvertFromCode<ReferData>(code);
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class SubData
        {
            [JsonProperty("code")]
            public string Code { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("count")]
            public int Count { get; set; }
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class KeyData : EntityBase, IUniqueKey
        {
            [JsonProperty("code")]
            public string Code { get; set; }
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("other")]
            public string Other { get; set; }

            public override bool IsInitial => throw new NotImplementedException();

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public string[] UniqueKeyProperties => new string[] { "Code" };

            public UniqueKey UniqueKey => new UniqueKey(Code);
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class KeyDataCollection : EntityBaseCollection<KeyData>
        {
            public override object Clone() => throw new NotImplementedException();
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class NonKeyData : EntityBase
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            public override bool IsInitial => throw new NotImplementedException();

            public override object Clone() => throw new NotImplementedException();
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class NonKeyDataCollection : EntityBaseCollection<NonKeyData>
        {
            public override object Clone() => throw new NotImplementedException();
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class ReferKeyData : EntityBase
        {
            [JsonProperty("code")]
            public string CodeSid { get; set; }

            public ReferData Code { get => CodeSid; set => CodeSid = value.Code; }

            [JsonProperty("text")]
            public string Text { get; set; }

            public override bool IsInitial => throw new NotImplementedException();

            public override object Clone()
            {
                throw new NotImplementedException();
            }

            public string[] UniqueKeyProperties => new string[] { "Code" };

            public UniqueKey UniqueKey => new UniqueKey(Code);
        }

        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class TestData : EntityBase
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            public string Ignore { get; set; }
            [JsonProperty("isValid")]
            public bool IsValid { get; set; }
            [JsonProperty("date")]
            public DateTime Date { get; set; }
            [JsonProperty("count")]
            public int Count { get; set; }
            [JsonProperty("amount")]
            public decimal Amount { get; set; }
            [JsonProperty("sub")]
            public SubData Sub { get; set; }
            [JsonProperty("values")]
            public int[] Values { get; set; }
            [JsonProperty("nokeys")]
            public List<SubData> NoKeys { get; set; }
            [JsonProperty("keys")]
            public List<KeyData> Keys { get; set; }
            [JsonProperty("keyscoll")]
            public KeyDataCollection KeysColl { get; set; }
            [JsonProperty("refers")]
            public List<ReferKeyData> Refers { get; set; }
            [JsonProperty("nonkeys")]
            public NonKeyDataCollection NonKeys { get; set; }
            [JsonProperty("dict")]
            public Dictionary<string, string> Dict { get; set; }
            [JsonProperty("dict2")]
            public Dictionary<int, string> Dict2 { get; set; }

            public override object Clone() => throw new NotImplementedException();
            public override bool IsInitial => throw new NotImplementedException();
        }


        [Test]
        public void Merge_Nulls()
        {
            var td = new TestData();
            Assert.Throws<ArgumentNullException>(() => { JsonEntityMerge.Merge<TestData>(null, td); });
            Assert.Throws<ArgumentNullException>(() => { JsonEntityMerge.Merge<TestData>(new JArray(), null); });
        }

        [Test]
        public void Merge_Malformed()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(new JArray(), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.IsNull(mi.Property);
            Assert.AreEqual("The JSON document is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Empty()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ }"), td));
        }

        [Test]
        public void Merge_Property_UnknownWarning()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"abcde\": 1 }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Warning, mi.Type);
            Assert.AreEqual("abcde", mi.Property);
            Assert.AreEqual("The JSON path is not valid for the entity.", mi.Text);
        }

        [Test]
        public void Merge_Property_UnknownError()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"abcde\": 1 }"), td, new JsonEntityMergeArgs { TreatWarningsAsErrors = true, LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("abcde", mi.Property);
            Assert.AreEqual("The JSON path is not valid for the entity.", mi.Text);
        }

        [Test]
        public void Merge_Property_StringValue()
        {
            var td = new TestData { Name = "Fred" };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"name\": \"Barry\" }"), td));
            Assert.AreEqual("Barry", td.Name);
        }

        [Test]
        public void Merge_Property_StringNull()
        {
            var td = new TestData { Name = "Fred" };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"name\": null }"), td));
            Assert.IsNull(td.Name);
        }

        [Test]
        public void Merge_Property_StringNumberValue()
        {
            var td = new TestData { Name = "Fred" };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"name\": 123 }"), td));
            Assert.AreEqual("123", td.Name);
        }

        [Test]
        public void Merge_Property_String_MalformedA()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"name\": [ \"Barry\" ] }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("name", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Property_String_MalformedB()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"name\": [ \"Barry\", \"Betty\" ] }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("name", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Property_String_MalformedC()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"name\": { \"name\": \"Betty\" } }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("name", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Property_Bool_Malformed()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"isValid\": \"xxx\" }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("isValid", mi.Property);
            Assert.AreEqual("The JSON token is malformed: String 'xxx' was not recognized as a valid Boolean.", mi.Text);
        }

        [Test]
        public void Merge_PrimitiveTypesA()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse(
                "{ \"id\": \"13512759-4f50-e911-b35c-bc83850db74d\", \"name\": \"Barry\", \"isValid\": true, \"date\": \"2018-12-31\", \"count\": \"12\", \"amount\": 132.58 }"
                ), td));

            Assert.AreEqual(new Guid("13512759-4f50-e911-b35c-bc83850db74d"), td.Id);
            Assert.AreEqual("Barry", td.Name);
            Assert.IsTrue(td.IsValid);
            Assert.AreEqual(new DateTime(2018, 12, 31), td.Date);
            Assert.AreEqual(12, td.Count);
            Assert.AreEqual(132.58m, td.Amount);
        }

        [Test]
        public void Merge_PrimitiveTypesB()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse(
                "{ \"id\": \"13512759-4f50-e911-b35c-bc83850db74d\", \"name\": \"Barry\", \"isValid\": true, \"date\": \"2018-12-31\", \"count\": \"12\", \"amount\": 132.58 }"
                ), td));

            Assert.AreEqual(new Guid("13512759-4f50-e911-b35c-bc83850db74d"), td.Id);
            Assert.AreEqual("Barry", td.Name);
            Assert.IsTrue(td.IsValid);
            Assert.AreEqual(new DateTime(2018, 12, 31), td.Date);
            Assert.AreEqual(12, td.Count);
            Assert.AreEqual(132.58m, td.Amount);
        }

        [Test]
        public void Merge_PrimitiveTypesC()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse(
                "{ \"id\": \"13512759-4f50-e911-b35c-bc83850db74d\", \"name\": \"Barry\", \"isValid\": true, \"date\": \"2018-12-31\", \"count\": \"12\", \"amount\": 132.58 }"
                ), td));

            Assert.AreEqual(new Guid("13512759-4f50-e911-b35c-bc83850db74d"), td.Id);
            Assert.AreEqual("Barry", td.Name);
            Assert.IsTrue(td.IsValid);
            Assert.AreEqual(new DateTime(2018, 12, 31), td.Date);
            Assert.AreEqual(12, td.Count);
            Assert.AreEqual(132.58m, td.Amount);
        }

        [Test]
        public void Merge_Property_SubEntityNull()
        {
            var td = new TestData { Sub = new SubData() };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"sub\": null }"), td));
            Assert.IsNull(td.Sub);
        }

        [Test]
        public void Merge_Property_SubEntityNewEmpty()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"sub\": { } }"), td));
            Assert.IsNotNull(td.Sub);
            Assert.IsNull(td.Sub.Code);
            Assert.IsNull(td.Sub.Text);
        }

        [Test]
        public void Merge_Property_SubEntityExistingEmpty()
        {
            var td = new TestData { Sub = new SubData() };
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"sub\": { } }"), td));
            Assert.IsNotNull(td.Sub);
            Assert.IsNull(td.Sub.Code);
            Assert.IsNull(td.Sub.Text);
        }

        [Test]
        public void Merge_Property_ArrayMalformed()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"values\": { } }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("values", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Property_ArrayNull()
        {
            var td = new TestData { Values = new int[] { 1, 2, 3 } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"values\": null }"), td));
            Assert.IsNull(td.Values);
        }

        [Test]
        public void Merge_Property_ArrayEmpty()
        {
            var td = new TestData { Values = new int[] { 1, 2, 3 } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"values\": [] }"), td));
            Assert.IsNotNull(td.Values);
            Assert.AreEqual(0, td.Values.Length);
        }

        [Test]
        public void Merge_Property_ArrayValues_NoChanges()
        {
            var td = new TestData { Values = new int[] { 1, 2, 3 } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"values\": [ 1, 2, 3] }"), td));
            Assert.IsNotNull(td.Values);
            Assert.AreEqual(3, td.Values.Length);
        }

        [Test]
        public void Merge_Property_ArrayValues_Changes()
        {
            var td = new TestData { Values = new int[] { 1, 2, 3 } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"values\": [ 3, 2, 1] }"), td));
            Assert.IsNotNull(td.Values);
            Assert.AreEqual(3, td.Values.Length);
            Assert.AreEqual(3, td.Values[0]);
            Assert.AreEqual(2, td.Values[1]);
            Assert.AreEqual(1, td.Values[2]);
        }

        [Test]
        public void Merge_Property_ListMalformed()
        {
            MessageItem mi = null;
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nokeys\": { } }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("nokeys", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Property_NoKeys_ListNull()
        {
            var td = new TestData { NoKeys = new List<SubData> { new SubData() } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nokeys\": null }"), td));
            Assert.IsNull(td.Values);
        }

        [Test]
        public void Merge_Property_NoKeys_ListEmpty()
        {
            var td = new TestData { NoKeys = new List<SubData> { new SubData() } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nokeys\": [ ] }"), td));
            Assert.IsNotNull(td.NoKeys);
            Assert.AreEqual(0, td.NoKeys.Count);
        }

        [Test]
        public void Merge_Property_NoKeys_ListMalformed()
        {
            MessageItem mi = null;
            var td = new TestData { NoKeys = new List<SubData> { new SubData() } };
            Assert.AreEqual(JsonEntityMergeResult.Error,
                JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nokeys\": [ { \"code\": \"abc\", \"text\": \"xyz\" }, { \"count\": \"xxx\" }, null ] }"),
                td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("nokeys[1].count", mi.Property);
            Assert.AreEqual("The JSON token is malformed: Input string was not in a correct format.", mi.Text);
        }

        [Test]
        public void Merge_Property_NoKeys_List()
        {
            var td = new TestData { NoKeys = new List<SubData> { new SubData() } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nokeys\": [ { \"code\": \"abc\", \"text\": \"xyz\" }, { }, null ] }"), td));
            Assert.IsNotNull(td.NoKeys);
            Assert.AreEqual(3, td.NoKeys.Count);

            Assert.IsNotNull(td.NoKeys[0]);
            Assert.AreEqual("abc", td.NoKeys[0].Code);
            Assert.AreEqual("xyz", td.NoKeys[0].Text);

            Assert.IsNotNull(td.NoKeys[1]);
            Assert.IsNull(td.NoKeys[1].Code);
            Assert.IsNull(td.NoKeys[1].Text);

            Assert.IsNull(td.NoKeys[2]);
        }

        [Test]
        public void Merge_Property_Keys_Null()
        {
            MessageItem mi = null;
            var td = new TestData { Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.Error,
                JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keys\": [ null ] }"),
                td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("keys[0]", mi.Property);
            Assert.AreEqual("The JSON token must be an object where Unique Key value(s) are required.", mi.Text);
        }

        [Test]
        public void Merge_Property_Keys_Empty()
        {
            MessageItem mi = null;
            var td = new TestData { Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.Error,
                JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keys\": [ { } ] }"),
                td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("keys[0]", mi.Property);
            Assert.AreEqual("The JSON object must specify the 'code' token as required for the unique key.", mi.Text);
        }

        [Test]
        public void Merge_Property_Keys_ListItemNoChanges1()
        {
            var td = new TestData { Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def", Other = "123" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keys\": [ { \"code\": \"abc\", \"text\": \"def\" } ] }"), td));
            Assert.IsNotNull(td.Keys);
            Assert.AreEqual(1, td.Keys.Count);

            Assert.IsNotNull(td.Keys[0]);
            Assert.AreEqual("abc", td.Keys[0].Code);
            Assert.AreEqual("def", td.Keys[0].Text);
            Assert.AreEqual("123", td.Keys[0].Other);
        }

        [Test]
        public void Merge_Property_Keys_ListItemNoChanges2()
        {
            var td = new TestData { Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def", Other = "123" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keys\": [ { \"code\": \"abc\" } ] }"), td));
            Assert.IsNotNull(td.Keys);
            Assert.AreEqual(1, td.Keys.Count);

            Assert.IsNotNull(td.Keys[0]);
            Assert.AreEqual("abc", td.Keys[0].Code);
            Assert.AreEqual("def", td.Keys[0].Text);
            Assert.AreEqual("123", td.Keys[0].Other);
        }

        [Test]
        public void Merge_Property_Keys_ListItemWithChangesToExisting()
        {
            var td = new TestData { Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keys\": [ { \"code\": \"abc\", \"text\": \"xyz\" } ] }"), td));
            Assert.IsNotNull(td.Keys);
            Assert.AreEqual(1, td.Keys.Count);

            Assert.IsNotNull(td.Keys[0]);
            Assert.AreEqual("abc", td.Keys[0].Code);
            Assert.AreEqual("xyz", td.Keys[0].Text);
        }

        [Test]
        public void Merge_Property_Keys_ListItemChangedNew()
        {
            var td = new TestData { Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keys\": [ { \"code\": \"def\", \"text\": \"ghi\" } ] }"), td));
            Assert.IsNotNull(td.Keys);
            Assert.AreEqual(1, td.Keys.Count);

            Assert.IsNotNull(td.Keys[0]);
            Assert.AreEqual("def", td.Keys[0].Code);
            Assert.AreEqual("ghi", td.Keys[0].Text);
        }

        [Test]
        public void Merge_Property_KeysColl_Null()
        {
            MessageItem mi = null;
            var td = new TestData { KeysColl = new KeyDataCollection { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.Error,
                JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keyscoll\": [ null ] }"),
                td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("keyscoll[0]", mi.Property);
            Assert.AreEqual("The JSON token must be an object where Unique Key value(s) are required.", mi.Text);
        }

        [Test]
        public void Merge_Property_KeysColl_Empty()
        {
            MessageItem mi = null;
            var td = new TestData { KeysColl = new KeyDataCollection { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.Error,
                JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keyscoll\": [ { } ] }"),
                td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));

            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("keyscoll[0]", mi.Property);
            Assert.AreEqual("The JSON object must specify the 'code' token as required for the unique key.", mi.Text);
        }

        [Test]
        public void Merge_Property_KeysColl_ListItemNoChanges1()
        {
            var td = new TestData { KeysColl = new KeyDataCollection { new KeyData { Code = "abc", Text = "def", Other = "123" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keyscoll\": [ { \"code\": \"abc\", \"text\": \"def\" } ] }"), td));
            Assert.IsNotNull(td.KeysColl);
            Assert.AreEqual(1, td.KeysColl.Count);

            Assert.IsNotNull(td.KeysColl[0]);
            Assert.AreEqual("abc", td.KeysColl[0].Code);
            Assert.AreEqual("def", td.KeysColl[0].Text);
            Assert.AreEqual("123", td.KeysColl[0].Other);
        }

        [Test]
        public void Merge_Property_KeysColl_ListItemNoChanges2()
        {
            var td = new TestData { KeysColl = new KeyDataCollection { new KeyData { Code = "abc", Text = "def", Other = "123" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keyscoll\": [ { \"code\": \"abc\" } ] }"), td));
            Assert.IsNotNull(td.KeysColl);
            Assert.AreEqual(1, td.KeysColl.Count);

            Assert.IsNotNull(td.KeysColl[0]);
            Assert.AreEqual("abc", td.KeysColl[0].Code);
            Assert.AreEqual("def", td.KeysColl[0].Text);
            Assert.AreEqual("123", td.KeysColl[0].Other);
        }

        [Test]
        public void Merge_Property_KeysColl_ListItemWithChangesToExisting()
        {
            var td = new TestData { KeysColl = new KeyDataCollection { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keyscoll\": [ { \"code\": \"abc\", \"text\": \"xyz\" } ] }"), td));
            Assert.IsNotNull(td.KeysColl);
            Assert.AreEqual(1, td.KeysColl.Count);

            Assert.IsNotNull(td.KeysColl[0]);
            Assert.AreEqual("abc", td.KeysColl[0].Code);
            Assert.AreEqual("xyz", td.KeysColl[0].Text);
        }

        [Test]
        public void Merge_Property_KeysColl_ListItemChangedNew()
        {
            var td = new TestData { KeysColl = new KeyDataCollection { new KeyData { Code = "abc", Text = "def" } } };
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"keyscoll\": [ { \"code\": \"def\", \"text\": \"ghi\" } ] }"), td));
            Assert.IsNotNull(td.KeysColl);
            Assert.AreEqual(1, td.KeysColl.Count);

            Assert.IsNotNull(td.KeysColl[0]);
            Assert.AreEqual("def", td.KeysColl[0].Code);
            Assert.AreEqual("ghi", td.KeysColl[0].Text);
        }

        [Test]
        public void Merge_XtremeLoadTest_1000()
        {
            var text = "{ \"id\": \"13512759-4f50-e911-b35c-bc83850db74d\", \"name\": \"Barry\", \"isValid\": true, \"date\": \"2018-12-31\", \"count\": \"12\", \"amount\": 132.58, \"dict\": [ { \"k\": \"v\" }, { \"k2\": \"w2\" } ], "
                    + "\"values\": [ 1, 2, 4], \"sub\": { \"code\": \"abc\", \"text\": \"xyz\" }, \"nokeys\": [ { \"code\": \"abc\", \"text\": \"xyz\" }, null, { } ], "
                    + "\"keys\": [ { \"code\": \"abc\", \"text\": \"xyz\" }, { }, null ] }";

            for (int i = 0; i < 1000; i++)
            {
                var td = new TestData { Values = new int[] { 1, 2, 3 }, Keys = new List<KeyData> { new KeyData { Code = "abc", Text = "def" } } };
                JsonEntityMerge.Merge<TestData>(JObject.Parse(text), td);
            }
        }

        [Test]
        public void Merge_Property_RefData_UniqueKey()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"refers\": [ { \"code\": \"abc\", \"text\": \"xyz\" } ] }"), td));
            Assert.IsNotNull(td.Refers);
            Assert.AreEqual(1, td.Refers.Count);

            Assert.IsNotNull(td.Refers[0]);
            Assert.AreEqual("abc", td.Refers[0].CodeSid);
            Assert.AreEqual("xyz", td.Refers[0].Text);
        }

        [Test]
        public void Merge_Property_NonKeyData()
        {
            var td = new TestData();
            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nonkeys\": [ { \"code\": \"abc\", \"text\": \"xyz\" }, { \"code\": \"abc\", \"text\": \"uvw\"  } ] }"), td));
            Assert.IsNotNull(td.NonKeys);
            Assert.AreEqual(2, td.NonKeys.Count);

            Assert.IsNotNull(td.NonKeys[0]);
            Assert.AreEqual("abc", td.NonKeys[0].Code);
            Assert.AreEqual("xyz", td.NonKeys[0].Text);

            Assert.IsNotNull(td.NonKeys[1]);
            Assert.AreEqual("abc", td.NonKeys[1].Code);
            Assert.AreEqual("uvw", td.NonKeys[1].Text);
        }

        [Test]
        public void Merge_Property_NonKeyData_WithPrior()
        {
            var td = new TestData
            {
                NonKeys = new NonKeyDataCollection
                {
                    new NonKeyData { Code = "def", Text = "hij" }
                }
            };

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"nonkeys\": [ { \"code\": \"abc\", \"text\": \"xyz\" }, { \"code\": \"abc\", \"text\": \"uvw\"  } ] }"), td));
            Assert.IsNotNull(td.NonKeys);
            Assert.AreEqual(2, td.NonKeys.Count);

            Assert.IsNotNull(td.NonKeys[0]);
            Assert.AreEqual("abc", td.NonKeys[0].Code);
            Assert.AreEqual("xyz", td.NonKeys[0].Text);

            Assert.IsNotNull(td.NonKeys[1]);
            Assert.AreEqual("abc", td.NonKeys[1].Code);
            Assert.AreEqual("uvw", td.NonKeys[1].Text);
        }

        [Test]
        public void Merge_Property_Dict_Null()
        {
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": null }"), td));
            Assert.IsNull(td.Dict);
        }

        [Test]
        public void Merge_Property_Dict_Null2()
        {
            var td = new TestData();

            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": null }"), td));
            Assert.IsNull(td.Dict);
        }

        [Test]
        public void Merge_Property_Dict_Empty()
        {
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": [ ] }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(0, td.Dict.Count);
        }

        [Test]
        public void Merge_Property_Dict_Empty2()
        {
            var td = new TestData();

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": [ ] }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(0, td.Dict.Count);
        }

        [Test]
        public void Merge_Property_Dict_Single()
        {
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": { \"k\": \"w\" } }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(1, td.Dict.Count);
            Assert.IsTrue(td.Dict.ContainsKey("k"));
            Assert.AreEqual("w", td.Dict["k"]);
        }

        [Test]
        public void Merge_Property_Dict_Single2()
        {
            var td = new TestData();

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": { \"k\": \"w\" } }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(1, td.Dict.Count);
            Assert.IsTrue(td.Dict.ContainsKey("k"));
            Assert.AreEqual("w", td.Dict["k"]);
        }

        [Test]
        public void Merge_Property_Dict_Single_NoChanges()
        {
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.SuccessNoChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": { \"k\": \"v\" } }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(1, td.Dict.Count);
            Assert.IsTrue(td.Dict.ContainsKey("k"));
            Assert.AreEqual("v", td.Dict["k"]);
        }

        [Test]
        public void Merge_Property_Dict_Single_Malformed()
        {
            MessageItem mi = null;
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": { \"k\": [ \"v\" ] } }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));
            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("dict", mi.Property);
            Assert.AreEqual("The JSON token is malformed: Can not convert Array to String.", mi.Text);
        }

        [Test]
        public void Merge_Property_Dict_Single_Malformed2()
        {
            MessageItem mi = null;
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": 123 }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));
            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("dict", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }

        [Test]
        public void Merge_Property_Dict_Single_Malformed3()
        {
            MessageItem mi = null;
            var td = new TestData { Dict2 = new Dictionary<int, string> { { 123, "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict2\": { \"k\": \"v\" } }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));
            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("dict2", mi.Property);
            Assert.AreEqual("The JSON token is malformed: The value \"k\" is not of type \"System.Int32\" and cannot be used in this generic collection. (Parameter 'key')", mi.Text);
        }

        [Test]
        public void Merge_Property_Dict_Multi()
        {
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" }, { "k2", "v2" } } };

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": [ { \"k\": \"v\" }, { \"k2\": \"w2\" } ] }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(2, td.Dict.Count);
            Assert.IsTrue(td.Dict.ContainsKey("k"));
            Assert.AreEqual("v", td.Dict["k"]);
            Assert.IsTrue(td.Dict.ContainsKey("k2"));
            Assert.AreEqual("w2", td.Dict["k2"]);
        }

        [Test]
        public void Merge_Property_Dict_Multi2()
        {
            var td = new TestData();

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": [ { \"k\": \"v\" }, { \"k2\": \"w2\" } ] }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(2, td.Dict.Count);
            Assert.IsTrue(td.Dict.ContainsKey("k"));
            Assert.AreEqual("v", td.Dict["k"]);
            Assert.IsTrue(td.Dict.ContainsKey("k2"));
            Assert.AreEqual("w2", td.Dict["k2"]);
        }

        [Test]
        public void Merge_Property_Dict_Multi3()
        {
            var td = new TestData { Dict = new Dictionary<string, string> { { "a", "aa" }, { "b", "bb" } } };

            Assert.AreEqual(JsonEntityMergeResult.SuccessWithChanges, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": [ { \"b\": \"bb\" }, { \"a\": \"aa\" } ] }"), td));
            Assert.NotNull(td.Dict);
            Assert.AreEqual(2, td.Dict.Count);
            Assert.IsTrue(td.Dict.ContainsKey("a"));
            Assert.AreEqual("aa", td.Dict["a"]);
            Assert.IsTrue(td.Dict.ContainsKey("b"));
            Assert.AreEqual("bb", td.Dict["b"]);
        }

        [Test]
        public void Merge_Property_Dict_Multi_Malformed()
        {
            MessageItem mi = null;
            var td = new TestData { Dict = new Dictionary<string, string> { { "k", "v" } } };

            Assert.AreEqual(JsonEntityMergeResult.Error, JsonEntityMerge.Merge<TestData>(JObject.Parse("{ \"dict\": [ { \"k\": \"v\" }, 123 ] }"), td, new JsonEntityMergeArgs { LogAction = (m) => mi = m }));
            Assert.IsNotNull(mi);
            Assert.AreEqual(MessageType.Error, mi.Type);
            Assert.AreEqual("dict", mi.Property);
            Assert.AreEqual("The JSON token is malformed and could not be parsed.", mi.Text);
        }
    }
}