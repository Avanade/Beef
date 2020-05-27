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
        /// Creates a <see cref="SubscriberStatus.NotSubscribed"/> <see cref="Result"/>.
        /// </summary>
        /// <returns>The <see cref="SubscriberStatus.NotSubscribed"/> <see cref="Result"/>.</returns>
        internal static Result NotSubscribed()
            => new Result { Status = SubscriberStatus.NotSubscribed, Reason = "An EventSubscriber was not found." };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.ExceptionContinue"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/>.</param>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.ExceptionContinue"/> <see cref="Result"/>.</returns>
        internal static Result ExceptionContinue(System.Exception exception, string? reason)
            => new Result { Status = SubscriberStatus.ExceptionContinue, Exception = exception, Reason = reason ?? exception.Message };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.Success"/> <see cref="Result"/>.
        /// </summary>
        /// <returns>The <see cref="SubscriberStatus.Success"/> <see cref="Result"/>.</returns>
        public static Result Success() 
            => new Result { Status = SubscriberStatus.Success, Reason = "Success." };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.DataNotFound"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="reason">The optional reason.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overridding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.DataNotFound"/> <see cref="Result"/>.</returns>
        public static Result DataNotFound(string? reason = null, ResultHandling ? resultHandlingOverride = null)
            => new Result { Status = SubscriberStatus.DataNotFound, Reason = reason ?? new LText("Beef.NotFoundException"), ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="reason">The optional reason.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overridding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(string? reason = null, ResultHandling? resultHandlingOverride = null) 
            => new Result { Status = SubscriberStatus.InvalidData, Reason = reason ?? new LText("Beef.ValidationException"), ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/> from a set of <paramref name="messages"/>.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/>.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overridding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(MessageItemCollection messages, ResultHandling? resultHandlingOverride = null) 
            => new Result { Status = SubscriberStatus.InvalidData, Reason = Check.NotNull(messages, nameof(messages)).ToString(), ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/> from a <see cref="Beef.ValidationException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Beef.ValidationException"/>.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overridding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(Beef.ValidationException exception, ResultHandling? resultHandlingOverride = null) 
            => new Result { Status = SubscriberStatus.InvalidData, Reason = Check.NotNull(exception, nameof(exception)).Messages?.ToString() ?? exception?.Message, Exception = exception, ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/> from a <see cref="Beef.BusinessException"/>.
        /// </summary>
        /// <param name="exception">The <see cref="Beef.BusinessException"/>.</param>
        /// <param name="resultHandlingOverride">The <see cref="ResultHandling"/> where overridding the default behaviour.</param>
        /// <returns>The <see cref="SubscriberStatus.InvalidData"/> <see cref="Result"/>.</returns>
        public static Result InvalidData(Beef.BusinessException exception, ResultHandling? resultHandlingOverride = null)
            => new Result { Status = SubscriberStatus.InvalidData, Reason = Check.NotNull(exception, nameof(exception)).Message, Exception = exception, ResultHandling = resultHandlingOverride };

        /// <summary>
        /// Private contructor.
        /// </summary>
        private Result() { }

        /// <summary>
        /// Gets the event subject.
        /// </summary>
        internal string? Subject { get; set; }

        /// <summary>
        /// Gets the event action.
        /// </summary>
        internal string? Action { get; set; }

        /// <summary>
        /// Gets the corresponding subscriber.
        /// </summary>
        internal IEventSubscriber? Subscriber { get; set; }

        /// <summary>
        /// Gets the <see cref="SubscriberStatus"/>.
        /// </summary>
        public SubscriberStatus Status { get; private set; }

        /// <summary>
        /// Gets the reason text.
        /// </summary>
        public string? Reason { get; private set; }

        /// <summary>
        /// Gets the <see cref="ResultHandling"/> where specified (overridding the default).
        /// </summary>
        public ResultHandling? ResultHandling { get; private set; }

        /// <summary>
        /// Gets the corresponding <see cref="System.Exception"/>.
        /// </summary>
        public System.Exception? Exception { get; private set; }

        /// <summary>
        /// Outputs the <see cref="Result"/> as a <see cref="string"/>.
        /// </summary>
        /// <returns>The <see cref="Result"/> as a <see cref="string"/>.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Status: {Status}");

            if (!string.IsNullOrEmpty(Subject))
                sb.AppendLine($"Subject: {Subject}");

            if (!string.IsNullOrEmpty(Action))
                sb.AppendLine($"Action: {Action}");

            sb.Append($"Reason: {Reason}");
            return sb.ToString();
        }
    }
}