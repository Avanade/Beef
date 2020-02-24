// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;

namespace Beef.Caching
{
    /// <summary>
    /// Enables the core cache capabilities including extended <see cref="Flush"/> (see <see cref="OnFlushCache"/>) support.
    /// </summary>
    public abstract class CacheCoreBase : ICacheCore
    {
        private readonly ICachePolicy? _policy;
        private bool _disposed;

        /// <summary>
        /// Gets the internal <see cref="PolicyKey"/> string format.
        /// </summary>
        protected const string InternalPolicyKeyFormat = "{0}__INTERNAL__";

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheCoreBase"/> class that automatically <see cref="CachePolicyManager.Register">registers</see> for centralised <see cref="CachePolicyManager.Flush"/> management.
        /// </summary>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="doNotRegister">Indicates that the automatic <see cref="CachePolicyManager.Register"/> should not occur (inheriting class must perform).</param>
        protected CacheCoreBase(string? policyKey = null, bool doNotRegister = false)
        {
            PolicyKey = string.IsNullOrEmpty(policyKey) ? Guid.NewGuid().ToString() : policyKey;
            if (!doNotRegister)
                CachePolicyManager.Register(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheCoreBase"/> class that uses the specified <paramref name="policy"/> versus being managed centrally.
        /// </summary>
        /// <param name="policy">The <see cref="ICachePolicy"/>.</param>
        protected CacheCoreBase(ICachePolicy policy)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            PolicyKey = $"{Guid.NewGuid().ToString()}__NOT_USED__";
        }

        /// <summary>
        /// Gets the policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>).
        /// </summary>
        public string PolicyKey { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ICachePolicy"/> (as specified via the constructor, or from the centralised <see cref="CachePolicyManager"/> using the <see cref="PolicyKey"/>). 
        /// </summary>
        public ICachePolicy GetPolicy() => _policy ?? CachePolicyManager.Get(PolicyKey);

        /// <summary>
        /// Gets the lock object to enable additional locking in a multithreaded context (all internal operations use the lock).
        /// </summary>
        protected object Lock { get; } = new object();

        /// <summary>
        /// Gets the count of items in the cache (both expired and non-expired may co-exist until flushed).
        /// </summary>
        public abstract long Count { get; }

        /// <summary>
        /// Registers an internal <b>Policy Key</b> (using the <see cref="InternalPolicyKeyFormat"/>) with a <see cref="NoCachePolicy"/> to ensure always invoked;
        /// it is then expected that the <see cref="OnFlushCache(bool)"/> will handle policy expiration specifically.
        /// </summary>
        protected void RegisterInternalPolicyKey()
        {
            var overridePolicyKey = string.Format(System.Globalization.CultureInfo.InvariantCulture, InternalPolicyKeyFormat, PolicyKey);
            CachePolicyManager.Register(this, overridePolicyKey);
            CachePolicyManager.Set(overridePolicyKey, new NoCachePolicy());
        }

        /// <summary>
        /// Flush the cache.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        public void Flush(bool ignoreExpiry = false)
        {
            lock (Lock)
            {
                if (ignoreExpiry || GetPolicy().IsExpired)
                    OnFlushCache(ignoreExpiry);
            }
        }

        /// <summary>
        /// Required to be overridden to flush/empty the underlying cache; called by <see cref="Flush"/> within the context of a <see cref="Lock"/>. The <see cref="Flush"/> checks the underlying policy
        /// to detemine whether it has expired; as such the implementing logic should not revalidate this condition (always assume flush) - this value is passed for context should the implementer need to
        /// understand the context in which it was called to perform additional logic.
        /// </summary>
        /// <param name="ignoreExpiry"><c>true</c> indicates to flush immediately; otherwise, <c>false</c> to only flush when the <see cref="ICachePolicy"/> is <see cref="ICachePolicy.IsExpired"/> (default).</param>
        protected abstract void OnFlushCache(bool ignoreExpiry);

        /// <summary>
        /// Releases all resources (see <see cref="Flush(bool)"/>) and invokes the <see cref="CachePolicyManager.Unregister(string)"/> for the <see cref="PolicyKey"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CacheCoreBase"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                lock (Lock)
                {
                    Flush(true);
                    CachePolicyManager.Unregister(PolicyKey);
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~CacheCoreBase()
        {
            Dispose(false);
        }
    }
}
