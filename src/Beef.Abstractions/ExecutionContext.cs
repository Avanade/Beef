// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beef
{
    /// <summary>
    /// Provides the core capabilities for the <see cref="ExecutionContext"/>.
    /// </summary>
    public interface IExecutionContext : IETag
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> that provides access to the configured service container.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; }

        /// <summary>
        /// Gets the operation type (defaults to <see cref="OperationType.Unspecified"/>).
        /// </summary>
        OperationType OperationType { get; }

        /// <summary>
        /// Gets the unique user identifier.
        /// </summary>
        string? UserId { get; }

        /// <summary>
        /// Gets the username for the request.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the tenant identifier.
        /// </summary>
        Guid? TenantId { get; }

        /// <summary>
        /// Gets the parition key. This value is immutable.
        /// </summary>
        public string? PartitionKey { get; }

        /// <summary>
        /// Gets the correlation identifier (a unique identifier assigned to the request).
        /// </summary>
        public string? CorrelationId { get; }

        /// <summary>
        /// Gets the session correlation identifier (a unique identifier assigned to the session).
        /// </summary>
        public string? SessionCorrelationId { get; }

        /// <summary>
        /// Gets the request timestamp (to enable consistent execution-related timestamping). This value is immutable.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Determines whether the user has the required <paramref name="permission"/>.
        /// </summary>
        /// <param name="permission">The permission to validate.</param>
        /// <param name="throwAuthorizationException">Indicates whether to throw an <see cref="AuthorizationException"/> where the user is not authorised.</param>
        /// <returns><c>true</c> where the user is authorized; otherwise, <c>false</c>.</returns>
        /// <remarks>This method is intended to be overridden; this implementation always returns <c>false</c>.</remarks>
        bool IsAuthorized(string permission, bool throwAuthorizationException = false);

        /// <summary>
        /// Determines whether the user has the required permission (as a combination of an <paramref name="entity"/> and <paramref name="action"/>).
        /// </summary>
        /// <param name="entity">The entity name.</param>
        /// <param name="action">The action name.</param>
        /// <param name="throwAuthorizationException">Indicates whether to throw an <see cref="AuthorizationException"/> where the user is not authorised.</param>
        /// <returns><c>true</c> where the user is authorized; otherwise, <c>false</c>.</returns>
        /// <remarks>This method is intended to be overridden; this implementation always returns <c>false</c>.</remarks>
        bool IsAuthorized(string entity, string action, bool throwAuthorizationException = false);

        /// <summary>
        /// Determines whether the user is in the specified role (see <see cref="ExecutionContext.SetRoles"/> and <see cref="ExecutionContext.GetRoles"/>).
        /// </summary>
        /// <param name="role">The role name.</param>
        /// <param name="throwAuthorizationException">Indicates whether to throw an <see cref="AuthorizationException"/> where the user is not in the specified role.</param>
        /// <returns><c>true</c> where the user is in the specified role; otherwise, <c>false</c>.</returns>
        bool IsInRole(string role, bool throwAuthorizationException = false);
    }

    /// <summary>
    /// Represents a thread-bound (request) execution context using <see cref="AsyncLocal{ExecutionContext}"/>.
    /// </summary>
    /// <remarks>Used to house/pass context parameters and capabilities that are outside of the general operation arguments.</remarks>
    public class ExecutionContext : IExecutionContext
    {
        private static readonly AsyncLocal<ExecutionContext?> _asyncLocal = new();
        private static readonly Func<ExecutionContext?> _get = () => _asyncLocal.Value;
        private static readonly Action<ExecutionContext?> _set = (ec) => _asyncLocal.Value = ec;

        private IServiceProvider? _serviceProvider;
        private string? _userId;
        private string? _username;
        private Guid? _tenantId;
        private string? _partitionKey;
        private DateTime _timestamp = Cleaner.Clean(DateTime.UtcNow);
        private bool _timestampChanged;
        private PagingArgs? _pagingArgs;
        private KeyOnlyDictionary<string>? _roles;
        private readonly Lazy<MessageItemCollection> _messages = new(true);
        private readonly Lazy<Dictionary<string, object>> _properties = new(true);

        /// <summary>
        /// Gets the standard message for when changing an immutable value.
        /// </summary>
        public const string ImmutableText = "Value is immutable; cannot be changed once already set to a value.";

        /// <summary>
        /// Gets the current <see cref="ExecutionContext"/> for the executing thread graph (see <see cref="AsyncLocal{T}"/>).
        /// </summary>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> will be thrown where <see cref="HasCurrent"/> is <c>false</c>.</exception>
        public static ExecutionContext Current => _get() ?? throw new InvalidOperationException("There is currently no ExecutionContext.Current instance; this must be set (SetCurrent) prior to access. Use ExecutionContext.HasCurrent to verify value and avoid this exception.");

        /// <summary>
        /// Indicates whether the <see cref="ExecutionContext"/> <see cref="Current"/> has a value.
        /// </summary>
        public static bool HasCurrent => _get() != null;

        /// <summary>
        /// Sets the <see cref="Current"/> instance (only allowed where <see cref="HasCurrent"/> is <c>false</c>).
        /// </summary>
        /// <param name="executionContext">The <see cref="ExecutionContext"/> instance.</param>
        /// <exception cref="InvalidOperationException">An <see cref="InvalidOperationException"/> will be thrown where <see cref="HasCurrent"/> is <c>true</c>.</exception>
        public static void SetCurrent(ExecutionContext executionContext)
        {
            Check.NotNull(executionContext, nameof(executionContext));
            if (HasCurrent)
                throw new InvalidOperationException("The SetCurrent method can only be used where there is no Current instance.");

            _set(executionContext);
        }

        /// <summary>
        /// Resets (clears) the <see cref="Current"/> <see cref="ExecutionContext"/>.
        /// </summary>
        public static void Reset() => _set(null);

        /// <summary>
        /// Gets the service of <see cref="Type"/> <typeparamref name="T"/> from the <see cref="Current"/> <see cref="ServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The service <see cref="Type"/>.</typeparam>
        /// <param name="throwExceptionOnNull">Indicates whether to throw an <see cref="InvalidOperationException"/> where the underlying <see cref="IServiceProvider.GetService(Type)"/> returns <c>null</c>.</param>
        /// <returns>The corresponding instance.</returns>
        public static T? GetService<T>(bool throwExceptionOnNull = true)
        {
            if (HasCurrent && Current.ServiceProvider != null)
                return Current.ServiceProvider.GetService<T>() ??
                    (throwExceptionOnNull ? throw new InvalidOperationException($"Attempted to get service '{typeof(T).FullName}' but null was returned; this would indicate that the service has not been configured correctly.") : default(T)!);

            if (throwExceptionOnNull)
                throw new InvalidOperationException($"Attempted to get service '{typeof(T).FullName}' but there is either no ExecutionContext.Current or the ExecutionContext.ServiceProvider has not been configured.");

            return default!;
        }

        /// <summary>
        /// Gets the service of <see cref="Type"/> <paramref name="type"/> from the <see cref="Current"/> <see cref="ServiceProvider"/>.
        /// </summary>
        /// <param name="type">The service <see cref="Type"/>.</param>
        /// <param name="throwExceptionOnNull">Indicates whether to throw an <see cref="InvalidOperationException"/> where the underlying <see cref="IServiceProvider.GetService(Type)"/> returns <c>null</c>.</param>
        /// <returns>The corresponding instance.</returns>
        public static object? GetService(Type type, bool throwExceptionOnNull = true)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (HasCurrent && Current.ServiceProvider != null)
                return Current.ServiceProvider.GetService(type) ??
                    (throwExceptionOnNull ? throw new InvalidOperationException($"Attempted to get service '{type.FullName}' but null was returned; this would indicate that the service has not been configured correctly.") : (object?)null);

            throw new InvalidOperationException($"Attempted to get service '{type.FullName}' but there is either no ExecutionContext.Current or the ExecutionContext.ServiceProvider has not been configured.");
        }

        /// <summary>
        /// Gets the current <see cref="ISystemTime"/> using <see cref="GetService{ISystemTime}(bool)"/>.
        /// </summary>
        public static ISystemTime SystemTime => GetService<ISystemTime>(true)!;

        /// <summary>
        /// Gets the username from the <see cref="Environment"/> settings.
        /// </summary>
        /// <returns>The fully qualified username.</returns>
        public static string EnvironmentUsername => Environment.UserDomainName == null ? Environment.UserName : Environment.UserDomainName + "\\" + Environment.UserName;

        /// <summary>
        /// Gets or sets the operation type (defaults to <see cref="OperationType.Unspecified"/>).
        /// </summary>
        public OperationType OperationType { get; set; } = OperationType.Unspecified;

        /// <summary>
        /// Gets or sets the <see cref="IServiceProvider"/> that provides access to the configured service container. This value is immutable. <b>Note: </b> this is set internally by <i>Beef</i>, do not set directly.
        /// </summary>
        public IServiceProvider? ServiceProvider
        {
            get => _serviceProvider;

            set
            {
                if (_serviceProvider != null && value != _serviceProvider)
                    throw new ArgumentException(ImmutableText);

                _serviceProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique user identifier. This value is immutable.
        /// </summary>
        public string? UserId
        {
            get => _userId;

            set
            {
                if (_userId != null && value != _userId)
                    throw new ArgumentException(ImmutableText);

                _userId = value;
            }
        }

        /// <summary>
        /// Gets or sets the username for the request. This value is immutable.
        /// </summary>
        /// <remarks>Where not overridden the <i>get</i> will default to <see cref="EnvironmentUsername"/> to ensure a value is always returned.</remarks>
        public string Username
        {
            get { return _username ?? EnvironmentUsername; }

            set
            {
                if (_username != null && value != _username)
                    throw new ArgumentException(ImmutableText);

                _username = Check.NotEmpty(value, nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the tenant identifier. This value is immutable.
        /// </summary>
        public Guid? TenantId
        {
            get { return _tenantId; }

            set
            {
                if (_tenantId != null && value != _tenantId)
                    throw new ArgumentException(ImmutableText);

                _tenantId = value;
            }
        }

        /// <summary>
        /// Gets or sets the parition key. This value is immutable.
        /// </summary>
        public string? PartitionKey
        {
            get { return _partitionKey; }

            set
            {
                if (_partitionKey != null && value != _partitionKey)
                    throw new ArgumentException(ImmutableText);

                _partitionKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the correlation identifier (a unique identifier assigned to the request).
        /// </summary>
        /// <remarks>This may be set automatically by <i>Beef</i>; for example from an HTTP header using <see cref="WebApi.WebApiConsts.CorrelationIdHeaderName"/>.</remarks>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the session correlation identifier (a unique identifier assigned to the session).
        /// </summary>
        public string? SessionCorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the request timestamp (to enable consistent execution-related timestamping). This value is immutable.
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timestamp; }

            set
            {
                if (_timestampChanged && value != _timestamp)
                    throw new ArgumentException(ImmutableText);

                _timestamp = Cleaner.Clean(value);
                _timestampChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs"/>. This value is immutable.
        /// </summary>
        public PagingArgs? PagingArgs
        {
            get { return _pagingArgs; }

            set
            {
                if (_pagingArgs != null && value != _pagingArgs)
                    throw new ArgumentException(ImmutableText);

                _pagingArgs = value;
            }
        }

        /// <summary>
        /// Gets or sets the <b>result</b> entity tag (where value does not support <see cref="IETag"/>).
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/> to be passed back to the originating consumer.
        /// </summary>
        public MessageItemCollection Messages { get => _messages.Value; }

        /// <summary>
        /// Gets the properties <see cref="Dictionary{TKey, TValue}"/> for passing/storing additional data.
        /// </summary>
        public Dictionary<string, object> Properties { get => _properties.Value; }

        /// <summary>
        /// Indicates whether <see cref="RefData.ReferenceDataBase"/> <see cref="RefData.ReferenceDataBase.Text"/> serialization is enabled. The
        /// <see cref="Entities.EntityBasicBase.GetRefDataText(Func{RefData.ReferenceDataBase})"/> checks this value to determine whether the <i>text</i> should be retrieved.
        /// </summary>
        public bool IsRefDataTextSerializationEnabled { get; set; }

        #region Security

        /// <summary>
        /// Gets the list of roles for the <see cref="Username"/> (as previously <see cref="SetRoles(IEnumerable{string})">set</see>).
        /// </summary>
        public IEnumerable<string> GetRoles()
        {
            return _roles == null ? Array.Empty<string>() : _roles.Select(x => x.Key).ToArray();
        }

        /// <summary>
        /// Sets the roles the current user is in (the roles must be unique). This value is immutable.
        /// </summary>
        /// <param name="roles">The <see cref="IEnumerable{String}"/> of roles the user is in.</param>
        public virtual void SetRoles(IEnumerable<string> roles)
        {
            if (_roles != null)
                throw new ArgumentException(ImmutableText);

            _roles = new KeyOnlyDictionary<string>();
            _roles.AddRange(roles);
        }

        /// <summary>
        /// Determines whether the user has the required <paramref name="permission"/>.
        /// </summary>
        /// <param name="permission">The permission to validate.</param>
        /// <param name="throwAuthorizationException">Indicates whether to throw an <see cref="AuthorizationException"/> where the user is not authorised.</param>
        /// <returns><c>true</c> where the user is authorized; otherwise, <c>false</c>.</returns>
        /// <remarks>This method is intended to be overridden; this implementation always returns <c>false</c>.</remarks>
        public virtual bool IsAuthorized(string permission, bool throwAuthorizationException = false)
        {
            if (string.IsNullOrEmpty(permission))
                throw new ArgumentNullException(nameof(permission));

            if (throwAuthorizationException)
                throw new AuthorizationException();

            return false;
        }

        /// <summary>
        /// Determines whether the user has the required permission (as a combination of an <paramref name="entity"/> and <paramref name="action"/>).
        /// </summary>
        /// <param name="entity">The entity name.</param>
        /// <param name="action">The action name.</param>
        /// <param name="throwAuthorizationException">Indicates whether to throw an <see cref="AuthorizationException"/> where the user is not authorised.</param>
        /// <returns><c>true</c> where the user is authorized; otherwise, <c>false</c>.</returns>
        /// <remarks>This method is intended to be overridden; this implementation always returns <c>false</c>.</remarks>
        public virtual bool IsAuthorized(string entity, string action, bool throwAuthorizationException = false)
        {
            if (string.IsNullOrEmpty(entity))
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrEmpty(action))
                throw new ArgumentNullException(nameof(action));

            if (throwAuthorizationException)
                throw new AuthorizationException();

            return false;
        }

        /// <summary>
        /// Determines whether the user is in the specified role (see <see cref="SetRoles"/> and <see cref="GetRoles"/>).
        /// </summary>
        /// <param name="role">The role name.</param>
        /// <param name="throwAuthorizationException">Indicates whether to throw an <see cref="AuthorizationException"/> where the user is not in the specified role.</param>
        /// <returns><c>true</c> where the user is in the specified role; otherwise, <c>false</c>.</returns>
        public virtual bool IsInRole(string role, bool throwAuthorizationException = false)
        {
            var isInRole = (_roles != null) && _roles.ContainsKey(role);
            if (!isInRole && throwAuthorizationException)
                throw new AuthorizationException();

            return isInRole;
        }

        #endregion

        #region NewScope

        /// <summary>
        /// Executes the asynchronous <paramref name="func"/> suppressing the flow of the <see cref="ExecutionContext"/> via the underlying <see cref="AsyncLocal{T}"/> <see cref="Current"/> property orchestration;
        /// whilst also <see cref="ServiceProviderServiceExtensions.CreateScope(IServiceProvider)">creating a new scope</see> for the <see cref="ServiceProvider"/>. Note that the overarching invocation will occur synchronously.
        /// <para><b>Warning:</b> this is an advanced feature and should only be used where this specific capability is required.</para>
        /// </summary>
        /// <param name="func">The asynchronous function to invoke within the new scope context.</param>
        public static void NewScope(Func<IServiceProvider, Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (!HasCurrent || Current.ServiceProvider == null)
                throw new InvalidOperationException("Can only be invoked where the ExecutionContext.Current has a value and the ServiceProvider property is not null.");

            var fec = Current.NewScopeCopy();

            using (var scope = Current.ServiceProvider.CreateScope())
            using (System.Threading.ExecutionContext.SuppressFlow())
            {
                Task.Run(async () =>
                {
                    fec.ServiceProvider = scope.ServiceProvider;
                    SetCurrent(fec);
                    await func(scope.ServiceProvider).ConfigureAwait(false);
                }).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Executes the asynchronous <paramref name="func"/> suppressing the flow of the <see cref="ExecutionContext"/> via the underlying <see cref="AsyncLocal{T}"/> <see cref="Current"/> property orchestration;
        /// whilst also <see cref="ServiceProviderServiceExtensions.CreateScope(IServiceProvider)">creating a new scope</see> for the <see cref="ServiceProvider"/>. Note that the overarching invocation will occur synchronously.
        /// <para><b>Warning:</b> this is an advanced feature and should only be used where this specific capability is required.</para>
        /// </summary>
        /// <param name="func">The asynchronous function to invoke within the new scope context.</param>
        public static T NewScope<T>(Func<IServiceProvider, Task<T>> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (!HasCurrent || Current.ServiceProvider == null)
                throw new InvalidOperationException("Can only be invoked where the ExecutionContext.Current has a value and the ServiceProvider property is not null.");

            var fec = Current.NewScopeCopy();

            using (var scope = Current.ServiceProvider.CreateScope())
            using (System.Threading.ExecutionContext.SuppressFlow())
            {
                return Task.Run(async () =>
                {
                    fec.ServiceProvider = scope.ServiceProvider;
                    SetCurrent(fec);
                    return await func(scope.ServiceProvider).ConfigureAwait(false);
                }).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="ExecutionContext"/> with only those properties that should be shared; specifically ignoring the <see cref="ServiceProvider"/>.
        /// </summary>
        /// <returns>The new <see cref="ExecutionContext"/>.</returns>
        private ExecutionContext NewScopeCopy()
        {
            return new ExecutionContext
            {
                OperationType = OperationType,
                _userId = _userId,
                _username = _username,
                _tenantId = _tenantId,
                _partitionKey = _partitionKey,
                CorrelationId = CorrelationId,
                SessionCorrelationId = SessionCorrelationId,
                _timestamp = _timestamp,
                _pagingArgs = _pagingArgs,
                _roles = _roles,
            };
        }

        #endregion
    }
}