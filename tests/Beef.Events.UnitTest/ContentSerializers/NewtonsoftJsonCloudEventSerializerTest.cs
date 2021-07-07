using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events.UnitTest.ContentSerializers
{
    [TestFixture]
    public class NewtonsoftJsonCloudEventSerializerTest
    {
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class Person
        {
            [JsonProperty("first")]
            public string FirstName;
            [JsonProperty("last")]
            public string LastName;

            public string Private;

            public static Person Create() => new Person { FirstName = "Rebecca", LastName = "Brown", Private = "Top secret" };
        }

        [Test]
        public async Task EventDataEndToEndWithBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer { IncludeEventMetadataProperties = null };
            var bytes = await eds.SerializeAsync(new EventData(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()));
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{""specversion"":""1.0"",""type"":""Test.Subject.Created"",""source"":""/test"",""id"":""00000001-0000-0000-0000-000000000000"",""time"":""2001-01-15T12:48:16Z"",""tenantid"":""00000002-0000-0000-0000-000000000000"",""subject"":""Test.Subject"",""action"":""Created"",""key"":""1"",""username"":""Bob"",""userid"":""123"",""correlationid"":""XXX"",""etag"":""YYY"",""partitionkey"":""PK""}", json);

            var ed = await eds.DeserializeAsync(bytes);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(ed, keyIsAString: true);
        }

        [Test]
        public async Task EventDataTEndToEndWithBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer { IncludeEventMetadataProperties = null };
            var bytes = await eds.SerializeAsync(new EventData<Person>(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()) { Value = Person.Create() });
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{""specversion"":""1.0"",""type"":""Test.Subject.Created"",""source"":""/test"",""id"":""00000001-0000-0000-0000-000000000000"",""time"":""2001-01-15T12:48:16Z"",""datacontenttype"":""application/json"",""tenantid"":""00000002-0000-0000-0000-000000000000"",""subject"":""Test.Subject"",""action"":""Created"",""key"":""1"",""username"":""Bob"",""userid"":""123"",""correlationid"":""XXX"",""etag"":""YYY"",""partitionkey"":""PK"",""data"":{""first"":""Rebecca"",""last"":""Brown""}}", json);

            var ed = await eds.DeserializeAsync(typeof(Person), bytes);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(ed, keyIsAString: true);
            Assert.NotNull(ed.GetValue());

            var p = ((EventData<Person>)ed).Value;
            Assert.NotNull(p);
            Assert.AreEqual("Rebecca", p.FirstName);
            Assert.AreEqual("Brown", p.LastName);
            Assert.Null(p.Private);
        }

        [Test]
        public async Task EventDataEndToEndNoBeef()
        {
            var eds = new NewtonsoftJsonCloudEventSerializer { IncludeEventMetadata = false };
            var bytes = await eds.SerializeAsync(new EventData(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()));
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual(@"{""specversion"":""1.0"",""type"":""Test.Subject.Created"",""source"":""/test"",""id"":""00000001-0000-0000-0000-000000000000"",""time"":""2001-01-15T12:48:16Z""}", json);

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
            Assert.AreEqual(@"{""specversion"":""1.0"",""type"":""Test.Subject.Created"",""source"":""/test"",""id"":""00000001-0000-0000-0000-000000000000"",""time"":""2001-01-15T12:48:16Z"",""datacontenttype"":""application/json"",""data"":88}", json);

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