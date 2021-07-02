// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Test.NUnit.Tests;
using Beef.Validation;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Manages the testing of a validator with integrated mocking of services as required.
    /// </summary>
    /// <remarks><see cref="TestSetUp.SetDefaultLocalReferenceData"/> is required to enable the reference data to function correctly. Where a <see cref="ExecutionContext.ServiceProvider"/> is 
    /// currently available this will be used versus creating new, minimizing the new to maintain (duplicate) the <see cref="ConfigureServices(Action{IServiceCollection})"/> logic.</remarks>
    [System.Diagnostics.DebuggerStepThrough]
    public sealed class ValidationTester : TesterBase
    {
        private readonly string? _username;
        private readonly object? _args;
        private OperationType _operationType = Beef.OperationType.Unspecified;
        private ErrorType? _expectedErrorType;
        private string? _expectedErrorMessage;
        private MessageItemCollection? _expectedMessages;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ValidationTester() => TextProvider.SetTextProvider(new DefaultTextProvider());

        /// <summary>
        /// Create a new <see cref="ValidationTester"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>A <see cref="ValidationTester"/> instance.</returns>
        public static ValidationTester Test(string? username = null, object? args = null) => new ValidationTester(username, args);

        /// <summary>
        /// Create a new <see cref="ValidationTester"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="TestSetUp.ConvertUsername(object?)"/>).
        /// </summary>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <returns>A <see cref="ValidationTester"/> instance.</returns>
        public static ValidationTester Test(object? userIdentifier, object? args = null) => new ValidationTester(TestSetUp.ConvertUsername(userIdentifier), args);

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationTester"/> class.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        private ValidationTester(string? username = null, object? args = null) : base(configureLocalRefData: true, inheritServiceCollection: true)
        {
            _username = username;
            _args = args;
        }

        /// <summary>
        /// Provides the opportunity to configure the services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        public ValidationTester ConfigureServices(Action<IServiceCollection> serviceCollection)
        {
            ConfigureLocalServices(Check.NotNull(serviceCollection, nameof(serviceCollection)));
            return this;
        }

        /// <summary>
        /// Provides the opportunity to configure the services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> function.</param>
        public ValidationTester ConfigureServices(Func<IServiceCollection, IServiceCollection> serviceCollection)
        {
            ConfigureLocalServices(sc => Check.NotNull(serviceCollection, nameof(serviceCollection))(sc));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a singleton service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public ValidationTester AddSingletonService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceSingleton(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a singleton service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public ValidationTester AddSingletonService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceSingleton(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a scoped service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public ValidationTester AddScopedService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceScoped(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a scoped service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public ValidationTester AddScopedService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceScoped(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a transient service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public ValidationTester AddTransientService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceTransient(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a transient service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public ValidationTester AddTransientService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceTransient(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Sets the <see cref="ExecutionContext.OperationType"/> to the specified <paramref name="operationType"/>.
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/>.</param>
        /// <returns>The <see cref="ValidationTester"/> instance to support fluent/chaining usage.</returns>
        public ValidationTester OperationType(OperationType operationType)
        {
            _operationType = operationType;
            return this;
        }

        /// <summary>
        /// Expect the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The expected <see cref="ErrorType"/>.</param>
        /// <param name="errorMessage">The expected error message text; where not specified the error message text will not be checked.</param>
        /// <returns>The <see cref="ValidationTester"/> instance to support fluent/chaining usage.</returns>
        public ValidationTester ExpectErrorType(ErrorType errorType, string? errorMessage = null)
        {
            _expectedErrorType = errorType;
            _expectedErrorMessage = errorMessage;
            return this;
        }

        /// <summary>
        /// Expect the specified <see cref="MessageType.Error"/> messages.
        /// </summary>
        /// <param name="messages">An array of expected <see cref="MessageType.Error"/> message texts.</param>
        /// <returns>The <see cref="ValidationTester"/> instance to support fluent/chaining usage.</returns>
        public ValidationTester ExpectMessages(params string[] messages)
        {
            var mic = new MessageItemCollection();
            foreach (var text in messages)
            {
                mic.AddError(text);
            }

            ExpectMessages(mic);
            return this;
        }

        /// <summary>
        /// Expect the specified messages.
        /// </summary>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        /// <returns>The <see cref="ValidationTester"/> instance to support fluent/chaining usage.</returns>
        public ValidationTester ExpectMessages(MessageItemCollection messages)
        {
            Check.NotNull(messages, nameof(messages));
            if (_expectedMessages == null)
                _expectedMessages = new MessageItemCollection();

            _expectedMessages.AddRange(messages);
            return this;
        }

        /// <summary>
        /// Indicates whether any errors are expected.
        /// </summary>
        private bool IsExpectingError => _expectedErrorType.HasValue || _expectedErrorMessage != null || _expectedMessages != null;

        /// <summary>
        /// Creates (instantiates) the <typeparamref name="TValidator"/> using Dependency Injection (DI) and validates the <typeparamref name="TEntity"/> <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The validator <see cref="Type"/> to instantiate.</typeparam>
        /// <param name="value">The <typeparamref name="TEntity"/> value.</param>
        /// <returns>The resulting <see cref="IValidationContext"/> where applicable; otherwise, <c>null</c>.</returns>
        public IValidationContext? CreateAndRun<TValidator, TEntity>(TEntity? value) where TEntity : class where TValidator : class, IValidator<TEntity>
            => Task.Run(() => CreateAndRunAsync<TValidator, TEntity>(value)).GetAwaiter().GetResult();

        /// <summary>
        /// Creates (instantiates) the <typeparamref name="TValidator"/> using Dependency Injection (DI) and validates asynchronously the <typeparamref name="TEntity"/> <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TValidator">The validator <see cref="Type"/> to instantiate.</typeparam>
        /// <param name="value">The <typeparamref name="TEntity"/> value.</param>
        /// <returns>The resulting <see cref="IValidationContext"/> where applicable; otherwise, <c>null</c>.</returns>
        public async Task<IValidationContext?> CreateAndRunAsync<TValidator, TEntity>(TEntity? value) where TEntity : class where TValidator : class, IValidator<TEntity>
        {
            PrepareExecutionContext(_username, _args);
            ExecutionContext.Current.OperationType = _operationType;

            var v = ExecutionContext.GetService<TValidator>(throwExceptionOnNull: true)!;
            return await RunInternalAsync(async () => await v.ValidateAsync(value!, null!).ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the validation <paramref name="func"/> checking the <see cref="IValidationContext"/> response against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The resulting <see cref="IValidationContext"/> where applicable; otherwise, <c>null</c>.</returns>
        public IValidationContext? Run(Func<Task<IValidationContext>> func) => Task.Run(() => RunAsync(func)).GetAwaiter().GetResult();

        /// <summary>
        /// Runs the validation <paramref name="func"/> asynchronously checking the <see cref="IValidationContext"/> response against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The resulting <see cref="IValidationContext"/> where applicable; otherwise, <c>null</c>.</returns>
        public async Task<IValidationContext?> RunAsync(Func<Task<IValidationContext>> func)
        {
            PrepareExecutionContext(_username, _args);
            ExecutionContext.Current.OperationType = _operationType;
            return await RunInternalAsync(func).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the validation <paramref name="func"/> checking the <see cref="IValidationContext"/> response against the expected outcomes (internal logic)
        /// </summary>
        private async Task<IValidationContext?> RunInternalAsync(Func<Task<IValidationContext>> func)
        {
            if (IsExpectingError && !_expectedErrorType.HasValue)
                _expectedErrorType = ErrorType.ValidationError;

            try
            {
                var vc = await Check.NotNull(func, nameof(func))().ConfigureAwait(false);
                if (vc == null)
                    Assert.Fail("The validation function returned a null IValidationContext result.");

                if (vc!.HasErrors)
                    LogStatus(ErrorType.ValidationError, new LText("Beef.ValidationException"), vc.Messages);
                else
                    LogStatus(null, null, null);

                if (_expectedErrorType.HasValue)
                {
                    if (!vc!.HasErrors && _expectedErrorType != ErrorType.ValidationError)
                        Assert.Fail($"Expected ErrorType was '{_expectedErrorType}'; actual was '{ErrorType.ValidationError}'.");

                    if (_expectedErrorMessage != null)
                    {
                        var message = new LText("Beef.ValidationException");
                        if (_expectedErrorMessage != message)
                            Assert.Fail($"Expected ErrorMessage was '{_expectedErrorMessage}'; actual was '{message}'.");
                    }
                }

                if (_expectedMessages != null)
                    CompareExpectedVsActualMessages(_expectedMessages, vc!.Messages);

                if (!IsExpectingError && vc.HasErrors)
                    Assert.Fail("Expected success yet one or more errors.");

                return vc;
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                bool errorTypeOK = false;
                if (ex is IBusinessException ibex)
                {
                    LogStatus(ibex.ErrorType, ex.Message, ex is ValidationException vex ? vex.Messages : null);

                    if (_expectedErrorType.HasValue && _expectedErrorType != ibex.ErrorType)
                        Assert.Fail($"Expected ErrorType was '{_expectedErrorType}'; actual was '{ibex.ErrorType}'.");

                    if (_expectedErrorMessage != null && _expectedErrorMessage != ex.Message)
                        Assert.Fail($"Expected ErrorMessage was '{_expectedErrorMessage}'; actual was '{ex.Message}'.");

                    errorTypeOK = true;
                    if (_expectedMessages != null)
                        CompareExpectedVsActualMessages(_expectedMessages, ex is ValidationException vexx ? vexx.Messages : null);
                    else if (!_expectedErrorType.HasValue)
                        Assert.Fail($"Expected success; however, {ex.GetType().Name} was thrown.");
                }

                if (IsExpectingError && errorTypeOK)
                    return null;

                throw;
            }
        }

        /// <summary>
        /// Runs the <paramref name="func"/> checking the <b>thrown</b> <see cref="ValidationException"/> against the expected outcomes.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        public async Task RunAsync(Func<Task> func)
        {
            PrepareExecutionContext(_username, _args);
            ExecutionContext.Current.OperationType = _operationType;

            if (IsExpectingError && !_expectedErrorType.HasValue)
                _expectedErrorType = ErrorType.ValidationError;

            try
            {
                await Check.NotNull(func, nameof(func))().ConfigureAwait(false);
                if (IsExpectingError)
                    Assert.Fail("The function completed successfully; expected a ValidationException.");
            }
            catch (AssertionException) { throw; }
            catch (Exception ex)
            {
                bool errorTypeOK = false;
                if (ex is IBusinessException ibex)
                {
                    LogStatus(ibex.ErrorType, ex.Message, ex is ValidationException vex ? vex.Messages : null);

                    if (_expectedErrorType.HasValue && _expectedErrorType != ibex.ErrorType)
                        Assert.Fail($"Expected ErrorType was '{_expectedErrorType}'; actual was '{ibex.ErrorType}'.");

                    if (_expectedErrorMessage != null && _expectedErrorMessage != ex.Message)
                        Assert.Fail($"Expected ErrorMessage was '{_expectedErrorMessage}'; actual was '{ex.Message}'.");

                    errorTypeOK = true;
                    if (_expectedMessages != null)
                        CompareExpectedVsActualMessages(_expectedMessages, ex is ValidationException vexx ? vexx.Messages : null);
                }

                if (IsExpectingError && errorTypeOK)
                    return;

                throw;
            }
        }

        /// <summary>
        /// Log the status.
        /// </summary>
        private static void LogStatus(ErrorType? errorType, string? errorMessage, MessageItemCollection? mic)
        {
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine("VALIDATION TESTER...");
            TestContext.Out.WriteLine("");
            TestContext.Out.WriteLine($"ErrorType: {(errorType == null ? "none" : errorType.ToString())}");
            TestContext.Out.WriteLine($"ErrorMessage: {(errorMessage ?? "none")}");
            TestContext.Out.WriteLine($"Messages: {(mic == null || mic.Count == 0 ? "none" : "")}");

            if (mic != null && mic.Count > 0)
            {
                foreach (var m in mic)
                {
                    TestContext.Out.WriteLine($" {m.Type}: {m.Text} {(m.Property == null ? "" : "(" + m.Property + ")")}");
                }

                TestContext.Out.WriteLine("");
            }
        }
    }
}