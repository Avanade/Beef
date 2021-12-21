using Beef.Events.EventHubs;
using Beef.Events.ServiceBus;
using Beef.Events.UnitTest.ContentSerializers;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using AzureEventHubs = Azure.Messaging.EventHubs;
using AzureServiceBus = Azure.Messaging.ServiceBus;

namespace Beef.Events.UnitTest.Converters
{
    [TestFixture]
    public partial class AzureEventHubsEventConverterTest
    {
        [Test]
        public Task EndToEndWithMessagingProperties() => new EventDataConverterTester<EventHubsEventConverter, AzureEventHubs.EventData>().EndToEnd(new EventHubsEventConverter() { UseMessagingPropertiesForMetadata = true },
            ce => Assert.AreEqual("Test.Subject", ce.Properties[EventMetadata.SubjectAttributeName]), false);

        [Test]
        public Task EndToEndValueWithMessagingProperties() => new EventDataConverterTester<EventHubsEventConverter, AzureEventHubs.EventData>().EndToEndValue(new EventHubsEventConverter() { UseMessagingPropertiesForMetadata = true },
            ce => Assert.AreEqual("Test.Subject", ce.Properties[EventMetadata.SubjectAttributeName]), false);

        [Test]
        public Task EndToEndWithoutMessagingProperties() => new EventDataConverterTester<EventHubsEventConverter, AzureEventHubs.EventData>().EndToEnd(new EventHubsEventConverter() { UseMessagingPropertiesForMetadata = false },
            ce => Assert.False(ce.Properties.ContainsKey(EventMetadata.SubjectAttributeName)), true);

        [Test]
        public Task EndToEndValueWithoutMessagingProperties() => new EventDataConverterTester<EventHubsEventConverter, AzureEventHubs.EventData>().EndToEndValue(new EventHubsEventConverter() { UseMessagingPropertiesForMetadata = false },
            ce => Assert.False(ce.Properties.ContainsKey(EventMetadata.SubjectAttributeName)), true);
    }

    [TestFixture]
    public class AzureServiceBusMessageConverterTest
    {
        [Test]
        public Task EndToEndWithMessagingProperties() => new EventDataConverterTester<ServiceBusMessageConverter, AzureServiceBus.ServiceBusMessage>().EndToEnd(new ServiceBusMessageConverter() { UseApplicationPropertiesForMetadata = true },
            ce => Assert.AreEqual("Test.Subject", ce.ApplicationProperties[EventMetadata.SubjectAttributeName]), false);

        [Test]
        public Task EndToEndValueWithMessagingProperties() => new EventDataConverterTester<ServiceBusMessageConverter, AzureServiceBus.ServiceBusMessage>().EndToEndValue(new ServiceBusMessageConverter() { UseApplicationPropertiesForMetadata = true },
            ce => Assert.AreEqual("Test.Subject", ce.ApplicationProperties[EventMetadata.SubjectAttributeName]), false);

        [Test]
        public Task EndToEndWithoutMessagingProperties() => new EventDataConverterTester<ServiceBusMessageConverter, AzureServiceBus.ServiceBusMessage>().EndToEnd(new ServiceBusMessageConverter() { UseApplicationPropertiesForMetadata = false },
            ce => Assert.False(ce.ApplicationProperties.ContainsKey(EventMetadata.SubjectAttributeName)), true);

        [Test]
        public Task EndToEndValueWithoutMessagingProperties() => new EventDataConverterTester<ServiceBusMessageConverter, AzureServiceBus.ServiceBusMessage>().EndToEndValue(new ServiceBusMessageConverter() { UseApplicationPropertiesForMetadata = false },
            ce => Assert.False(ce.ApplicationProperties.ContainsKey(EventMetadata.SubjectAttributeName)), true);
    }

    public class EventDataConverterTester<T, CT> where T : IEventDataConverter<CT> where CT : class
    {
        public async Task EndToEnd(T edc, Action<CT> action, bool defaultPropertiesOnly)
        {
            var ed = new EventData(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata());
            var ce = await edc.ConvertToAsync(ed);
            Assert.NotNull(ce);

            action(ce);

            var md = await edc.GetMetadataAsync(ce);
            Assert.NotNull(md);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(md, defaultPropertiesOnly);

            var ed2 = await edc.ConvertFromAsync(ce);
            Assert.NotNull(ed2);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(ed2, defaultPropertiesOnly);
        }

        public async Task EndToEndValue(T edc, Action<CT> action, bool defaultPropertiesOnly)
        {
            var ed = new EventData<int>(NewtonsoftJsonEventDataSerializerTest.CreateEventMetadata()) { Value = 88 };
            var ce = await edc.ConvertToAsync(ed);
            Assert.NotNull(ce);

            action(ce);

            var md = await edc.GetMetadataAsync(ce);
            Assert.NotNull(md);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(md, defaultPropertiesOnly);

            var ed2 = await edc.ConvertFromAsync(typeof(int), ce);
            Assert.NotNull(ed2);
            NewtonsoftJsonEventDataSerializerTest.AssertEventMetadata(ed2, defaultPropertiesOnly);
            Assert.True(ed2.HasValue);
            Assert.AreEqual(88, ed2.GetValue());
            Assert.AreEqual(88, ((EventData<int>)ed2).Value);
        }
    }
}