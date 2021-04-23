using Beef.Events;
using Xyz.Legacy.CdcPublisher.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Xyz.Legacy.CdcReceiver.Subscribers
{
    // EventSubscriberAttribute(s) define the subject and optional actions that uniquely link processing logic to subject.
    // Subscribers inherit from EventSubscriber or EventSubscriber<T> (specifying expected data type for automatic deserialization).
    [EventSubscriber("xyz.legacy.person.created")]
    [EventSubscriber("xyz.legacy.person.updated")]
    [EventSubscriber("xyz.legacy.person", "created", "updated")]
    public class PersonEditSubscriber : EventSubscriber<PersonCdc>
    {
        // Override the ReceiveAsync and implement logic; when finished return a Result.Success() to indicate successful execution. 
        public override Task<Result> ReceiveAsync(EventData<PersonCdc> eventData)
        {
            // Under normal circumstances there would be no need to log unless neccessary; this is for illustrative purposes only. Replace with logic to update system accordingly.
            Logger.LogInformation($"Subject: {eventData.Subject}, Action: {eventData.Action}, Source: {eventData.Source}, CorrelationId: {eventData.CorrelationId}, Value: {System.Environment.NewLine}{JsonConvert.SerializeObject(eventData.Value, Formatting.Indented)}");
            return Task.FromResult(Result.Success());
        }
    }
}