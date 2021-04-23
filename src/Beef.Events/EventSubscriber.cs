// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the shared/base capabilities for an <see cref="EventData"/> subscriber with no corresponding value. Note that the inheriting/implementing class <b>must</b> also be decorated with the
    /// <see cref="EventSubscriberAttribute"/> to specify the corresponding <see cref="EventSubscriberAttribute.SubjectTemplate"/> and <see cref="EventSubscriberAttribute.Actions"/>.
    /// </summary>
    public abstract class EventSubscriberBase : IEventSubscriber
    {
        private ILogger? _logger;
        private IEventSubscriberData? _originatingData;

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger 
        { 
            get => _logger ?? throw new InvalidOperationException("The Logger instance must be set prior to use."); 
            set => _logger = value ?? throw new ArgumentNullException(nameof(value)); 
        }

        /// <summary>
        /// Determines the <see cref="Events.RunAsUser"/>; defaults to <see cref="RunAsUser.Originating"/>.
        /// </summary>
        public RunAsUser RunAsUser { get; protected set; } = RunAsUser.Originating;

        /// <summary>
        /// Determines the <see cref="Events.UnhandledExceptionHandling"/> option; defaults to <see cref="UnhandledExceptionHandling.ThrowException"/>.
        /// </summary>
        public UnhandledExceptionHandling UnhandledExceptionHandling { get; protected set; } = UnhandledExceptionHandling.ThrowException;

        /// <summary>
        /// Gets or sets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidEventData"/> status (overrides <see cref="EventSubscriberHostArgs.InvalidEventDataHandling"/>).
        /// </summary>
        public ResultHandling? InvalidEventDataHandling { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.DataNotFound"/> status (overrides <see cref="EventSubscriberHostArgs.DataNotFoundHandling"/>).
        /// </summary>
        public ResultHandling? DataNotFoundHandling { get; set; }

        /// <summary>
        /// Gets or sets the the <see cref="ResultHandling"/> for a <see cref="Result"/> with a <see cref="SubscriberStatus.InvalidData"/> status (overrides <see cref="EventSubscriberHostArgs.InvalidDataHandling"/>).
        /// </summary>
        public ResultHandling? InvalidDataHandling { get; set;  }

        /// <summary>
        /// Gets or sets the maximum number of attempts before the event is automatically skipped.
        /// </summary>
        /// <remarks>This functionality is dependent on the <see cref="EventSubscriberHost"/> providing the functionality to check and action.</remarks>
        public int? MaxAttempts { get; set; }

        /// <summary>
        /// Gets the value <see cref="Type"/>; <c>null</c> indicates no value.
        /// </summary>
        public abstract Type? ValueType { get; }

        /// <summary>
        /// Indicates whether a <c>null</c> value is considered invalid data and a corresponding <see cref="Result.InvalidData(BusinessException, ResultHandling?)"/> should automatically result.
        /// </summary>
        public abstract bool ConsiderNullValueAsInvalidData { get; set; }

        /// <summary>
        /// Gets or sets the originating <see cref="IEventSubscriberData"/> (for internal use only).
        /// </summary>
        IEventSubscriberData? IEventSubscriber.OriginatingData { get => _originatingData; set => _originatingData = value ?? throw new ArgumentNullException(nameof(IEventSubscriber.OriginatingData)); }

        /// <summary>
        /// Gets the originating <see cref="IEventSubscriberData"/>.
        /// </summary>
        public IEventSubscriberData GetOriginatingData() => _originatingData ?? throw new InvalidOperationException("The OriginatingData currently does not have a value; this must be configured prior to access.");

        /// <summary>
        /// Receive and process the <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        public abstract Task<Result> ReceiveAsync(EventData eventData);
    }

    /// <summary>
    /// Provides the base capabilities for an <see cref="EventData"/> subscriber with no corresponding value. Note that the inheriting/implementing class <b>must</b> also be decorated with the
    /// <see cref="EventSubscriberAttribute"/> to specify the corresponding <see cref="EventSubscriberAttribute.SubjectTemplate"/> and <see cref="EventSubscriberAttribute.Actions"/>.
    /// </summary>
    /// <remarks></remarks>
    public abstract class EventSubscriber : EventSubscriberBase
    {
        /// <summary>
        /// Gets the value <see cref="Type"/>; always <c>null</c> as there will not be one.
        /// </summary>
        public override Type? ValueType { get => null; }

        /// <summary>
        /// Indicates whether a <c>null</c> value is considered invalid data and a corresponding <see cref="Result.InvalidData(BusinessException, ResultHandling?)"/> should automatically result.
        /// </summary>
        /// <remarks>Always is <c>false</c> as there is no <see cref="ValueType"/>.</remarks>
        public override bool ConsiderNullValueAsInvalidData { get => false; set => _ = value ? throw new NotSupportedException("Must always be 'false' where no underlying Value is supported.") : value; }

        /// <summary>
        /// Receive and process the <see cref="EventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        public override abstract Task<Result> ReceiveAsync(EventData eventData);
    }

    /// <summary>
    /// Provides the base capabilities for an <see cref="EventData"/> subscriber with a typed <typeparamref name="T"/> value. Note that the inheriting/implementing class <b>must</b> also be decorated with the
    /// <see cref="EventSubscriberAttribute"/> to specify the corresponding <see cref="EventSubscriberAttribute.SubjectTemplate"/> and <see cref="EventSubscriberAttribute.Actions"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="EventData{T}"/> <see cref="Type"/>.</typeparam>
    public abstract class EventSubscriber<T> : EventSubscriberBase
    {
        /// <summary>
        /// Gets the value <see cref="Type"/>.
        /// </summary>
        public override Type? ValueType { get => typeof(T); }

        /// <summary>
        /// Indicates whether a <c>null</c> value is considered invalid data and a corresponding <see cref="Result.InvalidData(BusinessException, ResultHandling?)"/> should automatically result.
        /// </summary>
        /// <remarks>Defaults to <c>true</c>.</remarks>
        public override bool ConsiderNullValueAsInvalidData { get; set; } = true;

        /// <summary>
        /// Receive and process the <see cref="EventData"/> (internally casts the <paramref name="eventData"/> to <see cref="EventData{T}"/> and invokes <see cref="ReceiveAsync(EventData{T})"/>).
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        public async override Task<Result> ReceiveAsync(EventData eventData)
        {
            EventData<T> ed;

            try
            {
                ed = (EventData<T>)eventData;
            }
            catch (InvalidCastException icex) { return EventSubscriberHost.CreateInvalidEventDataResult(icex); }

            return await ReceiveAsync(ed).ConfigureAwait(false);
        }

        /// <summary>
        /// Receive and process the <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData{T}"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        public abstract Task<Result> ReceiveAsync(EventData<T> eventData);
    }
}