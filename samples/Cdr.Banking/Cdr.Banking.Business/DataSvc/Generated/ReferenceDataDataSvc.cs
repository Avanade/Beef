/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Beef;
using Beef.Business;
using Beef.RefData;
using Beef.RefData.Caching;
using Cdr.Banking.Business.Data;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.DataSvc
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> data services.
    /// </summary>
    public partial class ReferenceDataDataSvc : IReferenceDataDataSvc
    {
        private readonly IServiceProvider _provider;
        private readonly Dictionary<Type, IReferenceDataCache> _cacheDict = new Dictionary<Type, IReferenceDataCache>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataDataSvc" /> class.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/>.</param>
        public ReferenceDataDataSvc(IServiceProvider provider)
        {
            _provider = Check.NotNull(provider, nameof(provider));
            _cacheDict.Add(typeof(RefDataNamespace.OpenStatus), new ReferenceDataCache<RefDataNamespace.OpenStatusCollection, RefDataNamespace.OpenStatus>(() => DataSvcInvoker.Current.InvokeAsync(typeof(ReferenceDataDataSvc), () => GetDataAsync(data => data.OpenStatusGetAllAsync()))));
            _cacheDict.Add(typeof(RefDataNamespace.ProductCategory), new ReferenceDataCache<RefDataNamespace.ProductCategoryCollection, RefDataNamespace.ProductCategory>(() => DataSvcInvoker.Current.InvokeAsync(typeof(ReferenceDataDataSvc), () => GetDataAsync(data => data.ProductCategoryGetAllAsync()))));
            _cacheDict.Add(typeof(RefDataNamespace.AccountUType), new ReferenceDataCache<RefDataNamespace.AccountUTypeCollection, RefDataNamespace.AccountUType>(() => DataSvcInvoker.Current.InvokeAsync(typeof(ReferenceDataDataSvc), () => GetDataAsync(data => data.AccountUTypeGetAllAsync()))));
            _cacheDict.Add(typeof(RefDataNamespace.MaturityInstructions), new ReferenceDataCache<RefDataNamespace.MaturityInstructionsCollection, RefDataNamespace.MaturityInstructions>(() => DataSvcInvoker.Current.InvokeAsync(typeof(ReferenceDataDataSvc), () => GetDataAsync(data => data.MaturityInstructionsGetAllAsync()))));
            _cacheDict.Add(typeof(RefDataNamespace.TransactionType), new ReferenceDataCache<RefDataNamespace.TransactionTypeCollection, RefDataNamespace.TransactionType>(() => DataSvcInvoker.Current.InvokeAsync(typeof(ReferenceDataDataSvc), () => GetDataAsync(data => data.TransactionTypeGetAllAsync()))));
            _cacheDict.Add(typeof(RefDataNamespace.TransactionStatus), new ReferenceDataCache<RefDataNamespace.TransactionStatusCollection, RefDataNamespace.TransactionStatus>(() => DataSvcInvoker.Current.InvokeAsync(typeof(ReferenceDataDataSvc), () => GetDataAsync(data => data.TransactionStatusGetAllAsync()))));
            ReferenceDataDataSvcCtor();
        }

        partial void ReferenceDataDataSvcCtor(); // Enables the ReferenceDataDataSvc constructor to be extended.

        /// <summary>
        /// Gets the data within a new scope; each reference data request needs to occur separately and independently.
        /// </summary>
        private async Task<T> GetDataAsync<T>(Func<IReferenceDataData, Task<T>> func)
        {
            using var scope = _provider.CreateScope();
            return await func(scope.ServiceProvider.GetService<IReferenceDataData>()).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="ReferenceDataBase"/> type associated </param>
        /// <returns>A <see cref="IReferenceDataCollection"/>.</returns>
        public IReferenceDataCollection GetCollection(Type type) =>
            _cacheDict.TryGetValue(type ?? throw new ArgumentNullException(nameof(type)), out var rdc) ? rdc.GetCollection() :
                throw new ArgumentException($"Type {type.Name} does not exist within the ReferenceDataDataSvc cache.", nameof(type));
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore