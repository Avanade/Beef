// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Beef.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Beef
{
    /// <summary>
    /// Represents a thread-bound (request) execution context.
    /// </summary>
    /// <remarks>Used to house/pass context parameters and capabilities that are outside of the general operation arguments. By default uses <see cref="AsyncLocal{T}"/>;
    /// although, this can be overridden (see <see cref="Register(Func{ExecutionContext}, Func{ExecutionContext}, Action{ExecutionContext})"/>).</remarks>
    public class ExecutionContext : IETag
    {
        private static readonly object _masterLock = new object();
        private static Func<ExecutionContext> _create = () => new ExecutionContext();
        private static readonly AsyncLocal<ExecutionContext?> _asyncLocal = new AsyncLocal<ExecutionContext?>();
        private static Func<ExecutionContext?> _get = () => _asyncLocal.Value;
        private static Action<ExecutionContext?> _set = (ec) => _asyncLocal.Value = ec;

        private Guid? _userId;
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
        private readonly Lazy<ConcurrentDictionary<Tuple<Type, UniqueKey>, object>> _caching = new Lazy<ConcurrentDictionary<Tuple<Type, UniqueKey>, object>>(true);

        /// <summary>
        /// Gets the standard message for when changing an immutable value.
        /// </summary>
        public const string ImmutableText = "Value is immutable; cannot be changed once already set to a value.";

        /// <summary>
        /// Gets or sets the current <see cref="ExecutionContext"/> for the executing thread graph (see <see cref="AsyncLocal{T}"/>).
        /// </summary>
        public static ExecutionContext Current
        {
            get
            {
                var ec = _get();
                if (ec != null)
                    return ec;

                lock (_masterLock)
                {
                    ec = _get();
                    if (ec == null)
                    {
                        ec = _create();
                        _set(ec);
                    }

                    return ec;
                }
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="ExecutionContext"/> has been <see cref="Register(Func{ExecutionContext})">registered</see>.
        /// </summary>
        public static bool HasBeenRegistered { get; private set; } = false;

        /// <summary>
        /// Indicates whether the <see cref="ExecutionContext"/> <see cref="Current"/> has a value.
        /// </summary>
        public static bool HasCurrent
        {
            get { return _get() != null; }
        }

        /// <summary>
        /// Sets the <see cref="Current"/> instance (only allowed where <see cref="HasCurrent"/> is <c>false</c>).
        /// </summary>
        /// <param name="executionContext">The <see cref="ExecutionContext"/> instance.</param>
        public static void SetCurrent(ExecutionContext executionContext)
        {
            Check.NotNull(executionContext, nameof(executionContext));
            if (HasCurrent)
                throw new InvalidOperationException("The SetCurrent method can only be used where there is no Current instance.");

            _set(executionContext);
        }

        /// <summary>
        /// Registers the <see cref="ExecutionContext"/> <paramref name="create"/> function that is invoked when the <see cref="Current"/> instance is requested for the first time
        /// (per thread/request context (see <see cref="AsyncLocal{T}"/>)).
        /// </summary>
        /// <param name="create">The <see cref="ExecutionContext"/> creation function.</param>
        public static void Register(Func<ExecutionContext> create)
        {
            lock (_masterLock)
            {
                if (HasBeenRegistered)
                    throw new InvalidOperationException("The Register method can only be invoked once.");

                _create = create ?? throw new ArgumentNullException(nameof(create));
                HasBeenRegistered = true;
            }
        }

        /// <summary>
        /// Registers the <see cref="ExecutionContext"/> <paramref name="create"/> function that is invoked when the <see cref="Current"/> instance is requested for the first time
        /// (where the per thread/request-style context is managed externally).
        /// </summary>
        /// <param name="create">The <see cref="ExecutionContext"/> creation function.</param>
        /// <param name="get">The <see cref="ExecutionContext"/> get function.</param>
        /// <param name="set">The <see cref="ExecutionContext"/> set function.</param>
        public static void Register(Func<ExecutionContext> create, Func<ExecutionContext?> get, Action<ExecutionContext?> set)
        {
            lock (_masterLock)
            {
                if (_create != null)
                    throw new InvalidOperationException("The Register method can only be invoked once.");

                _create = create ?? throw new ArgumentNullException(nameof(create));
                _get = get ?? throw new ArgumentNullException(nameof(get));
                _set = set ?? throw new ArgumentNullException(nameof(set));
                HasBeenRegistered = true;
            }
        }

        /// <summary>
        /// Resets (renews) the <see cref="Current"/> <see cref="ExecutionContext"/>.
        /// </summary>
        /// <param name="renew">Indicates whether the <see cref="Current"/> is immediately renewed (defaults to <c>true</c>).</param>
        public static void Reset(bool renew = true)
        {
            var ec = _get();
            if (ec != null)
                ec.DataContextScope?.Dispose();

            _set((renew) ? _create() : null);
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
        /// Gets or sets the current <see cref="Beef.Diagnostics.Logger"/>.
        /// </summary>
        internal Logger? Logger { get; set; }

        /// <summary>
        /// Registers the <see cref="ExecutionContext"/> <see cref="Logger"/> instance (accessible via <see cref="Logger"/> <see cref="Logger.Default"/>).
        /// </summary>
        /// <param name="binder">The action that binds the logger to an underlying logging capability.</param>
        public void RegisterLogger(Action<LoggerArgs> binder)
        {
            if (Logger != null)
                throw new InvalidOperationException("A RegisterLogger has already been performed for the current ExecutionContext instance.");

            Logger = new Logger(binder ?? throw new ArgumentNullException(nameof(binder)));
        }

        /// <summary>
        /// Indicates whether a <see cref="RegisterLogger"/> has been performed for this <see cref="ExecutionContext"/> instance.
        /// </summary>
        public bool HasLogger => Logger != null;

        /// <summary>
        /// Gets or sets the operation type (defaults to <see cref="OperationType.Unspecified"/>).
        /// </summary>
        public OperationType OperationType { get; set; } = OperationType.Unspecified;

        /// <summary>
        /// Gets or sets the user identifier. This value is immutable.
        /// </summary>
        public Guid? UserId
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
        /// Gets or sets the correlation identifier (a unique identifier assigned to the request). This value is immutable.
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

        #region Cache

        /// <summary>
        /// Gets the cached value associated with the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">The cached value where found; otherwise, the default value for the <see cref="Type"/>.</param>
        /// <returns><c>true</c> where found; otherwise, <c>false</c>.</returns>
        public bool TryGetCacheValue<T>(UniqueKey key, out T value)
        {
            if (_caching.IsValueCreated && _caching.Value.TryGetValue(new Tuple<Type, UniqueKey>(typeof(T), key), out object val))
            {
                value = (T)val;
                return true;
            }

            value = default!;
            return false;
        }

        /// <summary>
        /// Sets (adds or overrides) the cache value for the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value to set.</param>
        public void CacheSet<T>(UniqueKey key, T value)
        {
            _caching.Value.AddOrUpdate(new Tuple<Type, UniqueKey>(typeof(T), key), value!, (x, y) => value!);
        }

        /// <summary>
        /// Gets the cached value associated with the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The cached value where found; otherwise, the default value for the <see cref="Type"/>.</returns>
        public T CacheGet<T>(UniqueKey key)
        {
            TryGetCacheValue(key, out T val);
            return val;
        }

        /// <summary>
        /// Removes the cached value associated with the specified <see cref="Type"/> and key.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="key">The key of the value to remove.</param>
        /// <returns><c>true</c> where found and removed; otherwise, <c>false</c>.</returns>
        public bool CacheRemove<T>(UniqueKey key)
        {
            if (_caching.IsValueCreated)
                return _caching.Value.TryRemove(new Tuple<Type, UniqueKey>(typeof(T), key), out object _);
            else
                return false;
        }

        /// <summary>
        /// Clears the cache for the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        public void CacheClear<T>()
        {
            if (!_caching.IsValueCreated)
                return;

            foreach (var item in _caching.Value.Where(x => x.Key.Item1 == typeof(T)).ToList())
            {
                _caching.Value.TryRemove(item.Key, out object val);
            }
        }

        /// <summary>
        /// Clears the cache for all <see cref="Type">types</see>.
        /// </summary>
        public void CacheClearAll()
        {
            if (_caching.IsValueCreated)
                _caching.Value.Clear();
        }

        #endregion

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
        /// <see cref="Current"/> for usage. <i>Note:</i> any new threads <b>must</b> be created from within the <paramref name="action"/>. <b>Warning:</b> this is an advanced feature and should only be
        /// used where this specific capability is required.
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
                Logger = Logger,
                OperationType = OperationType,
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
                if (Originating != null && !ExecutionContext.HasCurrent)
                    ExecutionContext.SetCurrent(Originating.FlowCopy());
            }
        }
    }
}