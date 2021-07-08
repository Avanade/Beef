using NUnit.Framework;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events.UnitTest.ContentSerializers
{
    [TestFixture]
    public class NewtonsoftJsonEventDataSerializerTest
    {
        [Test]
        public async Task EventDataEndToEnd()
        {
            var eds = new NewtonsoftJsonEventDataSerializer();
            var bytes = await eds.SerializeAsync(new EventData(CreateEventMetadata()));
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual("﻿{\"eventId\":\"00000001-0000-0000-0000-000000000000\",\"tenantId\":\"00000002-0000-0000-0000-000000000000\",\"subject\":\"Test.Subject\",\"action\":\"Created\",\"source\":\"/test\",\"key\":1,\"username\":\"Bob\",\"userid\":\"123\",\"timestamp\":\"2001-01-15T12:48:16Z\",\"correlationId\":\"XXX\",\"etag\":\"YYY\",\"partitionKey\":\"PK\"}", json);

            var ed = await eds.DeserializeAsync(bytes);
            AssertEventMetadata(ed);
        }

        [Test]
        public async Task EventDataTEndToEnd()
        {
            var eds = new NewtonsoftJsonEventDataSerializer();
            var bytes = await eds.SerializeAsync(new EventData<int>(CreateEventMetadata()) { Value = 88 });
            Assert.Greater(bytes.Length, 0);

            var json = Encoding.UTF8.GetString(bytes);
            Assert.AreEqual("﻿{\"value\":88,\"eventId\":\"00000001-0000-0000-0000-000000000000\",\"tenantId\":\"00000002-0000-0000-0000-000000000000\",\"subject\":\"Test.Subject\",\"action\":\"Created\",\"source\":\"/test\",\"key\":1,\"username\":\"Bob\",\"userid\":\"123\",\"timestamp\":\"2001-01-15T12:48:16Z\",\"correlationId\":\"XXX\",\"etag\":\"YYY\",\"partitionKey\":\"PK\"}", json);

            var ed = await eds.DeserializeAsync(typeof(int), bytes);
            AssertEventMetadata(ed);
            Assert.AreEqual(88, ed.GetValue());
            Assert.AreEqual(88, ((EventData<int>)ed).Value);
        }

        [Test]
        public async Task EventDataValueOnly()
        {
            var eds = new NewtonsoftJsonEventDataSerializer { SerializeValueOnly = true };
            var bytes = await eds.SerializeAsync(new EventData(CreateEventMetadata()));
            Assert.AreEqual(bytes.Length, 0);

            var ed = await eds.DeserializeAsync(bytes);
            Assert.NotNull(ed);
            Assert.Null(ed.Subject);
            Assert.IsFalse(ed.HasValue);
        }

        [Test]
        public async Task EventDataTValueOnly()
        {
            var eds = new NewtonsoftJsonEventDataSerializer { SerializeValueOnly = true };
            var bytes = await eds.SerializeAsync(new EventData<int>(CreateEventMetadata()) { Value = 88 });
            Assert.Greater(bytes.Length, 0);

            var ed = await eds.DeserializeAsync(typeof(int), bytes);
            Assert.NotNull(ed);
            Assert.Null(ed.Subject);
            Assert.IsTrue(ed.HasValue);
            Assert.AreEqual(88, ed.GetValue());
            Assert.AreEqual(88, ((EventData<int>)ed).Value);
        }

        public static EventMetadata CreateEventMetadata()
        {
            return new EventMetadata
            {
                Subject = "Test.Subject",
                Action = "Created",
                EventId = new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                CorrelationId = "XXX",
                Source = new Uri("/test", UriKind.Relative),
                TenantId = new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
                Timestamp = new DateTime(2001, 01, 15, 12, 48, 16, DateTimeKind.Utc),
                ETag = "YYY",
                Key = 1,
                PartitionKey = "PK",
                UserId = "123",
                Username = "Bob"
            };
        }

        public static void AssertEventMetadata(EventMetadata metadata, bool defaultPropertiesOnly = false, bool keyIsAString = false)
        {
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Test.Subject", metadata.Subject);
            Assert.AreEqual("Created", metadata.Action);
            if (defaultPropertiesOnly)
            {
                Assert.AreEqual(new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), metadata.EventId);
                Assert.AreEqual("XXX", metadata.CorrelationId);
                Assert.AreEqual(new Uri("/test", UriKind.Relative), metadata.Source);
                Assert.AreEqual(new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), metadata.TenantId);
                Assert.AreEqual(new DateTime(2001, 01, 15, 12, 48, 16, DateTimeKind.Utc), metadata.Timestamp);
                Assert.Null(metadata.ETag);
                Assert.Null(metadata.Key);
                Assert.Null(metadata.PartitionKey);
                Assert.AreEqual("123", metadata.UserId);
                Assert.AreEqual("Bob", metadata.Username);
            }
            else
            {
                Assert.AreEqual(new Guid(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), metadata.EventId);
                Assert.AreEqual("XXX", metadata.CorrelationId);
                Assert.AreEqual(new Uri("/test", UriKind.Relative), metadata.Source);
                Assert.AreEqual(new Guid(2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), metadata.TenantId);
                Assert.AreEqual(new DateTime(2001, 01, 15, 12, 48, 16, DateTimeKind.Utc), metadata.Timestamp);
                Assert.AreEqual("YYY", metadata.ETag);
                if (keyIsAString)
                    Assert.AreEqual("1", metadata.Key);
                else
                    Assert.AreEqual(1, metadata.Key);

                Assert.AreEqual("PK", metadata.PartitionKey);
                Assert.AreEqual("123", metadata.UserId);
                Assert.AreEqual("Bob", metadata.Username);
            }
        }
    }
}