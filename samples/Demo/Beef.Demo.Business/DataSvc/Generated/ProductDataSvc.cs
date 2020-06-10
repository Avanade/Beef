/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Entities;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.DataSvc
{
    /// <summary>
    /// Provides the Product data repository services.
    /// </summary>
    public static partial class ProductDataSvc
    {
        #region Private
        #pragma warning disable CS0649 // Defaults to null by design; can be overridden in constructor.

        private static readonly Func<Product?, int, Task>? _getOnAfterAsync;
        private static readonly Func<ProductCollectionResult, ProductArgs?, PagingArgs?, Task>? _getByArgsOnAfterAsync;

        #pragma warning restore CS0649
        #endregion

        /// <summary>
        /// Gets the <see cref="Product"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Product"/> identifier.</param>
        /// <returns>The selected <see cref="Product"/> object where found; otherwise, <c>null</c>.</returns>
        public static Task<Product?> GetAsync(int id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(ProductDataSvc), async () => 
            {
                var __key = new UniqueKey(id);
                if (ExecutionContext.Current.TryGetCacheValue<Product>(__key, out Product __val))
                    return __val;

                var __result = await Factory.Create<IProductData>().GetAsync(id).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__key, __result!);
                if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="Product"/> collection object that matches the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="ProductArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="ProductCollectionResult"/>.</returns>
        public static Task<ProductCollectionResult> GetByArgsAsync(ProductArgs? args, PagingArgs? paging)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(ProductDataSvc), async () => 
            {
                var __result = await Factory.Create<IProductData>().GetByArgsAsync(args, paging).ConfigureAwait(false);
                if (_getByArgsOnAfterAsync != null) await _getByArgsOnAfterAsync(__result, args, paging).ConfigureAwait(false);
                return __result;
            });
        }
    }
}

#pragma warning restore IDE0005
#nullable restore