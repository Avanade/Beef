// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Text;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Represents the result of an <see cref="EventSubscriber.ReceiveAsync(EventData)"/> and <see cref="EventSubscriber{T}.ReceiveAsync(EventData{T})"/>. Note that where a subscriber throws a 
    /// <see cref="Beef.ValidationException"/> or <see cref="Beef.BusinessException"/> this will automatically result in an <see cref="SubscriberStatus.InvalidData"/>; likewise, a 
    /// <see cref="Beef.NotFoundException"/> will automatically result in a <see cref="SubscriberStatus.DataNotFound"/>.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Creates a <see cref="SubscriberStatus.Success"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.Success"/> <see cref="Result"/>.</returns>
        public static Result Success(string? reason = null) 
            => new Result { Status = SubscriberStatus.Success, Reason = reason ?? "Success." };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.DataNotFound"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="reason">The optional reason.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overriding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.DataNotFound"/> <see cref="Result"/>.</returns>
        public static Result DataNotFound(string? reason = null, ResultHandling ? resultHandlingOverride = null)
            => new Result { Status = SubscriberStatus.DataNotFound, Reason = reason ?? new LText("Beef.NotFoundException"), ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates an <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="reason">The optional reason.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overriding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(string? reason = null, ResultHandling? resultHandlingOverride = null) 
            => new Result { Status = SubscriberStatus.InvalidData, Reason = reason ?? new LText("Beef.ValidationException"), ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates an <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/> from a set of <paramref name="messages"/>.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/>.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overriding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(MessageItemCollection messages, ResultHandling? resultHandlingOverride = null) 
            => new Result { Status = SubscriberStatus.InvalidData, Reason = Check.NotNull(messages, nameof(messages)).ToString(), ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates an <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/> from a <see cref="Beef.ValidationException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Beef.ValidationException"/>.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overriding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(Beef.ValidationException exception, ResultHandling? resultHandlingOverride = null) 
            => new Result { Status = SubscriberStatus.InvalidData, Reason = Check.NotNull(exception, nameof(exception)).Messages?.ToString() ?? exception?.Message, Exception = exception, ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates an <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/> from a <see cref="Beef.BusinessException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Beef.BusinessException"/>.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overriding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(Beef.BusinessException exception, ResultHandling? resultHandlingOverride = null)
            => new Result { Status = SubscriberStatus.InvalidData, Reason = Check.NotNull(exception, nameof(exception)).Message, Exception = exception, ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        internal Result() { }

        /// <summary>
        /// Gets or sets the event subject.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the event action.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or set the corresponding subscriber.
        /// </summary>
        public IEventSubscriber? Subscriber { get; set; }

        /// <summary>
        /// Gets the <see cref="SubscriberStatus"/>.
        /// </summary>
        public SubscriberStatus Status { get; internal set; }

        /// <summary>
        /// Gets the reason text.
        /// </summary>
        public string? Reason { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> where specified (overriding the default).
        /// </summary>
        public ResultHandling? ResultHandling { get; internal set; }

        /// <summary>
        /// Gets the corresponding <see cref="System.Exception"/>.
        /// </summary>
        public System.Exception? Exception { get; internal set; }

        /// <summary>
        /// Outputs the <see cref="Result"/> as a <see cref="string"/>.
        /// </summary>
        /// <returns>The <see cref="Result"/> as a <see cref="string"/>.</returns>
        public override string ToString() => $"Status: {Status}, Handling: {ResultHandling}, Subject: {Subject}, Action: {Action}, Reason: {Reason}{(Exception == null ? "" : $", Exception: {Exception.Message}")}";

        /// <summary>
        /// Outputs the <see cref="Result"/> as a multi-line <see cref="string"/>.
        /// </summary>
        /// <returns>The <see cref="Result"/> as a multi-line <see cref="string"/>.</returns>
        public string ToMultiLineString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Status: {Status}");
            sb.AppendLine($"Subject: {Subject}");
            sb.AppendLine($"Action: {Action}");
            sb.AppendLine($"Reason: {Reason}");
            if (Exception != null)
                sb.AppendLine($"Exception: {Exception}");

            return sb.ToString();
        }
    }
}