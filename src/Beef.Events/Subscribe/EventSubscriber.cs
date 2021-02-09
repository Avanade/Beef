// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Provides the shared/base capabilities for an <see cref="EventData"/> subscriber with no corresponding value. Note that the inheriting/implementing class <b>must</b> also be decorated with the
    /// <see cref="EventSubscriberAttribute"/> to specify the corresponding <see cref="EventSubscriberAttribute.SubjectTemplate"/> and <see cref="EventSubscriberAttribute.Actions"/>.
    /// </summary>
    public abstract class EventSubscriberBase : IEventSubscriber
    {
        /// <summary>
        /// Determines the <see cref="Subscribe.RunAsUser"/>; defaults to <see cref="RunAsUser.Originating"/>.
        /// </summary>
        public RunAsUser RunAsUser { get; protected set; } = RunAsUser.Originating;

        /// <summary>
        /// Determines the <see cref="Subscribe.UnhandledExceptionHandling"/> option; defaults to <see cref="UnhandledExceptionHandling.ThrowException"/>.
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
        /// Gets the value <see cref="Type"/>; <c>null</c> indicates no value.
        /// </summary>
        public abstract Type? ValueType { get; }

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
        /// Receive and process the <see cref="EventData"/> (internally casts the <paramref name="eventData"/> to <see cref="EventData{T}"/> and invokes <see cref="ReceiveAsync(EventData{T})"/>).
        /// </summary>
        /// <param name="eventData">The <see cref="EventData"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        public override Task<Result> ReceiveAsync(EventData eventData)
        {
            EventData<T> ed;

            try
            {
                ed = (EventData<T>)eventData;
            }
            catch (InvalidCastException icex) { return Task.FromResult(Result.InvalidEventData(icex)); }

            return ReceiveAsync(ed);
        }

        /// <summary>
        /// Receive and process the <see cref="EventData{T}"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventData{T}"/>.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        public abstract Task<Result> ReceiveAsync(EventData<T> eventData);
    }
}