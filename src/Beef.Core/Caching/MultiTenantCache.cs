// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using System;

namespace Beef.Caching
{
    /// <summary>
    /// Provides multi-tenancy cache management over underlying tenant-based caches using the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/>.
    /// </summary>
    public class MultiTenantCache<TCache> : MultiKeyCache<Guid, TCache>
        where TCache : ICacheCore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTenantCache{TCache}"/> class.
        /// </summary>
        /// <param name="policyKey">The policy key used to determine the cache policy configuration (see <see cref="CachePolicyManager"/>); defaults to <see cref="Guid.NewGuid()"/> ensuring uniqueness.</param>
        /// <param name="createCache">The function that creates the cache for the <b>Tenant</b> <see cref="Guid"/> that should use the passed <see cref="ICachePolicy"/>.</param>
        /// <param name="doNotRegister">Indicates that the automatic <see cref="CachePolicyManager.Register"/> should not occur (inheriting class must perform).</param>
        public MultiTenantCache(Func<Guid, ICachePolicy, TCache> createCache, string? policyKey = null, bool doNotRegister = false) : base(createCache, policyKey, doNotRegister) { }

        /// <summary>
        /// Gets the tenant identifier from the Execution Context.
        /// </summary>
        private Guid GetTenantId()
        {
            if (!ExecutionContext.HasCurrent)
                return Guid.Empty;

            return ExecutionContext.Current.TenantId ?? Guid.Empty;
        }

        /// <summary>
        /// Gets <typeparamref name="TCache"/> using the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/> (automatically creates where not found).
        /// </summary>
        /// <returns>The underlying tenant cache.</returns>
        public TCache GetCache()
        {
            return GetCache(GetTenantId());
        }

        /// <summary>
        /// Gets <typeparamref name="TCache"/> using the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/> where found.
        /// </summary>
        /// <param name="cache">When this method returns, contains the cache associated with the specified key, if the key is found; otherwise, <c>null</c>. This parameter is
        /// passed uninitialized.</param>
        /// <returns><c>true</c> if the tenant was found; otherwise, <c>false</c>.</returns>
        public bool TryGetCache(out TCache cache)
        {
            return TryGetCache(GetTenantId(), out cache);
        }

        /// <summary>
        /// Determines whether the cache contains the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/>.
        /// </summary>
        /// <returns><c>true</c> if the tenant exists; otherwise, <c>false</c>.</returns>
        public bool Contains()
        {
            return Contains(GetTenantId());
        }

        /// <summary>
        /// Removes the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/> from the cache.
        /// </summary>
        public void Remove()
        {
            Remove(GetTenantId());
        }
    }
}