// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Represents an <see cref="ILogger"/> event publisher; whereby the events are simply logged then swallowed/discarded.
    /// </summary>
    public class LoggerEventPublisher : EventPublisherBase
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerEventPublisher"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public LoggerEventPublisher(ILogger<LoggerEventPublisher> logger) => _logger = Check.NotNull(logger, nameof(logger));

        /// <summary>
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="events">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected override async Task PublishEventsAsync(params EventData[] events)
        {
            if (events == null || events.Length == 0)
                _logger.LogInformation("No events were published.");
            else
            {
                foreach (var e in events)
                {
                    _logger.LogInformation($"Subject: {e.Subject}, Action: {e.Action}{(e.HasValue ? ", Value: " + JsonConvert.SerializeObject(e.GetValue()) : "")}");
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}