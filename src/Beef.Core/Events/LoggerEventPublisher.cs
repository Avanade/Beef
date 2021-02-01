// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Represents an <see cref="ILogger"/> event publisher; whereby the events are simply logged then swallowed/discarded on send.
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="events"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        protected override async Task SendEventsAsync(params EventData[] events)
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