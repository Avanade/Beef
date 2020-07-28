// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Beef
{
    /// <summary>
    /// Represents a thread-bound (request) execution context using <see cref="AsyncLocal{ExecutionContext}"/>.
    /// </summary>
    /// <remarks>Used to house/pass context parameters and capabilities that are outside of the general operation arguments.</remarks>
    public class ExecutionContext : IETag
    {
        private static readonly AsyncLocal<ExecutionContext?> _asyncLocal = new AsyncLocal<ExecutionContext?>();
        private static readonly Func<ExecutionContext?> _get = () => _asyncLocal.Value;
        private static readonly Action<ExecutionContext?> _set = (ec) => _asyncLocal.Value = ec;

        private IServiceProvider? _serviceProvider;
        private string? _userId;
        private string? _username;
        private Guid? _tenantId;
        private string? _partitionKey;
        private string? _correlationId;
        private string? _sessionCorrelationId;
        private DateTime _timestamp = Cleaner.Clean(DateTime.Now);
        private bool _timestampChanged;
        private PagingArgs? _pagingArgs;
        private KeyOnlyDictionary<string>? _roles;
        private readonly Lazy<MessageItemCollection> _messages = new Lazy<MessageItemCollection>(true);
        private readonly Lazy<Dictionary<string, object>> _properties = new Lazy<Dictionary<string, object>>(true);

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
        public static void Reset()
        {
            var ec = _get();
            if (ec != null)
                ec.DataContextScope?.Dispose();

            _set(null);
        }

        /// <summary>
        /// Gets the service of <see cref="Type"/> <typeparamref name="T"/> from the <see cref="Current"/> <see cref="ServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T">The service <see cref="Type"/>.</typeparam>
        /// <param name="throwExceptionOnNull">Indicates whether to throw an <see cref="InvalidOperationException"/> where the underlying <see cref="IServiceProvider.GetService(Type)"/> returns <c>null</c>.</param>
        /// <returns>The corresponding instance.</returns>
        public static T GetService<T>(bool throwExceptionOnNull = true)
        {
            if (HasCurrent && Current.ServiceProvider != null)
                return Current.ServiceProvider.GetService<T>() ??
                    (throwExceptionOnNull ? throw new InvalidOperationException($"Attempted to get service '{typeof(T).Name}' but null was returned; this would indicate that the service has not been configured correctly.") : default(T)!);

            if (throwExceptionOnNull)
                throw new InvalidOperationException($"Attempted to get service '{typeof(T).Name}' but there is either no ExecutionContext.Current or the ExecutionContext.ServiceProvider has not been configured.");

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
                    (throwExceptionOnNull ? throw new InvalidOperationException($"Attempted to get service '{type.Name}' but null was returned; this would indicate that the service has not been configured correctly.") : (object?)null);

            throw new InvalidOperationException($"Attempted to get service '{type.Name}' but there is either no ExecutionContext.Current or the ExecutionContext.ServiceProvider has not been configured.");
        }

        /// <summary>
        /// Gets the username from the <see cref="Environment"/> settings.
        /// </summary>
        /// <returns>The fully qualified username.</returns>
        public static string EnvironmentUsername => Environment.UserDomainName == null ? Environment.UserName : Environment.UserDomainName + "\\" + Environment.UserName;

        /// <summary>
        /// Gets or sets the current <see cref="Beef.DataContextScope"/>.
        /// </summary>
        internal DataContextScope? DataContextScope { get; set; }

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
        /// Gets or sets the correlation identifier (a unique identifier assigned to the request). This value is immutable. <b>Note: </b> this is set internally by <i>Beef</i>, do not set directly.
        /// </summary>
        public string? CorrelationId
        {
            get { return _correlationId; }

            set
            {
                if (_correlationId != null && value != _correlationId)
                    throw new ArgumentException(ImmutableText);

                _correlationId = value;
            }
        }

        /// <summary>
        /// Gets or sets the session correlation identifier (a unique identifier assigned to the session). This value is immutable.
        /// </summary>
        public string? SessionCorrelationId
        {
            get { return _sessionCorrelationId; }

            set
            {
                if (_sessionCorrelationId != null && value != _sessionCorrelationId)
                    throw new ArgumentException(ImmutableText);

                _sessionCorrelationId = value;
            }
        }

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

        #region Flow

        /// <summary>
        /// Executes the <paramref name="action"/> suppressing the flow of the <see cref="ExecutionContext"/> across asynchronous threads. Any threads created that need the <see cref="ExecutionContext"/>
        /// should use the <see cref="ExecutionContextFlow.SetExecutionContext"/> which will copy the key properties of the originating <see cref="ExecutionContext"/> and set up a new 
        /// <see cref="Current"/> for usage. <i>Note:</i> any new threads <b>must</b> be created from within the <paramref name="action"/>. <para><b>Warning:</b> this is an advanced feature and should only be
        /// used where this specific capability is required.</para>
        /// </summary>
        /// <param name="action">The action to invoke with flow suppression.</param>
        public static void FlowSuppression(Action<ExecutionContextFlow> action)
        {
            using var afc = System.Threading.ExecutionContext.SuppressFlow();
            action?.Invoke(new ExecutionContextFlow(HasCurrent ? Current : null));
        }

        /// <summary>
        /// Creates a copy of the <see cref="ExecutionContext"/> with only those properties that can be easily shared across a new executing thread context.
        /// </summary>
        /// <returns>The new <see cref="ExecutionContext"/>.</returns>
        internal ExecutionContext FlowCopy()
        {
            return new ExecutionContext
            {
                OperationType = OperationType,
                _serviceProvider = _serviceProvider,
                _userId = _userId,
                _username = _username,
                _tenantId = _tenantId,
                _partitionKey = _partitionKey,
                _correlationId = _correlationId,
                _sessionCorrelationId = _sessionCorrelationId,
                _timestamp = _timestamp,
                _pagingArgs = _pagingArgs,
                _roles = _roles,
            };
        }

        #endregion
    }

    /// <summary>
    /// Enables access to the <see cref="Originating"/> <see cref="ExecutionContext"/> and the ability to <see cref="SetExecutionContext"/> using a copy within the new executing thread context.
    /// </summary>
    public sealed class ExecutionContextFlow
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContextFlow"/> class.
        /// </summary>
        /// <param name="originating">The originating <see cref="ExecutionContext"/>.</param>
        internal ExecutionContextFlow(ExecutionContext? originating) => Originating = originating;

        /// <summary>
        /// Gets the originating <see cref="ExecutionContext"/>.
        /// </summary>
        public ExecutionContext? Originating { get; }

        /// <summary>
        /// Sets the <see cref="ExecutionContext.Current"/> using a copy of the <see cref="Originating"/> <see cref="ExecutionContext"/>.
        /// </summary>
        public void SetExecutionContext()
        {
            lock (_lock)
            {
                ExecutionContext.Reset();
                if (Originating != null)
                    ExecutionContext.SetCurrent(Originating.FlowCopy());
            }
        }
    }
}