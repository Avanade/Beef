// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.RefData
{
    /// <summary>
    /// Provides a standard mechanism for managing and accessing the <b>ReferenceData</b> via the <see cref="Current"/> property. 
    /// </summary>
    /// <remarks>
    /// This class provides the interface for the <b>ReferenceData</b> but not the implementation itself. This is required as <b>Entity</b> classes should
    /// contain no implementation specific logic, such as database access, etc. This enables the interface to be seperated from the implementation
    /// and ensures that the <b>ReferenceData</b> follows the same pattern of using <b>Components</b> and <b>Data Services</b> for the implementation logic.
    /// <para>This also enables there to be multiple implementations; for example the loading and caching could be different on the User Interface versus
    /// Application tiers.</para> 
    /// <para><see cref="Register"/> must be invoked to set the providing implementation before accessing the <see cref="Current"/> instance.</para>
    /// </remarks>
    public abstract partial class ReferenceDataManager
    {
        private static ReferenceDataManager _current;

        [ThreadStatic()]
        private static ReferenceDataContext _context;

        #region Create/Default

        /// <summary>
        /// Registers the <see cref="Current"/> <see cref="ReferenceDataManager"/> instance.
        /// </summary>
        /// <param name="refData">The concrete <see cref="ReferenceDataManager"/> instance.</param>
        public static void Register(ReferenceDataManager refData)
        {
            if (_current != null)
                throw new InvalidOperationException("The Register method can only be invoked once.");

            _current = refData ?? throw new ArgumentNullException(nameof(refData));
        }

        /// <summary>
        /// Indicates whether the <see cref="Current"/> has a value.
        /// </summary>
        public static bool HasCurrent { get => _current != null; }

        /// <summary>
        /// Gets the current <see cref="ReferenceDataManager"/> instance. 
        /// </summary>
        public static ReferenceDataManager Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("The Register method must be invoked before this property can be accessed.");

                return _current;
            }
        }

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

        #endregion

        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>The corresponding <see cref="IReferenceDataCollection"/>.</returns>
        public abstract IReferenceDataCollection this[Type type] { get; }

        /// <summary>
        /// Gets all the underlying <see cref="ReferenceDataBase"/> <see cref="Type">types</see>.
        /// </summary>
        /// <returns>An array of the <see cref="ReferenceDataBase"/> <see cref="Type">types</see>.</returns>
        public abstract Type[] GetAllTypes();

        /// <summary>
        /// Prefetches all of the named <see cref="ReferenceDataBase"/> objects. 
        /// </summary>
        /// <param name="names">The list of <see cref="ReferenceDataBase"/> names.</param>
        /// <remarks>Note for implementers; should only fetch where not already cached or expired. This is provided to improve performance
        /// for consuming applications to reduce the overhead of making multiple individual invocations, i.e. reduces chattiness across a 
        /// potentially high-latency connection.</remarks>
        public abstract Task PrefetchAsync(params string[] names);
    }
}