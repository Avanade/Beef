// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the typed <see cref="EventData"/> subscriber capabilities.
    /// </summary>
    /// <typeparam name="TOriginating">The originating event/message <see cref="Type"/>.</typeparam>
    /// <typeparam name="TData">The event data <see cref="Type"/>.</typeparam>
    /// <typeparam name="THost">This <see cref="Type"/> for fluent-style method-chaining support.</typeparam>
    public abstract class EventSubscriberHost<TOriginating, TData, THost> : EventSubscriberHost
        where TOriginating : class
        where TData : EventSubscriberData<TOriginating>
        where THost : EventSubscriberHost<TOriginating, TData, THost>
    {
        private InvokerBase<TData, Result>? _invoker;

        private class EventSubscriberHostInvoker : InvokerBase<TData, Result> { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHost{TOriginating, TData, THost}"/> with the specified <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        /// <param name="args">The optional <see cref="EventSubscriberHostArgs"/>.</param>
        /// <param name="eventDataConverter">The <see cref="IEventDataConverter{TOriginating}"/>.</param>
        public EventSubscriberHost(EventSubscriberHostArgs args, IEventDataConverter<TOriginating> eventDataConverter) : base(args) => EventDataConverter = Check.NotNull(eventDataConverter, nameof(eventDataConverter));

        /// <summary>
        /// Gets the <see cref="IEventDataConverter{TOriginating}"/>.
        /// </summary>
        public IEventDataConverter<TOriginating> EventDataConverter { get; }

        /// <summary>
        /// Gets or sets the <see cref="InvokerBase{TData, Result}"/>.
        /// </summary>
        /// <remarks>Defaults to an internal invoker where not specified.</remarks>
        public InvokerBase<TData, Result> Invoker { get => _invoker ??= new EventSubscriberHostInvoker(); set => _invoker = value ?? throw new ArgumentNullException(nameof(value)); }

        /// <summary>
        /// Use (set) the <see cref="Invoker"/>.
        /// </summary>
        /// <param name="invoker">The <see cref="InvokerBase{TData}"/>.</param>
        /// <returns>This <typeparamref name="THost"/> instance (for fluent-style method chaining).</returns>
        public THost UseInvoker(InvokerBase<TData, Result> invoker)
        {
            Invoker = invoker;
            return (THost)this;
        }

        /// <summary>
        /// Use (set) the <see cref="EventSubscriberHost.Logger"/>.
        /// </summary>
        /// <returns>This <typeparamref name="THost"/> instance (for fluent-style method chaining).</returns>
        public THost UseLogger(ILogger logger)
        {
            Logger = logger;
            return (THost)this;
        }

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="IEventSubscriberData"/>.
        /// </summary>
        /// <param name="data">The <see cref="IEventSubscriberData"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>.</returns>
        protected override async Task<(EventMetadata? Metadata, Exception? Exception)> GetMetadataAsync(IEventSubscriberData data)
        {
            try
            {
                return (await EventDataConverter.GetMetadataAsync((TOriginating)data.Originating).ConfigureAwait(false), null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        /// <summary>
        /// Receives the message and processes.
        /// </summary>
        /// <param name="data">The event data.</param>
        public Task<Result> ReceiveAsync(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return Invoker.InvokeAsync(this, async () =>
            {
                // Invoke the base EventSubscriberHost.ReceiveAsync to do the actual work!
                return await ReceiveAsync(data, async (subscriber) =>
                {
                    // Convert/get the beef event data.
                    try
                    {
                        return subscriber.ValueType == null 
                            ? await EventDataConverter.ConvertFromAsync(data.Originating).ConfigureAwait(false) 
                            : await EventDataConverter.ConvertFromAsync(subscriber.ValueType, data.Originating).ConfigureAwait(false);
                    }
                    catch (Exception ex) { throw new EventSubscriberUnhandledException(CreateInvalidEventDataResult(ex)); }
                }).ConfigureAwait(false);
            }, data);
        }
    }

    /// <summary>
    /// Provides the base <see cref="EventData"/> subscriber capabilities.
    /// </summary>
    public abstract class EventSubscriberHost
    {
        private ILogger? _logger;

        /// <summary>
        /// Gets or sets the <see cref="RunAsUser.System"/> username (this defaults to <see cref="ExecutionContext"/> <see cref="ExecutionContext.EnvironmentUsername"/>).
        /// </summary>
        public static string SystemUsername { get; set; } = ExecutionContext.EnvironmentUsername;

        /// <summary>
        /// Gets the username based on the <see cref="IEventSubscriber.RunAsUser"/> (see also <see cref="SystemUsername"/>).
        /// </summary>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> that will process the message.</param>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <returns>The username.</returns>
        protected static string GetUsername(IEventSubscriber subscriber, EventData @event)
            => Check.NotNull(subscriber, nameof(subscriber)).RunAsUser == RunAsUser.Originating ? Check.NotNull(@event, nameof(@event)).Username ?? SystemUsername : SystemUsername;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSubscriberHost"/>.
        /// </summary>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        protected EventSubscriberHost(EventSubscriberHostArgs args) => Args = Check.NotNull(args, nameof(args));

        /// <summary>
        /// Gets the <see cref="EventSubscriberHostArgs"/>.
        /// </summary>
        public EventSubscriberHostArgs Args { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/>.
        /// </summary>
        /// <remarks>Where the <see cref="AuditWriter"/> implements <see cref="IUseLogger"/> then the underlying <see cref="IUseLogger.UseLogger(ILogger)"/> will be invoked on set.</remarks>
        public ILogger Logger
        {
            get => _logger ?? throw new InvalidOperationException("Logger has not been set; the implementing class must set prior to any use.");

            protected set
            {
                _logger = value ?? throw new ArgumentNullException(nameof(value));
                if (AuditWriter is IUseLogger ul)
                    ul.UseLogger(_logger);
            }
        }

        /// <summary>
        /// Gets the <see cref="IAuditWriter"/> (value from <see cref="Args"/> <see cref="EventSubscriberHostArgs.AuditWriter"/>).
        /// </summary>
        public IAuditWriter? AuditWriter => Args.AuditWriter;

        /// <summary>
        /// Gets the maximum number of attempts before the event is automatically skipped (value from <see cref="Args"/> <see cref="EventSubscriberHostArgs.MaxAttempts"/>).
        /// </summary>
        /// <remarks>This functionality is dependent on the <see cref="EventSubscriberHost"/> providing the functionality to check and action.</remarks>
        public int? MaxAttempts => Args.MaxAttempts;

        /// <summary>
        /// Gets the <see cref="EventMetadata"/> from the <see cref="IEventSubscriberData"/>.
        /// </summary>
        /// <param name="data">The <see cref="IEventSubscriberData"/>.</param>
        /// <returns>The <see cref="EventMetadata"/>; a <c>null</c> indicates that there was a conversion error.</returns>
        protected abstract Task<(EventMetadata? Metadata, Exception? Exception)> GetMetadataAsync(IEventSubscriberData data);

        /// <summary>
        /// Receives the message and processes when the <see cref="EventMetadata.Subject"/> and <see cref="EventMetadata.Action"/> has been subscribed.
        /// </summary>
        /// <param name="data">The originating <see cref="IEventSubscriberData"/> (required to enable <see cref="IAuditWriter.WriteAuditAsync(IEventSubscriberData, Result)"/>).</param>
        /// <param name="getEventData">The function to get the corresponding <see cref="EventData"/> or <see cref="EventData{T}"/> only performed where subscribed for processing.</param>
        /// <returns>The <see cref="Result"/>.</returns>
        /// <remarks>This method also manages the Dependency Injection (DI) scope for each event execution (see <see cref="ServiceProviderServiceExtensions.CreateScope(IServiceProvider)"/>).</remarks>
        protected async Task<Result> ReceiveAsync(IEventSubscriberData data, Func<IEventSubscriber, Task<EventData>> getEventData)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (getEventData == null)
                throw new ArgumentNullException(nameof(getEventData));

            var md = await GetMetadataAsync(data).ConfigureAwait(false);
            if (md.Metadata == null)
                return await CheckResultAsync(data, CreateInvalidEventDataResult(md.Exception, "EventData is invalid; unable to convert EventData from the originating value."), null).ConfigureAwait(false);

            data.SetMetadata(md.Metadata);
            if (string.IsNullOrEmpty(data.Metadata.Subject))
                return await CheckResultAsync(data, CreateInvalidEventDataResult(null, "EventData is invalid; Subject is required."), null).ConfigureAwait(false);

            // Manages a Dependency Injection (DI) scope for each event execution.
            using (Args.ServiceProvider.CreateScope())
            {
                // Match a subscriber to the subject + template supplied.
                var subscriber = Args.CreateEventSubscriber(Args.SubjectTemplateWildcard, Args.SubjectPathSeparator, data.Metadata.Subject, data.Metadata.Action);
                if (subscriber == null)
                    return await CheckResultAsync(data, CreateNotSubscribedResult(), null).ConfigureAwait(false);

                subscriber.Logger = Logger;

                // Where matched get the EventData and execute the subscriber receive.
                EventData @event;
                try
                {
                    @event = await getEventData(subscriber).ConfigureAwait(false);
                    if (@event == null)
                        return await CheckResultAsync(data, CreateInvalidEventDataResult(null, $"EventData is invalid; is required."), subscriber).ConfigureAwait(false);

                    if (subscriber.ConsiderNullValueAsInvalidData && @event.GetValue() == null)
                        return await CheckResultAsync(data, CreateInvalidEventDataResult(null, $"EventData is invalid; Value must not be null."), subscriber).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Handle the exception as per the subscriber configuration.
                    return await CheckResultAsync(data, CreateInvalidEventDataResult(ex), subscriber).ConfigureAwait(false);
                }

                try
                {
                    // Create and set the execution context for the event.
                    ExecutionContext.Reset();
                    var ec = Args.ServiceProvider.GetService<ExecutionContext>();
                    if (Args.UpdateExecutionContext == null)
                        UpdateExecutionContext(ec, subscriber, @event);
                    else
                        Args.UpdateExecutionContext(ec, subscriber, @event);

                    ec.ServiceProvider = Args.ServiceProvider;
                    ec.CorrelationId = @event.CorrelationId;
                    ExecutionContext.SetCurrent(ec);

                    // Process the event.
                    subscriber.OriginatingData = data;
                    return await CheckResultAsync(data, await subscriber.ReceiveAsync(@event).ConfigureAwait(false), subscriber).ConfigureAwait(false);
                }
                catch (InvalidEventDataException iedex) { return await CheckResultAsync(data, CreateInvalidEventDataResult(iedex), subscriber).ConfigureAwait(false); }
                catch (ValidationException vex) { return await CheckResultAsync(data, Result.InvalidData(vex), subscriber).ConfigureAwait(false); }
                catch (BusinessException bex) { return await CheckResultAsync(data, Result.InvalidData(bex), subscriber).ConfigureAwait(false); }
                catch (NotFoundException) { return await CheckResultAsync(data, Result.DataNotFound(), subscriber).ConfigureAwait(false); }
                catch (EventSubscriberUnhandledException) { throw; }
                catch (Exception ex) { return await CheckResultAsync(data, CreateUnhandledExceptionResult(ex), subscriber).ConfigureAwait(false); }
            }
        }

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.InvalidEventData"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/>.</param>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.NotSubscribed"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreateInvalidEventDataResult(System.Exception? exception, string? reason = null)
            => new Result { Status = SubscriberStatus.InvalidEventData, Reason = reason ?? (exception == null ? null : $"EventData is invalid: {exception.Message}") ?? "EventData is invalid.", Exception = exception };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.NotSubscribed"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.NotSubscribed"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreateNotSubscribedResult(string? reason = null)
            => new Result { Status = SubscriberStatus.NotSubscribed, Reason = reason ?? "An EventSubscriber was not found." };

        /// <summary>
        /// Creates an <see cref="SubscriberStatus.ExceptionContinue"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/>.</param>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.ExceptionContinue"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreateExceptionContinueResult(System.Exception exception, string? reason = null)
            => new Result { Status = SubscriberStatus.ExceptionContinue, Exception = exception, Reason = reason ?? exception?.Message };

        /// <summary>
        /// Creates an <see cref="SubscriberStatus.UnhandledException"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="exception">The <see cref="System.Exception"/>.</param>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.UnhandledException"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreateUnhandledExceptionResult(System.Exception exception, string? reason = null)
            => new Result { Status = SubscriberStatus.UnhandledException, Exception = exception, Reason = reason ?? exception?.Message };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.PoisonSkipped"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="reason">The optional reason.</param>
        /// <param name="handling">The <see cref="ResultHandling"/>. Defaults to <see cref="ResultHandling.ContinueWithAudit"/>.</param>
        /// <returns>The <see cref="SubscriberStatus.PoisonSkipped"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreatePoisonSkippedResult(string? subject, string? action, string? reason = null, ResultHandling handling = ResultHandling.ContinueWithAudit)
            => new Result { Subject = subject, Action = action, Status = SubscriberStatus.PoisonSkipped, ResultHandling = handling, Reason = reason ?? "EventData was identified as Poison and was marked as SkipMessage; this event is skipped (i.e. not processed)." };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.PoisonMismatch"/> <see cref="Result"/>.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <param name="reason">The optional reason.</param>
        /// <param name="handling">The <see cref="ResultHandling"/>. Defaults to <see cref="ResultHandling.ContinueWithAudit"/>.</param>
        /// <returns>The <see cref="SubscriberStatus.PoisonMismatch"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreatePoisonMismatchResult(string? subject, string? action, string? reason = null, ResultHandling handling = ResultHandling.ContinueWithAudit)
            => new Result { Subject = subject, Action = action, Status = SubscriberStatus.PoisonMismatch, ResultHandling = handling, Reason = reason ?? "EventData does not match the expected poison message and it is uncertain whether it has been successfully processed." };

        /// <summary>
        /// Creates a <see cref="SubscriberStatus.PoisonMaxAttempts"/> <see cref="Result"/> from an existing <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The existing <see cref="Result"/>.</param>
        /// <param name="attempts">The number of attempts.</param>
        /// <param name="reason">The optional reason.</param>
        /// <returns>The <see cref="SubscriberStatus.PoisonMaxAttempts"/> <see cref="Result"/>.</returns>
        /// <remarks>This should only be used internally when implementing an <see cref="EventSubscriberHost"/> or <see cref="IAuditWriter"/> for example; otherwise, an unintended side-effect could occur.</remarks>
        public static Result CreatePoisonMaxAttemptsResult(Result result, int attempts, string? reason = null)
            => new Result
            {
                Subject = result.Subject,
                Action = result.Action,
                Status = SubscriberStatus.PoisonMaxAttempts,
                ResultHandling = ResultHandling.ContinueWithAudit,
                Subscriber = result.Subscriber,
                Reason = reason ?? $"EventData was identified as Poison and has been configured to automatically SkipMessage after {attempts} attempts; this event is skipped (i.e. not processed)."
            };


        /// <summary>
        /// Checks the <see cref="Result"/> and handles accordingly.
        /// </summary>
        private async Task<Result> CheckResultAsync(IEventSubscriberData data, Result result, IEventSubscriber? subscriber = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            result.Subject = data.Metadata.Subject;
            result.Action = data.Metadata.Action;
            result.Subscriber = subscriber;

            switch (result.Status)
            {
                case SubscriberStatus.Success: 
                    break;

                case SubscriberStatus.InvalidEventData:
                    await HandleTheHandlingAsync(data, result, result.ResultHandling ?? subscriber?.InvalidEventDataHandling ?? Args.InvalidEventDataHandling).ConfigureAwait(false);
                    break;

                case SubscriberStatus.NotSubscribed:
                    await HandleTheHandlingAsync(data, result, result.ResultHandling ?? Args.NotSubscribedHandling).ConfigureAwait(false);
                    break;

                case SubscriberStatus.DataNotFound:
                    await HandleTheHandlingAsync(data, result, result.ResultHandling ?? subscriber?.DataNotFoundHandling ?? Args.DataNotFoundHandling).ConfigureAwait(false);
                    break;

                case SubscriberStatus.InvalidData:
                    await HandleTheHandlingAsync(data, result, result.ResultHandling ?? subscriber?.InvalidDataHandling ?? Args.InvalidDataHandling).ConfigureAwait(false);
                    break;

                case SubscriberStatus.ExceptionContinue:
                    await HandleTheHandlingAsync(data, result, ResultHandling.ContinueWithAudit).ConfigureAwait(false);
                    break;

                case SubscriberStatus.UnhandledException:
                    await HandleTheHandlingAsync(data, result,
                        subscriber == null || subscriber.UnhandledExceptionHandling == UnhandledExceptionHandling.ThrowException ? ResultHandling.ThrowException : ResultHandling.ContinueWithAudit).ConfigureAwait(false);

                    break;
            }

            return result;
        }

        /// <summary>
        /// Handle the result as required.
        /// </summary>
        private async Task HandleTheHandlingAsync(IEventSubscriberData data, Result result, ResultHandling handling)
        {
            switch (result.ResultHandling ??= handling)
            {
                case ResultHandling.ContinueWithLogging:
                    Logger.LogWarning(result.ToString());
                    break;

                case ResultHandling.ContinueWithAudit:
                    await AuditWriter!.WriteAuditAsync(data, result).ConfigureAwait(false);
                    break;

                case ResultHandling.ThrowException:
                    await AuditWriter!.WriteAuditAsync(data, result).ConfigureAwait(false);
                    throw new EventSubscriberUnhandledException(result);

                case ResultHandling.ContinueSilent:
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates the <see cref="ExecutionContext"/> for processing the <paramref name="event"/> setting the username based on the <see cref="IEventSubscriber"/> <see cref="IEventSubscriber.RunAsUser"/>.
        /// </summary>
        /// <param name="executionContext">The <see cref="ExecutionContext"/> to update.</param>
        /// <param name="subscriber">The <see cref="IEventSubscriber"/> that will process the message.</param>
        /// <param name="event">The <see cref="EventData"/>.</param>
        /// <remarks></remarks>
        private static void UpdateExecutionContext(ExecutionContext executionContext, IEventSubscriber subscriber, EventData @event)
        {
            if (executionContext == null)
                throw new ArgumentNullException(nameof(executionContext));

            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            executionContext.Username = subscriber.RunAsUser == RunAsUser.Originating ? @event.Username ?? SystemUsername : SystemUsername;
            executionContext.UserId = @event.UserId;
            executionContext.TenantId = @event.TenantId;
        }
    }
}