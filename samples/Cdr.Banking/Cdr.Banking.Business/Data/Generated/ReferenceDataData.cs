/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef.Business;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Data.Cosmos;
using RefDataNamespace = Cdr.Banking.Common.Entities;

namespace Cdr.Banking.Business.Data
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> database access.
    /// </summary>
    public partial class ReferenceDataData : IReferenceDataData
    {
        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.OpenStatus"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.OpenStatusCollection"/>.</returns>
        public async Task<RefDataNamespace.OpenStatusCollection> OpenStatusGetAllAsync()
        {
            var __coll = new RefDataNamespace.OpenStatusCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => { CosmosDb.Default.ValueQuery(OpenStatusMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.ProductCategory"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.ProductCategoryCollection"/>.</returns>
        public async Task<RefDataNamespace.ProductCategoryCollection> ProductCategoryGetAllAsync()
        {
            var __coll = new RefDataNamespace.ProductCategoryCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => { CosmosDb.Default.ValueQuery(ProductCategoryMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.AccountUType"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.AccountUTypeCollection"/>.</returns>
        public async Task<RefDataNamespace.AccountUTypeCollection> AccountUTypeGetAllAsync()
        {
            var __coll = new RefDataNamespace.AccountUTypeCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => { CosmosDb.Default.ValueQuery(AccountUTypeMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.MaturityInstructions"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.MaturityInstructionsCollection"/>.</returns>
        public async Task<RefDataNamespace.MaturityInstructionsCollection> MaturityInstructionsGetAllAsync()
        {
            var __coll = new RefDataNamespace.MaturityInstructionsCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => { CosmosDb.Default.ValueQuery(MaturityInstructionsMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.TransactionType"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.TransactionTypeCollection"/>.</returns>
        public async Task<RefDataNamespace.TransactionTypeCollection> TransactionTypeGetAllAsync()
        {
            var __coll = new RefDataNamespace.TransactionTypeCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => { CosmosDb.Default.ValueQuery(TransactionTypeMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.TransactionStatus"/> objects.
        /// </summary>
        /// <returns>A <see cref="RefDataNamespace.TransactionStatusCollection"/>.</returns>
        public async Task<RefDataNamespace.TransactionStatusCollection> TransactionStatusGetAllAsync()
        {
            var __coll = new RefDataNamespace.TransactionStatusCollection();
            await DataInvoker.Default.InvokeAsync(this, async () => { CosmosDb.Default.ValueQuery(TransactionStatusMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.RequiresNewAndTransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.OpenStatus"/> entity and Cosmos <see cref="RefDataNamespace.OpenStatus"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.OpenStatus, RefDataNamespace.OpenStatus> OpenStatusMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.OpenStatus, RefDataNamespace.OpenStatus>()
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.ProductCategory"/> entity and Cosmos <see cref="RefDataNamespace.ProductCategory"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.ProductCategory, RefDataNamespace.ProductCategory> ProductCategoryMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.ProductCategory, RefDataNamespace.ProductCategory>()
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.AccountUType"/> entity and Cosmos <see cref="RefDataNamespace.AccountUType"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.AccountUType, RefDataNamespace.AccountUType> AccountUTypeMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.AccountUType, RefDataNamespace.AccountUType>()
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.MaturityInstructions"/> entity and Cosmos <see cref="RefDataNamespace.MaturityInstructions"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.MaturityInstructions, RefDataNamespace.MaturityInstructions> MaturityInstructionsMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.MaturityInstructions, RefDataNamespace.MaturityInstructions>()
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.TransactionType"/> entity and Cosmos <see cref="RefDataNamespace.TransactionType"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.TransactionType, RefDataNamespace.TransactionType> TransactionTypeMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.TransactionType, RefDataNamespace.TransactionType>()
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.TransactionStatus"/> entity and Cosmos <see cref="RefDataNamespace.TransactionStatus"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.TransactionStatus, RefDataNamespace.TransactionStatus> TransactionStatusMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.TransactionStatus, RefDataNamespace.TransactionStatus>()
            .AddStandardProperties();
    }
}

#pragma warning restore IDE0005
#nullable restore