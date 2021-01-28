/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beef;
using Beef.RefData;
using Beef.Demo.Business.DataSvc;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business
{
    /// <summary>
    /// Provides the <see cref="ReferenceData"/> implementation using the corresponding data services.
    /// </summary>
    public partial class ReferenceDataProvider : RefDataNamespace.ReferenceData
    {
        private readonly IReferenceDataDataSvc _dataService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataProvider"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="IReferenceDataDataSvc"/>.</param>
        public ReferenceDataProvider(IReferenceDataDataSvc dataService) { _dataService = Check.NotNull(dataService, nameof(dataService)); ReferenceDataProviderCtor(); }

        partial void ReferenceDataProviderCtor(); // Enables the ReferenceDataProvider constructor to be extended.
        
        #region Collections

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.CountryCollection"/>.
        /// </summary>
        public override RefDataNamespace.CountryCollection Country => (RefDataNamespace.CountryCollection)this[typeof(RefDataNamespace.Country)];

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.USStateCollection"/>.
        /// </summary>
        public override RefDataNamespace.USStateCollection USState => (RefDataNamespace.USStateCollection)this[typeof(RefDataNamespace.USState)];

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.GenderCollection"/>.
        /// </summary>
        public override RefDataNamespace.GenderCollection Gender => (RefDataNamespace.GenderCollection)this[typeof(RefDataNamespace.Gender)];

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.EyeColorCollection"/>.
        /// </summary>
        public override RefDataNamespace.EyeColorCollection EyeColor => (RefDataNamespace.EyeColorCollection)this[typeof(RefDataNamespace.EyeColor)];

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.PowerSourceCollection"/>.
        /// </summary>
        public override RefDataNamespace.PowerSourceCollection PowerSource => (RefDataNamespace.PowerSourceCollection)this[typeof(RefDataNamespace.PowerSource)];

        /// <summary> 
        /// Gets the <see cref="RefDataNamespace.CompanyCollection"/>.
        /// </summary>
        public override RefDataNamespace.CompanyCollection Company => (RefDataNamespace.CompanyCollection)this[typeof(RefDataNamespace.Company)];

        #endregion

        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>A <see cref="IReferenceDataCollection"/>.</returns>
        public override IReferenceDataCollection this[Type type] => _dataService.GetCollection(type);
        
        /// <summary>
        /// Prefetches all, or the list of <see cref="ReferenceDataBase"/> objects, where not already cached or expired.
        /// </summary>
        /// <param name="names">The list of <see cref="ReferenceDataBase"/> names; otherwise, <c>null</c> for all.</param>
        public override Task PrefetchAsync(params string[] names)
        {
            var types = new List<Type>();
            if (names == null)
            {
                types.AddRange(GetAllTypes());
            }
            else
            {
                foreach (string name in names.Distinct())
                {
                    switch (name)
                    {
                        case var n when string.Compare(n, nameof(RefDataNamespace.Country), StringComparison.InvariantCultureIgnoreCase) == 0: types.Add(typeof(RefDataNamespace.Country)); break;
                        case var n when string.Compare(n, nameof(RefDataNamespace.USState), StringComparison.InvariantCultureIgnoreCase) == 0: types.Add(typeof(RefDataNamespace.USState)); break;
                        case var n when string.Compare(n, nameof(RefDataNamespace.Gender), StringComparison.InvariantCultureIgnoreCase) == 0: types.Add(typeof(RefDataNamespace.Gender)); break;
                        case var n when string.Compare(n, nameof(RefDataNamespace.EyeColor), StringComparison.InvariantCultureIgnoreCase) == 0: types.Add(typeof(RefDataNamespace.EyeColor)); break;
                        case var n when string.Compare(n, nameof(RefDataNamespace.PowerSource), StringComparison.InvariantCultureIgnoreCase) == 0: types.Add(typeof(RefDataNamespace.PowerSource)); break;
                        case var n when string.Compare(n, nameof(RefDataNamespace.Company), StringComparison.InvariantCultureIgnoreCase) == 0: types.Add(typeof(RefDataNamespace.Company)); break;
                    }
                }
            }

            Parallel.ForEach(types, (type, _) => { var __ = this[type]; });
            return Task.CompletedTask;
        }
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore