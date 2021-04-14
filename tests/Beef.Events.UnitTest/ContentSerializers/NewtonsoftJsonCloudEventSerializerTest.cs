using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events.UnitTest.ContentSerializers
{
    [TestFixture]
    public class NewtonsoftJsonCloudEventSerializerTest
    {
        [Test]
        public async Task EventDataEndToEndWithBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer();
            var bytes = await eds.SerializeAsync(new EventData(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()));
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{
  ""specversion"": ""1.0"",
  ""type"": ""Test.Subject.Created"",
  ""source"": ""/test"",
  ""id"": ""00000001-0000-0000-0000-000000000000"",
  ""time"": ""2001-01-15T12:48:16"",
  ""beef_subject"": ""Test.Subject"",
  ""beef_action"": ""Created"",
  ""beef_tenantid"": ""00000002-0000-0000-0000-000000000000"",
  ""beef_key"": 1,
  ""beef_etag"": ""YYY"",
  ""beef_username"": ""Bob"",
  ""beef_userid"": ""123"",
  ""beef_correlationid"": ""XXX"",
  ""beef_partitionkey"": ""PK""
}", json);

            var ed = await eds.DeserializeAsync(bytes);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(ed);
        }

        [Test]
        public async Task EventDataTEndToEndWithBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer();
            var bytes = await eds.SerializeAsync(new EventData<int>(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()) { Value = 88 });
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{
  ""specversion"": ""1.0"",
  ""type"": ""Test.Subject.Created"",
  ""source"": ""/test"",
  ""id"": ""00000001-0000-0000-0000-000000000000"",
  ""time"": ""2001-01-15T12:48:16"",
  ""datacontenttype"": ""application/json"",
  ""data"": ""88"",
  ""beef_subject"": ""Test.Subject"",
  ""beef_action"": ""Created"",
  ""beef_tenantid"": ""00000002-0000-0000-0000-000000000000"",
  ""beef_key"": 1,
  ""beef_etag"": ""YYY"",
  ""beef_username"": ""Bob"",
  ""beef_userid"": ""123"",
  ""beef_correlationid"": ""XXX"",
  ""beef_partitionkey"": ""PK""
}", json);

            var ed = await eds.DeserializeAsync(typeof(int), bytes);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(ed);
            Assert.AreEqual(88, ed.GetValue());
            Assert.AreEqual(88, ((EventData<int>)ed).Value);
        }

        [Test]
        public async Task EventDataEndToEndNoBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer { IncludeEventMetadata = false };
            var bytes = await eds.SerializeAsync(new EventData(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()));
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{
  ""specversion"": ""1.0"",
  ""type"": ""Test.Subject.Created"",
  ""source"": ""/test"",
  ""id"": ""00000001-0000-0000-0000-000000000000"",
  ""time"": ""2001-01-15T12:48:16""
}", json);

            var ed = await eds.DeserializeAsync(bytes);
            AssertPartialEventMetadata(ed);
        }

        [Test]
        public async Task EventDataTEndToEndNoBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer { IncludeEventMetadata = false };
            var bytes = await eds.SerializeAsync(new EventData<int>(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()) { Value = 88 });
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{
  ""specversion"": ""1.0"",
  ""type"": ""Test.Subject.Created"",
  ""source"": ""/test"",
  ""id"": ""00000001-0000-0000-0000-000000000000"",
  ""time"": ""2001-01-15T12:48:16"",
  ""datacontenttype"": ""application/json"",
  ""data"": ""88""
}", json);

            var ed = await eds.DeserializeAsync(typeof(int), bytes);
            AssertPartialEventMetadata(ed);
            Assert.AreEqual(88, ed.GetValue());
            Assert.AreEqual(88, ((EventData<int>)ed).Value);
        }

        public static void AssertPartialEventMetadata(EventMetadata metadata)
        {
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Test.Subject.Created", metadata.Subject);
            Assert.AreEqual(new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), metadata.EventId);
            Assert.AreEqual(new Uri("/test", UriKind.Relative), metadata.Source);
            Assert.AreEqual(new DateTime(2001, 01, 15, 12, 48, 16), metadata.Timestamp);
            Assert.Null(metadata.Action);
            Assert.Null(metadata.CorrelationId);
            Assert.Null(metadata.TenantId);
            Assert.Null(metadata.ETag);
            Assert.Null(metadata.Key);
            Assert.Null(metadata.PartitionKey);
            Assert.Null(metadata.UserId);
            Assert.Null(metadata.Username);
        }
    }
}