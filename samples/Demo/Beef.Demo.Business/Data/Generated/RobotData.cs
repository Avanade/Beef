/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Beef;
using Beef.Business;
using Beef.Data.Cosmos;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Robot"/> data access.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Will not always appear static depending on code-gen options")]
    public partial class RobotData : IRobotData
    {
        private readonly ICosmosDb _cosmos;

        #region Extensions
        #pragma warning disable CS0649, IDE0044 // Defaults to null by design; can be overridden in constructor.

        private Action<ICosmosDbArgs>? _onDataArgsCreate;
        private Func<IQueryable<Model.Robot>, RobotArgs?, ICosmosDbArgs, IQueryable<Model.Robot>>? _getByArgsOnQuery;

        #pragma warning restore CS0649, IDE0044
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotData"/> class.
        /// </summary>
        /// <param name="cosmos">The <see cref="ICosmosDb"/>.</param>
        public RobotData(ICosmosDb cosmos)
            { _cosmos = Check.NotNull(cosmos, nameof(cosmos)); RobotDataCtor(); }

        partial void RobotDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <returns>The selected <see cref="Robot"/> where found.</returns>
        public Task<Robot?> GetAsync(Guid id)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                var __dataArgs = CosmosMapper.Default.CreateArgs("Items", PartitionKey.None, onCreate: _onDataArgsCreate);
                return await _cosmos.Container(__dataArgs).GetAsync(id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Creates a new <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <returns>The created <see cref="Robot"/>.</returns>
        public Task<Robot> CreateAsync(Robot value)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                var __dataArgs = CosmosMapper.Default.CreateArgs("Items", PartitionKey.None, onCreate: _onDataArgsCreate);
                return await _cosmos.Container(__dataArgs).CreateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Updates an existing <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <returns>The updated <see cref="Robot"/>.</returns>
        public Task<Robot> UpdateAsync(Robot value)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                var __dataArgs = CosmosMapper.Default.CreateArgs("Items", PartitionKey.None, onCreate: _onDataArgsCreate);
                return await _cosmos.Container(__dataArgs).UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Deletes the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        public Task DeleteAsync(Guid id)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                var __dataArgs = CosmosMapper.Default.CreateArgs("Items", PartitionKey.None, onCreate: _onDataArgsCreate);
                await _cosmos.Container(__dataArgs).DeleteAsync(id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gets the <see cref="RobotCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.RobotArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="RobotCollectionResult"/>.</returns>
        public Task<RobotCollectionResult> GetByArgsAsync(RobotArgs? args, PagingArgs? paging)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                RobotCollectionResult __result = new RobotCollectionResult(paging);
                var __dataArgs = CosmosMapper.Default.CreateArgs("Items", __result.Paging!, PartitionKey.None, onCreate: _onDataArgsCreate);
                __result.Result = _cosmos.Container(__dataArgs).Query(q => _getByArgsOnQuery?.Invoke(q, args, __dataArgs) ?? q).SelectQuery<RobotCollection>();
                return await Task.FromResult(__result).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Provides the <see cref="Robot"/> and Cosmos <see cref="Model.Robot"/> property mapping.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "By design; as there is a direct relationship")]
        public partial class CosmosMapper : CosmosDbMapper<Robot, Model.Robot, CosmosMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CosmosMapper"/> class.
            /// </summary>
            public CosmosMapper()
            {
                Property(s => s.Id, d => d.Id).SetUniqueKey(false);
                Property(s => s.ModelNo, d => d.ModelNo);
                Property(s => s.SerialNo, d => d.SerialNo);
                Property(s => s.EyeColorSid, d => d.EyeColor);
                Property(s => s.PowerSourceSid, d => d.PowerSource);
                AddStandardProperties();
                CosmosMapperCtor();
            }
            
            partial void CosmosMapperCtor(); // Enables the CosmosMapper constructor to be extended.
        }
    }
}

#pragma warning restore IDE0005
#nullable restore