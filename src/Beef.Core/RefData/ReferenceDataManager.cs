// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.RefData
{
    /// <summary>
    /// Provides a standard mechanism for managing and accessing all the available/possible <b>ReferenceData</b> entities via the <see cref="Current"/> property. 
    /// </summary>
    /// <remarks>
    /// This is required as <b>Entity</b> classes should contain no implementation specific logic, such as database access, etc. This enables the <b>ReferenceData</b> access logic to be seperated from
    /// the implementation. This ensures that the <b>ReferenceData</b> follows the same pattern of using <c>Manager</c>, <c>DataSvc</c> and <c>Data</c> layering for the implementation logic.
    /// <para>This also enables there to be multiple implementations; for example the loading and caching could be different on the external consumer channel versus the internal application tier.</para> 
    /// <para>The <see cref="Register"/> enables overridding of the <c>provider</c> where the default <see cref="ExecutionContext.ServiceProvider"/> (i.e. dependency injection) is not configured.</para>
    /// </remarks>
    public sealed class ReferenceDataManager : IReferenceDataProvider
    {
        private static readonly ConcurrentDictionary<Type, IReferenceDataProvider> _providers = new ConcurrentDictionary<Type, IReferenceDataProvider>();
        private static readonly ConcurrentDictionary<Type, Type> _refEntityToInterface = new ConcurrentDictionary<Type, Type>();

        [ThreadStatic()]
        private static ReferenceDataContext? _context;

        /// <summary>
        /// Registers one or more <see cref="IReferenceDataProvider"/> provider instances.
        /// </summary>
        /// <param name="refDataProviders">The <see cref="IReferenceDataProvider"/> provider instances.</param>
        public static void Register(params IReferenceDataProvider[] refDataProviders)
        {
            if (refDataProviders == null || refDataProviders.Length == 0)
                return;

            foreach (var provider in refDataProviders.Where(p => p != null))
            {
                if (!_providers.TryAdd(provider.ProviderType, provider))
                    throw new ArgumentException($"Provider Type '{provider.ProviderType.FullName}' has already been registered.");
            }
        }

        /// <summary>
        /// Gets the current <see cref="ReferenceDataManager"/> instance. 
        /// </summary>
        public static ReferenceDataManager Current { get; } = new ReferenceDataManager();

        /// <summary>
        /// Gets the thread-based <see cref="ReferenceDataContext"/> (used for the setting of the contextual validation date).
        /// </summary>
        /// <remarks>This property is thread independent (see <see cref="ThreadStaticAttribute"/>).</remarks>
        public static ReferenceDataContext Context
        {
            get
            {
                if (_context == null)
                    _context = new ReferenceDataContext();

                return _context;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataManager"/>.
        /// </summary>
        private ReferenceDataManager() { }

        /// <summary>
        /// Gets the provider <see cref="Type"/>.
        /// </summary>
        Type IReferenceDataProvider.ProviderType => typeof(ReferenceDataManager);

#pragma warning disable CA1043 // Use Integral Or String Argument For Indexers; by-design, Type indexer seems perfectly reasonable.
        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the specified <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>The corresponding <see cref="IReferenceDataCollection"/>.</returns>
        public IReferenceDataCollection this[Type type]
#pragma warning restore CA1043 
        {
            get
            {
                Check.NotNull(type, nameof(type));
                var interfaceType = _refEntityToInterface.GetOrAdd(type, rdt =>
                {
                    var rdi = type.GetCustomAttribute<ReferenceDataInterfaceAttribute>();
                    if (rdi == null || !rdi.InterfaceType.IsInterface)
                        throw new ArgumentException($"Type '{type.Name}' must have the ReferenceDataInterfaceAttribute assigned and the InterfaceType property is an interface.");

                    return rdi.InterfaceType;
                });

                return GetProvider(interfaceType)[type];
            }
        }

        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the specified <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>The corresponding <see cref="IReferenceDataCollection"/>.</returns>
        public IReferenceDataCollection GetByType(Type type) => this[type];

#pragma warning disable CA1822 // Mark members as static; by-design.
        /// <summary>
        /// Gets the <see cref="Register(IReferenceDataProvider[])">registered</see> <see cref="IReferenceDataProvider"/> for the specified <paramref name="providerType"/>.
        /// </summary>
        /// <param name="providerType">The <see cref="IReferenceDataProvider.ProviderType"/>.</param>
        /// <returns>The <see cref="IReferenceDataProvider"/> instance.</returns>
        public IReferenceDataProvider GetProvider(Type providerType)
#pragma warning restore CA1822
        {
            Check.NotNull(providerType, nameof(providerType));

            if (ExecutionContext.HasCurrent && ExecutionContext.Current.ServiceProvider != null)
            {
                var service = ExecutionContext.Current.ServiceProvider.GetService(providerType);
                if (service != null)
                    return (IReferenceDataProvider)service;
            }

            if (_providers.TryGetValue(providerType, out var provider))
                return provider;

            throw new ArgumentException($"Provider with Name '{providerType.FullName}' has not been registered (either using dependency injection (primary) or using the Register method (secondary)). " +
                "For DI this indicates that the ExecutionContext.ServiceProvider property has not been set. For AgentTester this is likely because the AgentTester.TestSetup has not yet been invoked to set, this is needed.");
        }

        /// <summary>
        /// Gets all the underlying <see cref="ReferenceDataBase"/> <see cref="Type">types</see>. This throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>An array of the <see cref="ReferenceDataBase"/> <see cref="Type">types</see>.</returns>
        public Type[] GetAllTypes() => throw new NotSupportedException();

        /// <summary>
        /// Prefetches all of the named <see cref="ReferenceDataBase"/> objects. This throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="names">The list of <see cref="ReferenceDataBase"/> names.</param>
        /// <remarks>Note for implementers; should only fetch where not already cached or expired. This is provided to improve performance for consuming applications to reduce the overhead of
        /// making multiple individual invocations, i.e. reduces chattiness across a potentially high-latency connection.</remarks>
        public Task PrefetchAsync(params string[] names) => throw new NotSupportedException();
    }
}