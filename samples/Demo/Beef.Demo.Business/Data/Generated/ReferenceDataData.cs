/*
 * This file is automatically generated; any changes will be lost. 
 */
 
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef.Business;
using Beef.Mapper.Converters;
using RefDataNamespace = Beef.Demo.Common.Entities;
using Beef.Data.Database;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> database access.
    /// </summary>
    public partial class ReferenceDataData : IReferenceDataData
    {
        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Gender"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.GenderCollection"/>.</returns>
        public async Task<RefDataNamespace.GenderCollection> GenderGetAllAsync()
        {
            var __coll = new RefDataNamespace.GenderCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => 
            {
                Database.Default.GetRefData<RefDataNamespace.GenderCollection, RefDataNamespace.Gender>(__coll, "[Ref].[spGenderGetAll]", "GenderId", additionalProperties: (dr, item, fields) =>
                {
                });
                await Task.Delay(0);
            }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress);

            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Company"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.CompanyCollection"/>.</returns>
        public async Task<RefDataNamespace.CompanyCollection> CompanyGetAllAsync()
        {
            var __coll = new RefDataNamespace.CompanyCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => await this.CompanyGetAll_OnImplementation(__coll), BusinessInvokerArgs.RequiresNewAndTransactionSuppress);
            return __coll;
        }
    }
}
