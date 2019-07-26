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
    /// <remarks>Used to house/pass context parameters and capabilities that are outside of the general operation arguments. By default uses <see cref="AsyncLocal{T}"/>,
    /// although this can be overridden (see <see cref="Register(Func{ExecutionContext}, Func{ExecutionContext}, Action{ExecutionContext})"/>).</remarks>
    public class ExecutionContext : IETag
    {
        private static readonly object _masterLock = new object();
        private static Func<ExecutionContext> _create = () => new ExecutionContext();
        private static AsyncLocal<ExecutionContext> _asyncLocal = new AsyncLocal<ExecutionContext>();
        private static Func<ExecutionContext> _get = () => _asyncLocal.Value;
        private static Action<ExecutionContext> _set = (ec) => _asyncLocal.Value = ec;
        private string _username;
        private Guid? _tenantId;
        private string _partitionKey;
        private string _correlationId;
        private string _sessionCorrelationId;
        private DateTime _timestamp = DateTime.Now;
        private bool _timestampChanged;
        private PagingArgs _pagingArgs;
        private KeyOnlyDictionary<string> _roles;
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
        /// Sets the <see cref="Current"/> instance (only allowed where there is no <b>Current</b> instance and no <see cref="Register(Func{ExecutionContext})"/> has been configured).
        /// </summary>
        /// <param name="executionContext">The <see cref="ExecutionContext"/> instance.</param>
        public static void SetCurrent(ExecutionContext executionContext)
        {
            Check.NotNull(executionContext, nameof(executionContext));
            if (HasCurrent)
                throw new InvalidOperationException("The SetCurrent method can only be used where there is no Current instance.");

            if (HasBeenRegistered)
                throw new InvalidOperationException("The SetCurrent method can only be used where there is on Register function configured.");

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
        public static void Register(Func<ExecutionContext> create, Func<ExecutionContext> get, Action<ExecutionContext> set)
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
        /// Gets or sets the current <see cref="Beef.DataContextScope"/>.
        /// </summary>
        internal DataContextScope DataContextScope { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="Beef.Diagnostics.Logger"/>.
        /// </summary>
        internal Logger Logger { get; set; }

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
        /// Gets or sets the username for the request. This value is immutable.
        /// </summary>
        public string Username
        {
            get { return _username; }

            set
            {
                if (_username != null && value != _username)
                    throw new ArgumentException(ImmutableText);

                _username = value;
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
        public string PartitionKey
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
        public string CorrelationId
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
        public string SessionCorrelationId
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

                _timestamp = value;
                _timestampChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs"/>. This value is immutable.
        /// </summary>
        public PagingArgs PagingArgs
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
        /// Gets or sets the entity tag (where value does not support <see cref="IETag"/>).
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// Gets the <see cref="MessageItemCollection"/> to be passed back to the originating consumer.
        /// </summary>
        public MessageItemCollection Messages { get => _messages.Value; }

        /// <summary>
        /// Gets the properties <see cref="Dictionary{TKey, TValue}"/> for passing/storing additional data.
        /// </summary>
        public Dictionary<string, object> Properties { get => _properties.Value; }

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

            value = default;
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
            _caching.Value.AddOrUpdate(new Tuple<Type, UniqueKey>(typeof(T), key), value, (x, y) => value);
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
                return _caching.Value.TryRemove(new Tuple<Type, UniqueKey>(typeof(T), key), out object val);
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
            return _roles?.Select(x => x.Key).ToArray();
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
    }   
}